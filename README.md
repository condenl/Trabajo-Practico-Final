# Perfumería

Aplicación de escritorio C# (.NET Framework 4.7.2) con SQL Server **16.x (SQL Server 2022) o superior**, incluida la versión **17.x (SQL Server 2025)**. Las cuentas de acceso pertenecen a la base de datos; funcionan tanto desde Visual Studio como desde `UI.exe` y la aplicación instalada.

## Usuarios de demostración

Credenciales públicas y deliberadamente conocidas, **solo para pruebas y entregas académicas**. No habilitar estos datos en una base de producción ni reutilizar estas contraseñas para cuentas reales. El campo Usuario del login recibe el correo completo.

| Actor operativo | Usuario | Contraseña | Alcance |
| --- | --- | --- | --- |
| Dueño / Compras | `demo.dueno@perfumeria.example` | `DemoDueno2026!` | Proveedores, productos, cotizaciones, órdenes, recepción y ventas. No aprueba compras. |
| Equipo de Contabilidad | `demo.contabilidad@perfumeria.example` | `DemoContabilidad2026!` | Revisar, aprobar o rechazar órdenes pendientes. |
| Depósito / Recepción | `demo.deposito@perfumeria.example` | `DemoDeposito2026!` | Recibir órdenes emitidas, consultar stock y trazabilidad. |
| Ventas / Fidelización | `demo.ventas@perfumeria.example` | `DemoVentas2026!` | Registrar clientes, membresías y ventas. |

Los perfiles no son administradores de seguridad y no reciben permisos de gestión de usuarios o roles. Las operaciones no autorizadas deben permanecer ocultas o rechazarse. Las cuentas se crean activas y sin intentos fallidos; cuatro contraseñas incorrectas pueden bloquearlas según las reglas normales de la aplicación. La precarga no restablece contraseñas, no desbloquea cuentas ni cambia permisos preexistentes.

Proveedores y clientes son actores externos, **sin usuario/contraseña de acceso** en las iteraciones actuales. Se incluyen estos registros ficticios:

| Actor externo | Identificador de prueba | Datos |
| --- | --- | --- |
| Proveedor 1 | `DEMO Proveedor 1`, CUIT ficticio `99000000001` | Contacto `demo.proveedor1@perfumeria.example`, antigüedad 10, prestigio 90. |
| Proveedor 2 | `DEMO Proveedor 2`, CUIT ficticio `99000000002` | Contacto `demo.proveedor2@perfumeria.example`, antigüedad 5, prestigio 80. |
| Cliente | `demo.cliente@perfumeria.example` | Cliente DEMO, membresía `DEMO-2026-001`, descuento 10 %, sin consentimiento de marketing. |

No se generan compras, ventas ni stock automáticamente. Para probar el circuito completo: con Dueño registrar un producto y dos cotizaciones vigentes; crear y enviar la orden; con Contabilidad aprobarla; con Dueño emitirla; con Depósito recibir los lotes; con Ventas buscar al cliente demo y venderle stock recibido con su descuento.

## Usarlas con el instalador EXE en otra computadora

1. Ejecutar `Installer/Output/Perfumeria_Setup_Iteracion2.exe` e indicar la instancia local SQL Server 16.x o superior (por ejemplo, 17.x).
2. Marcar **Cargar actores de demostración (claves públicas del README; solo pruebas)**. La opción está desmarcada inicialmente.
3. El instalador prepara la base `Trabajo Practico Final`, carga los actores, verifica sus accesos y configura `UI.exe.config` con esa misma base.
4. Abrir **Perfumería** desde el acceso directo e ingresar con cualquiera de las cuatro cuentas de la tabla. No hace falta Visual Studio ni ejecutar scripts manualmente.

Estas son cuentas de la aplicación, no logins de SQL Server ni de Windows. La conexión SQL continúa usando autenticación integrada de Windows; la identidad que ejecuta la aplicación necesita acceso a la base. Copiar solo `UI.exe` no copia las cuentas: hay que instalar/preparar la base y conservar las DLL y `UI.exe.config`. El README también se incluye en la carpeta instalada.

Si la base ya existe, se conserva. Si hay conflictos con las cuentas demo o errores previos de integridad, la precarga falla sin sobrescribirlos y el instalador informa el error. Solo cuando acaba de restaurar una base nueva del respaldo distribuido, inicializa DVH/DVV antes de cargar los actores. Revisar `Installer/instalacion-base-datos.log`. No es una función de reparación de bases existentes.

### LocalDB y recuperación de la instalación fallida

Se admite `(localdb)\MSSQLLocalDB` cuando su motor es 16 o superior. La comprobación usa tanto el endpoint local como `SERVERPROPERTY('IsLocalDB')`; no depende de que el nombre de equipo informado por SQL coincida con Windows. La precarga sigue rechazando servidores remotos y ahora valida el destino antes de restaurar. LocalDB pertenece al usuario de Windows: instalar y ejecutar la aplicación con la misma cuenta, sin cambiar a otro usuario administrador.

Si una versión anterior restauró la base y luego falló con **«Solo se permite una instancia local de desarrollo»**, la base ya existe pero sus dígitos verificadores iniciales podrían no haberse preparado. No hay que borrarla ni restaurar encima. **Solo si esa base acaba de salir de esa instalación fallida y todavía no se utilizó**, seguir estos pasos:

1. Ejecutar el instalador actualizado sin marcar **Cargar actores de demostración** ni **Ejecutar Perfumería**, para actualizar los binarios y conservar la base.
2. En PowerShell elevado como administrador **con la misma cuenta de Windows**, desde la carpeta instalada (por defecto `C:\Program Files\Perfumeria`), ejecutar lo siguiente. Confirmar antes que `UI.exe.config` apunte a la instancia y base de esa instalación fallida:

   ```powershell
   Copy-Item .\UI.exe.config .\SeedDemoActors.exe.config
   .\SeedDemoActors.exe --check-target "Trabajo Practico Final"
   if ($LASTEXITCODE -ne 0) { throw 'Destino incorrecto; detener la recuperacion.' }
   .\SeedDemoActors.exe --initialize-new "Trabajo Practico Final"
   if ($LASTEXITCODE -ne 0) { throw 'La inicializacion fallo; revisar el mensaje y no continuar.' }
   .\SeedDemoActors.exe --verify "Trabajo Practico Final"
   if ($LASTEXITCODE -ne 0) { throw 'La verificacion fallo; revisar el mensaje.' }
   ```

3. Abrir Perfumería cuando se informen los cuatro accesos correctos. Un reintento normal del instalador con datos demo también debe terminar sin duplicarlos.

La inicialización explícita recalcula DVH/DVV de los usuarios restaurados y crea los actores demo en una transacción. **No usarla en una base que ya tenga uso real o cambios que se deban auditar**: podría ocultar una alteración de integridad. En ese caso conservar un respaldo y diagnosticar la discrepancia antes de continuar. El instalador nunca selecciona este modo automáticamente para una base preexistente.

## Precarga sobre la base de desarrollo

La base local utilizada en esta sesión es `Trabajo Practico Final 3` en `DESKTOP-TFHVP49`; no es el nombre fijo del instalador. El script toma la conexión de `UI/App.config` y exige que coincida con el nombre indicado. Compilar primero `UI/UI.csproj` en Release.

```powershell
# Prevalidación de solo lectura (opción predeterminada).
powershell -NoProfile -File Scripts/seed-demo-actors.ps1 -Database "Trabajo Practico Final 3" -Mode check
# Crear actores y perfiles faltantes en una transacción.
powershell -NoProfile -File Scripts/seed-demo-actors.ps1 -Database "Trabajo Practico Final 3" -Mode apply
# Probar el Login real, todos los permisos y la integridad DVH/DVV.
powershell -NoProfile -File Scripts/seed-demo-actors.ps1 -Database "Trabajo Practico Final 3" -Mode verify
```

El seed solo admite una instancia SQL local 16.x o superior con autenticación integrada. Puede ejecutarse en otras computadoras: no depende del nombre de este equipo. Usa el cifrado, hash y cálculo de integridad de la aplicación. Repetirlo con datos coincidentes no duplica registros. `verify` inicia/cierra sesiones de prueba y, como el login normal, actualiza los intentos y dígitos verificadores de esas cuentas.

## Generar el instalador

Con Visual Studio/MSBuild, .NET Framework 4.7.2 e Inno Setup 6 instalados:

```powershell
powershell -NoProfile -File Installer/build-installer.ps1
```

Compila la aplicación, la herramienta de precarga y el instalador. Los detalles de prerrequisitos se encuentran en [Installer/README.md](Installer/README.md). El modelo editable y sus verificaciones se describen en [Documentacion/EnterpriseArchitect/README.md](Documentacion/EnterpriseArchitect/README.md).
