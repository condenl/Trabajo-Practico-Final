using System;

internal static class SeedDemoTargetPolicy
{
    internal static bool IsLocalTarget(string dataSource, string sqlMachine, bool isLocalDb, string clientMachine)
    {
        // LocalDB uses its own endpoint/metadata; MachineName alone is not reliable here.
        const string prefix = @"(localdb)\";
        string source = (dataSource ?? "").Trim();
        if (isLocalDb)
            return source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && source.Length > prefix.Length;
        return !string.IsNullOrWhiteSpace(sqlMachine) &&
            string.Equals(sqlMachine, clientMachine, StringComparison.OrdinalIgnoreCase);
    }
}
