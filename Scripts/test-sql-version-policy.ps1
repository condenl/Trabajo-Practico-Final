# Regression test of the actual guards, without connecting or restoring databases.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

function Get-VersionGuard {
    param([string]$RelativePath, [string]$Variable)
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile((Join-Path $root $RelativePath), [ref]$tokens, [ref]$errors)
    if ($errors.Count) { throw "Syntax errors in $RelativePath" }
    $guards = @($ast.FindAll({
        param($node)
        $node -is [System.Management.Automation.Language.IfStatementAst] -and
        $node.Clauses[0].Item1.Extent.Text -match ('^\s*(\[int\])?\$' + $Variable + '\s+-\w+\s+\d+\s*$')
    }, $true))
    if ($guards.Count -ne 1) { throw "Expected one version guard in $RelativePath" }
    return [scriptblock]::Create($guards[0].Clauses[0].Item1.Extent.Text)
}

$prerequisiteGuard = Get-VersionGuard 'Installer/VerificarPrerrequisitos.ps1' 'sqlMajor'
$initializerGuard = Get-VersionGuard 'Installer/InicializarBaseDatos.ps1' 'majorVersion'
$seed = Get-Content (Join-Path $root 'Scripts/SeedDemoActors.cs') -Raw -Encoding UTF8
$matches = [regex]::Matches($seed, 'Require\(\(int\)server\[1\].*?;')
if ($matches.Count -ne 1) { throw 'Expected one SQL version requirement in SeedDemoActors.cs' }
# Compile the exact production C# statement with a test Require implementation.
Add-Type -TypeDefinition @"
public static class SeedVersionPolicyProbe {
    private static void Require(bool ok, string message) {
        if (!ok) throw new System.InvalidOperationException(message);
    }
    public static bool Accepts(int version) {
        object[] server = { "local", version };
        try { $($matches[0].Value) return true; }
        catch (System.InvalidOperationException) { return false; }
    }
}
"@

foreach ($version in @(0, 15, 16, 17, 18)) {
    $sqlMajor = $version
    $majorVersion = $version
    $expected = $version -ge 16
    $results = @((-not (& $prerequisiteGuard)), (-not (& $initializerGuard)), [SeedVersionPolicyProbe]::Accepts($version))
    foreach ($accepted in $results) {
        if ($accepted -ne $expected) { throw "Version ${version}: expected acceptance=$expected, actual=$accepted" }
    }
    Write-Output "PASS SQL $version - prerequisites, initialization and demo seed: accepted=$expected"
}
