# Instalador inicial A01

Este directorio implementa el entregable **A01 de la Iteracion 2** siguiendo la guia provista para instaladores `.exe` en C#.

## Alcance

- instala los binarios Release de la aplicacion en `Program Files`;
- crea accesos directos en el menu Inicio y, opcionalmente, en el escritorio;
- solicita la instancia de SQL Server;
- verifica .NET Framework 4.7.2 o superior antes de copiar la aplicacion;
- verifica que la instancia indicada sea SQL Server 16.x (SQL Server 2022);
- si falta .NET, lo descarga desde Microsoft en segundo plano, valida su firma digital y lo instala silenciosamente;
- si falta SQL Server, intenta descargar el bootstrapper firmado de SQL Server 2022 Express y, si Microsoft retira temporalmente el enlace versionado, abre la pagina oficial de descarga;
- restaura la base inicial si todavia no existe;
- ejecuta las migraciones idempotentes de N01 y N02;
- actualiza `UI.exe.config` con la instancia elegida y autenticacion integrada;
- conserva un registro en `Installer\instalacion-base-datos.log`.

Los prerrequisitos descargados se guardan en `%APPDATA%\Perfumeria\Prerrequisitos`. El motor SQL no se instala silenciosamente: su configuracion requiere aceptar los terminos de Microsoft y elegir una instancia. El instalador de Perfumeria se reanuda al volver a ejecutarlo una vez instalado SQL Server 2022.

## Generacion del `.exe`

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

Ejecutar el instalador en una maquina virtual con y sin los prerrequisitos. Confirmar que detecta .NET Framework 4.7.2+, rechaza instancias distintas de SQL Server 16.x y facilita la descarga si falta una dependencia. Luego confirmar que se puede iniciar sesion, registrar una compra, aprobarla con Contabilidad, recibir un lote y registrar una venta con descuento de membresia.
