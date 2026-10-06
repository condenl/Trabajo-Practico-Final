# Instalador inicial A01

Este directorio implementa el entregable **A01 de la Iteracion 2** siguiendo la guia provista para instaladores `.exe` en C#.

## Alcance

- instala los binarios Release de la aplicacion en `Program Files`;
- crea accesos directos en el menu Inicio y, opcionalmente, en el escritorio;
- solicita la instancia de SQL Server;
- verifica .NET Framework 4.7.2 o superior antes de copiar la aplicacion;
- verifica que la instancia indicada sea SQL Server 16.x (SQL Server 2022) o superior, incluida 17.x (SQL Server 2025);
- si falta .NET, lo descarga desde Microsoft en segundo plano, valida su firma digital y lo instala silenciosamente;
- si falta SQL Server, intenta descargar el bootstrapper firmado de SQL Server 2022 Express y, si Microsoft retira temporalmente el enlace versionado, abre la pagina oficial de descarga;
- restaura la base inicial si todavia no existe;
- ejecuta las migraciones idempotentes de N01 y N02;
- actualiza `UI.exe.config` con la instancia elegida y autenticacion integrada;
- conserva un registro en `Installer\instalacion-base-datos.log`.

Los prerrequisitos descargados se guardan en `%APPDATA%\Perfumeria\Prerrequisitos`. El motor SQL no se instala silenciosamente: su configuracion requiere aceptar los terminos de Microsoft y elegir una instancia. El instalador de Perfumeria se reanuda al volver a ejecutarlo una vez instalado SQL Server 2022.

## Generacion del `.exe`

Para incluir los actores demo y el README con las credenciales, usar ahora:

```powershell
powershell -NoProfile -File Installer/build-installer.ps1
```

El script compila UI y `SeedDemoActors.exe` antes de llamar a Inno Setup. La opción **Cargar actores de demostración** está desmarcada inicialmente. Al marcarla, las cuatro cuentas documentadas en el [README principal](../README.md) se crean en la misma base que utiliza el EXE instalado; no dependen de Visual Studio ni del equipo de desarrollo. Requiere una instancia local SQL Server 16.x o superior. Las claves son públicas y solo deben usarse en pruebas.

La precarga es transaccional e idempotente y verifica el login y los permisos. No modifica credenciales o roles existentes. Inicializa los dígitos verificadores del respaldo de distribución únicamente si la base acaba de ser restaurada por esa ejecución; ante errores de integridad en una base preexistente se detiene. El instalador comprueba el código de salida de la configuración y no ofrece iniciar la aplicación si falla.

Las instrucciones manuales siguientes requieren haber generado previamente `Installer/bin/SeedDemoActors.exe` con el script anterior.

1. Compilar la solucion en modo Release:

   ```powershell
   dotnet msbuild "Trabajo Practico Final.sln" /t:Build /p:Configuration=Release /m
   ```

2. Instalar Inno Setup 6 en el equipo de generacion.
3. Compilar `Perfumeria.iss` desde Inno Setup o con:

   ```powershell
   & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "Installer\Perfumeria.iss"
   ```

El resultado se genera en `Installer\Output\Perfumeria_Setup_Iteracion2.exe`.

## Prueba recomendada

Ejecutar el instalador en una maquina virtual con y sin los prerrequisitos. Confirmar que detecta .NET Framework 4.7.2+, acepta SQL Server 16.x y 17.x, rechaza versiones anteriores a 16 y facilita la descarga si falta una dependencia. Luego confirmar que se puede iniciar sesion, registrar una compra, aprobarla con Contabilidad, recibir un lote y registrar una venta con descuento de membresia.

La version se consulta en la instancia indicada, no en SSMS. Si ya tiene SQL Server 17.x, indicar su nombre correcto (`.` para la predeterminada o `.\NOMBRE` para una instancia con nombre); no necesita instalar 16.x adicionalmente. Una instancia detenida o sin permisos de conexion no se puede verificar.

Se conserva el respaldo de distribucion generado con SQL Server 16 para las computadoras de la facultad. Microsoft admite [restaurarlo en SQL Server 17](https://learn.microsoft.com/en-us/sql/database-engine/install-windows/supported-version-and-edition-upgrades-2025?view=sql-server-ver17). No reemplazarlo por un respaldo generado en 17 si debe seguir instalando en 16: los respaldos no son compatibles hacia versiones anteriores.
