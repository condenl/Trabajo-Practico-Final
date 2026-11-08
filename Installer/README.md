# Instalador inicial A01

Este directorio implementa el entregable **A01 de la Iteracion 2** siguiendo la guia provista para instaladores `.exe` en C#.

## Alcance

- instala los binarios Release de la aplicacion en `Program Files`;
- crea accesos directos en el menu Inicio y, opcionalmente, en el escritorio;
- busca y precarga una instancia SQL Server local compatible, permitiendo cambiarla;
- verifica .NET Framework 4.7.2 o superior antes de copiar la aplicacion;
- verifica que la instancia indicada sea SQL Server 16.x (SQL Server 2022) o superior, incluida 17.x (SQL Server 2025);
- si falta .NET, lo descarga desde Microsoft en segundo plano, valida su firma digital y lo instala silenciosamente;
- comprueba la conexión al pulsar Siguiente en la página de SQL y nuevamente antes de instalar; distingue motor ausente, instancia incorrecta y servicio detenido;
- muestra **Descargar SQL Server** únicamente si el diagnóstico confirma motor ausente o una versión anterior a 16; abre la [página oficial de Microsoft en español](https://www.microsoft.com/es-es/sql-server/sql-server-downloads). Para un servicio detenido muestra **Abrir servicios**;
- restaura la base inicial si todavia no existe;
- ejecuta las migraciones idempotentes de N01, N02 y N03, más `Iteracion3_Pulido_Demo.sql` para permisos administrativos y textos del acceso;
- actualiza `UI.exe.config` con la instancia elegida y autenticacion integrada;
- conserva un registro en `Installer\instalacion-base-datos.log`.

La descarga automática de .NET se guarda en `%APPDATA%\Perfumeria\Prerrequisitos`. Para SQL Server se utiliza la página oficial en lugar de un enlace fijo al ejecutable, que puede dejar de estar disponible. El motor SQL no se instala ni se descarga automáticamente por un fallo de conexión: primero se informa la causa. Después de instalar Express 2022 o superior, se puede regresar al asistente de Perfumería, indicar la instancia y pulsar Siguiente para volver a comprobarla.

## Cuando SQL Server no está disponible

Al entrar por primera vez en la página de SQL, el asistente consulta las instancias locales y comprueba su conexión y versión. Prioriza una que ya contenga la base `Trabajo Practico Final`; en una instalación nueva, prefiere un servicio Express compatible, luego otros servicios SQL y finalmente LocalDB. Omite servicios detenidos, conexiones fallidas y versiones anteriores a 16. Si no encuentra una opción verificable, deja el campo vacío para indicar el destino manualmente. Los tiempos de conexión de esta búsqueda son acotados y no se descargan componentes ni se crean bases de la aplicación.

El campo sigue siendo editable. Una selección manual o un argumento `/SQLSERVER="instancia"` se respeta y no se reemplaza al retroceder y avanzar. Si se modifica el campo, el mensaje anterior se limpia y se vuelve a comprobar al pulsar Siguiente.

El botón de descarga comienza oculto. Sigue oculto si ya hay una instancia compatible, mientras se comprueba una instancia nueva, ante un nombre incorrecto o un fallo de conexión. Al editar el campo se ocultan las acciones del diagnóstico anterior; reaparecen únicamente las que correspondan al resultado de la nueva comprobación.


| Situación | Mensaje y acción del asistente |
| --- | --- |
| No hay motor ni instancias locales detectadas | «Motor de base de datos no instalado». **Descargar SQL Server** abre Microsoft para instalar Express 2022 o superior. Tener SSMS no equivale a tener el motor. |
| El nombre no existe, pero hay otras instancias | Muestra los nombres detectados para corregir el campo. No pide reinstalar el motor. |
| El servicio de esa instancia está detenido, pausado o iniciándose | Identifica el servicio y ofrece **Abrir servicios**. Iniciarlo o reanudarlo, esperar y volver a pulsar Siguiente. No cambia servicios automáticamente. |
| Servicio activo, pero sin conexión | Indica revisar el nombre y la configuración de la instancia; no afirma que falte SQL Server. |
| Instancia registrada sin su servicio | Indica revisar o reparar esa instalación. |
| LocalDB instalado sin conexión | Indica comprobar el nombre y la cuenta de Windows con `SqlLocalDB info`. LocalDB no utiliza un servicio de Windows; una conexión válida lo inicia automáticamente. |
| Versión anterior a 16 | Permite elegir otra instancia o descargar Express 2022 o superior. Acepta 16, 17 y versiones posteriores. |

Si no se puede leer el inventario de servicios o del registro, el diagnóstico es conservador: informa un problema de conexión sin declarar que el motor está ausente. Una conexión remota fallida tampoco se clasifica usando el inventario local. Se consultan ambas vistas del registro de Windows (32/64 bits).

La verificación no restaura ni modifica bases de la aplicación. El asistente permanece en la página de SQL ante un problema y permite corregirlo y reintentar. Los detalles técnicos quedan en `%TEMP%\Perfumeria-Prerrequisitos.log`. El intercambio interno de mensajes con Inno Setup utiliza un INI UTF-16 para conservar acentos.

## Generacion del `.exe`

Para incluir los actores, los escenarios demo y el README con las credenciales, usar:

```powershell
powershell -NoProfile -File Installer/build-installer.ps1
```

El script compila UI y `SeedDemoActors.exe` antes de llamar a Inno Setup. La opción **Cargar actores y escenarios demo** está desmarcada inicialmente. Al marcarla, las cuatro cuentas y los ocho escenarios documentados en el [README principal](../README.md#escenarios-precargados-para-demos) se crean en la misma base que utiliza el EXE instalado; no dependen de Visual Studio ni del equipo de desarrollo. Incluye productos, cotizaciones, órdenes en siete estados, lotes y movimientos de stock. Requiere una instancia local SQL Server 16.x o superior. Las claves son públicas y solo deben usarse en pruebas.

La precarga es transaccional e idempotente y verifica el login, los permisos y la coherencia de compras/stock. No reinicia escenarios ya utilizados ni modifica credenciales o roles existentes. Inicializa los dígitos verificadores del respaldo de distribución únicamente si la base acaba de ser restaurada por esa ejecución; ante errores de integridad en una base preexistente se detiene. El instalador comprueba el código de salida de la configuración y no ofrece iniciar la aplicación si falla.

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

La versión se consulta en la instancia indicada, no en SSMS. Si ya tiene SQL Server 17.x, indicar su nombre correcto (`.` para la predeterminada o `.\NOMBRE` para una instancia con nombre); no necesita instalar 16.x adicionalmente. Si está detenida, el asistente indica cómo iniciar su servicio antes de verificar la versión.

`powershell -NoProfile -File Scripts/test-sql-prerequisites.ps1` prueba los diagnósticos y la prioridad de selección con inventarios simulados (incluido servicio detenido y ausencia de motor), sin detener servicios reales. También prueba conexiones reales a `.` y `(localdb)\MSSQLLocalDB`, un nombre inexistente, una entrada inválida y el descubrimiento automático. Requiere esas dos instancias locales 16+ para los casos de conexión. `Scripts/test-sql-version-policy.ps1` comprueba la aceptación de versiones en prerrequisitos, inicialización y seed.

LocalDB 16+ también está admitido, por ejemplo `(localdb)\MSSQLLocalDB`. La precarga valida el destino antes de restaurar y distingue LocalDB de una instancia normal. Para retomar una restauración que falló con la validación local de una versión anterior, seguir [la recuperación explícita del README](../README.md#localdb-y-recuperación-de-la-instalación-fallida); no se borran bases ni se recalcula la integridad de bases existentes automáticamente.

La prueba `powershell -NoProfile -File Scripts/test-localdb-installer.ps1`, desde la raíz y después de compilar el instalador, verifica restauración, cuatro logins, reintento idempotente y recuperación explícita. Crea dos bases aisladas con sufijos únicos y las conserva para inspección. No ejecuta el asistente gráfico.

`Scripts/test-demo-scenarios.ps1 -ServerInstance "instancia-local"` verifica además los ocho escenarios con los actores reales y la BLL: transiciones de compras, recepciones, venta con membresía/FEFO, restricciones de permisos, preservación del progreso y rollback transaccional. Crea otra base aislada; nunca ejecuta los casos destructivos de prueba sobre la base de desarrollo o instalada.

Se conserva el respaldo de distribucion generado con SQL Server 16 para las computadoras de la facultad. Microsoft admite [restaurarlo en SQL Server 17](https://learn.microsoft.com/en-us/sql/database-engine/install-windows/supported-version-and-edition-upgrades-2025?view=sql-server-ver17). No reemplazarlo por un respaldo generado en 17 si debe seguir instalando en 16: los respaldos no son compatibles hacia versiones anteriores.

La precarga también incluye los ejemplos de evaluación N03 (valor frente al precio, ausencia de margen y **DEMO Reposicion con historial**). El último agrega tres ventas con descuento, dos compras y sus recepciones/movimientos: inicialmente muestra 90 unidades vendidas, stock 10, pendientes 8 y reposición sugerida 12. Se crea de manera atómica, conserva su progreso al reintentar y verifica su coherencia. El inicializador aplica `Iteracion3_Evaluacion_Proveedores.sql` tanto en una base nueva como en una existente, conservando sus datos.
