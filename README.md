# Perfumería

Aplicación de escritorio C# (.NET Framework 4.7.2) con SQL Server **16.x (SQL Server 2022) o superior**, incluida la versión **17.x (SQL Server 2025)**. Las cuentas de acceso pertenecen a la base de datos; funcionan tanto desde Visual Studio como desde `UI.exe` y la aplicación instalada.

## Uso de Compras y evaluación

Compras abre la grilla de órdenes. **Nuevo borrador** despliega sus campos; las altas de proveedores y perfumes están en **Catálogos → Nuevo proveedor / Nuevo perfume**, en ventanas separadas. Al guardar, las listas se recargan. **Actualizar** y el cambio de pestaña consultan catálogos, órdenes, pendientes y recepción; la selección se conserva si todavía existe y se informa la hora de carga.

En **Evaluación de proveedores**, **Comparar** muestra barras de puntaje y margen bruto, con indicadores de stock, compras pendientes, ventas y reposición. Una barra permite elegir proveedor; **Listado completo** muestra todas las ofertas, incluidas las excluidas. Los gráficos sintetizan las primeras ocho. **Criterios** e **Historial** se despliegan cuando se necesitan.

**Actualizar** recalcula una comparación todavía no confirmada y conserva su elección y fundamento en preparación. La comparación anterior permanece en el historial. Para una decisión confirmada, recarga ese registro; **Comparar** inicia otro análisis con datos actuales. Los gráficos usan el control incluido en .NET Framework y no requieren instalar otra biblioteca.

## Uso de Ventas y administración

En **Ventas**, seleccione un cliente por correo o use **Venta ocasional**. **Nuevo cliente...** abre una ventana independiente y, al guardar, lo selecciona para esa venta. Cambiar el correo quita al cliente anterior; confirmar una venta limpia tanto el carrito como el cliente para la siguiente operación. El carrito muestra precios de solo lectura, stock vendible y un botón **Quitar producto**. Subtotal, descuento de membresía y total aparecen antes de confirmar. Si los importes cambiaron desde la vista previa, se solicita revisarlos antes de registrar.

Las órdenes se pueden filtrar por **proveedor, estado y fechas**. PDF y XML usan el listado filtrado. **Limpiar filtros** recupera el listado completo; crear un borrador también los limpia para poder seleccionar la nueva orden. Enviar a Contabilidad solo se habilita para borradores y Emitir al proveedor para órdenes aprobadas, con una indicación del siguiente paso.

En **Usuarios**, **Nuevo usuario** limpia la selección y habilita Guardar nuevo usuario. Seleccionar una fila permite **Guardar cambios**; dejar la contraseña vacía la conserva. En **Auditoría**, **Revisar restauración...** compara los datos actuales con los que quedarán y requiere confirmación. Las credenciales se indican como ocultas. Las validaciones habituales se muestran junto al formulario.

**Ver detalle...** en Compras y Contabilidad muestra precios unitarios, cantidades pedidas/recibidas/pendientes y datos de aprobación o rechazo. En Recepción se informa el proveedor y el saldo del perfume; la cantidad ingresada no puede superar ese saldo. En N03, **Usar cantidad sugerida** realiza otra comparación para esa cantidad, sin confirmar una decisión; después de crear el borrador, **Abrir orden #...** lo selecciona en Compras sin duplicarlo.

Cambiar de sección o cerrar la aplicación solicita confirmación si se perdería un carrito, datos de compra/recepción, un fundamento de evaluación o una edición de usuario sin guardar. Cancelar mantiene la vista y sus datos; sin trabajo pendiente se continúa directamente.

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

## Escenarios precargados para demos

La misma opción de precarga incluye **3 productos, 2 cotizaciones y 8 órdenes de compra**, además de lotes y movimientos de ingreso. Buscar los marcadores `DEMO-OC-01` a `DEMO-OC-08` en **CreadaPor** de la grilla de compras; el número de orden real se asigna en cada base y se informa al ejecutar el script.

| Marcador | Estado inicial | Demo sugerida / actor |
| --- | --- | --- |
| `DEMO-OC-01` | Borrador | Dueño: enviar a revisión; tiene dos proveedores con cotizaciones comparables. |
| `DEMO-OC-02` | PendienteAprobacion | Contabilidad: aprobar o rechazar indicando un motivo. |
| `DEMO-OC-03` | Aprobada | Dueño: emitir la orden aprobada por Contabilidad. |
| `DEMO-OC-04` | Rechazada | Consultar el motivo; comprobar que no puede emitirse ni recibirse. |
| `DEMO-OC-05` | Emitida | Depósito: registrar la primera recepción (parcial o completa). |
| `DEMO-OC-06` | RecibidaParcial | Depósito: completar las **6 unidades Floral y 3 Madera** pendientes. Ya recibió 4 y 2, respectivamente. |
| `DEMO-OC-07` | Recibida | Consultar trazabilidad, certificados y movimientos de los lotes; vender el stock recibido. |
| `DEMO-OC-08` | Borrador | Dueño: intentar enviar a revisión. Se rechaza porque Cítrico solo tiene **una** cotización; registrar la segunda para continuar. |

Las órdenes 01 a 07 contienen 10 unidades de `DEMO-FLORAL` y 5 de `DEMO-MADERA`, por un total de $210.000 cada una. La 08 contiene 3 unidades de `DEMO-CITRICO`, total $27.000. Son precios ficticios. Las aprobaciones usan la cuenta de Contabilidad; los ingresos identifican a Depósito y conservan el origen de la compra.

Stock inicial: **14 Floral, 7 Madera y 0 Cítrico**. Con Ventas, buscar `demo.cliente@perfumeria.example`: vender 2 Floral y 1 Madera da un subtotal de $63.000, descuento de membresía de $6.300 y total de $56.700. Los lotes tienen distintos vencimientos para demostrar FEFO; intentar vender Cítrico permite mostrar falta de stock. No se precargan ventas: quedan disponibles para ejecutarlas durante la demo.

Las cotizaciones vencen un año después de la primera carga; los lotes, a los 90 o 180 días. **Repetir el script no duplica ni reinicia órdenes, no repone unidades vendidas y no renueva fechas**. Conserva el progreso de cada demo y valida la coherencia de totales, recepciones y movimientos. Para repetir exactamente los estados iniciales, utilizar una nueva base de pruebas; no restaurar encima de una base con trabajo que se deba conservar. Los marcadores `DEMO-*` están reservados para esta precarga.

## Usarlas con el instalador EXE en otra computadora

1. Ejecutar `Installer/Output/Perfumeria_Setup_Iteracion2.exe` e indicar la instancia local SQL Server 16.x o superior (por ejemplo, 17.x).
2. Marcar **Cargar actores y escenarios demo (claves públicas del README; solo pruebas)**. La opción está desmarcada inicialmente.
3. El instalador prepara la base `Trabajo Practico Final`, carga los actores y escenarios, verifica sus accesos y la coherencia de los datos, y configura `UI.exe.config` con esa misma base.
4. Abrir **Perfumería** desde el acceso directo e ingresar con cualquiera de las cuatro cuentas de la tabla. No hace falta Visual Studio ni ejecutar scripts manualmente.

El asistente precarga una instancia local cuya conexión y versión 16+ pudo verificar: prioriza la que ya contiene la base de Perfumería y, si no hay una, prefiere Express, otros servicios SQL y finalmente LocalDB. Puede cambiarse manualmente; la elección no se reemplaza al volver atrás. Al pulsar **Siguiente**, comprueba la instancia elegida. Si falta el motor, **Descargar SQL Server** abre la página oficial de Microsoft para instalar Express 2022 o superior. Si el servicio está detenido, ofrece **Abrir servicios**; si el nombre es incorrecto, muestra las instancias detectadas. Después de corregirlo, se puede volver a pulsar Siguiente. Un error de conexión no se presenta como una ausencia del motor.

Estas son cuentas de la aplicación, no logins de SQL Server ni de Windows. La conexión SQL continúa usando autenticación integrada de Windows; la identidad que ejecuta la aplicación necesita acceso a la base. Copiar solo `UI.exe` no copia las cuentas: hay que instalar/preparar la base y conservar las DLL y `UI.exe.config`. El README también se incluye en la carpeta instalada.

Si la base ya existe, se conserva. Si hay conflictos con las cuentas demo o errores previos de integridad, la precarga falla sin sobrescribirlos y el instalador informa el error. Solo cuando acaba de restaurar una base nueva del respaldo distribuido, inicializa DVH/DVV antes de cargar los actores. Revisar `Installer/instalacion-base-datos.log`. No es una función de reparación de bases existentes.

### LocalDB y recuperación de la instalación fallida

Se admite `(localdb)\MSSQLLocalDB` cuando su motor es 16 o superior. La comprobación usa tanto el endpoint local como `SERVERPROPERTY('IsLocalDB')`; no depende de que el nombre de equipo informado por SQL coincida con Windows. La precarga sigue rechazando servidores remotos y ahora valida el destino antes de restaurar. LocalDB pertenece al usuario de Windows: instalar y ejecutar la aplicación con la misma cuenta, sin cambiar a otro usuario administrador.

Si una versión anterior restauró la base y luego falló con **«Solo se permite una instancia local de desarrollo»**, la base ya existe pero sus dígitos verificadores iniciales podrían no haberse preparado. No hay que borrarla ni restaurar encima. **Solo si esa base acaba de salir de esa instalación fallida y todavía no se utilizó**, seguir estos pasos:

1. Ejecutar el instalador actualizado sin marcar **Cargar actores y escenarios demo** ni **Ejecutar Perfumería**, para actualizar los binarios y conservar la base.
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
# Crear actores, perfiles y escenarios faltantes en una transacción.
powershell -NoProfile -File Scripts/seed-demo-actors.ps1 -Database "Trabajo Practico Final 3" -Mode apply
# Probar el Login real, permisos, DVH/DVV y coherencia de los escenarios.
powershell -NoProfile -File Scripts/seed-demo-actors.ps1 -Database "Trabajo Practico Final 3" -Mode verify
```

El seed solo admite una instancia SQL local 16.x o superior con autenticación integrada. Puede ejecutarse en otras computadoras: no depende del nombre de este equipo. Usa el cifrado, hash y cálculo de integridad de la aplicación. Repetirlo con datos coincidentes no duplica registros. `verify` inicia/cierra sesiones de prueba y, como el login normal, actualiza los intentos y dígitos verificadores de esas cuentas.

Sobre una instalación existente, volver a ejecutar el instalador actualizado marcando los datos demo, o desde su carpeta ejecutar `SeedDemoActors.exe --apply "Trabajo Practico Final"` y luego `SeedDemoActors.exe --verify "Trabajo Practico Final"`, con `SeedDemoActors.exe.config` apuntando a la misma base que `UI.exe.config`. No hace falta borrar la base ni las cuentas existentes.

## Reportes e intercambio de órdenes

En **Aprovisionamiento → Órdenes de compra**, los botones de archivos están en una barra independiente encima de la grilla:

- **Reporte PDF:** guarda el listado completo mostrado, con proveedor/CUIT, estado, fechas, responsable, aprobación o rechazo y cada producto, cantidad, cantidad recibida, precio unitario, subtotal y total. Usa PDFsharp/MigraDoc GDI **6.2.4**, licencia MIT, compatible con .NET Framework 4.7.2. Incluye paginación automática y encabezados de tabla repetidos cuando el detalle ocupa varias páginas. No requiere tener instalado un lector de PDF para generarlo.
- **Exportar XML:** guarda el listado completo y sus detalles usando `System.Xml.Serialization.XmlSerializer`, con formato `ArchivoOrdenesCompra` versión 1. La exportación y deserialización conservan los estados y metadatos originales en el archivo.
- **Importar XML:** abre un archivo del mismo formato, valida el lote y solicita confirmación antes de guardar. Resuelve proveedores por **CUIT** y productos por **código**, que deben existir y estar activos en la base destino. Se crean **órdenes nuevas en Borrador**, con nuevos IDs, fecha y responsable actuales, total recalculado y cantidades recibidas en cero. No restaura aprobaciones, recepciones ni movimientos de stock y no sobrescribe órdenes existentes. Si se vuelve a importar el mismo archivo, se crean otras copias; el diálogo lo advierte y permite cancelar.
- **Sample XML:** copia al portapapeles un XML válido con **3 órdenes**, usando proveedores y perfumes activos de la base actual. Las cantidades y precios son ilustrativos; copie el contenido en un archivo `.xml` guardado como UTF-8 y ábralo con Importar XML. Generar el ejemplo no crea compras. Se requiere al menos un proveedor y un perfume activos; si hay menos de tres, se reutilizan en órdenes diferentes.

También se incluye [un XML listo con tres órdenes demo](Documentacion/Ejemplos/ordenes-compra-3.xml), disponible en la misma ruta dentro de la carpeta instalada. Usa los CUIT `99000000001` / `99000000002` y los códigos `DEMO-FLORAL`, `DEMO-MADERA` y `DEMO-CITRICO` de los catálogos precargados. Si esos registros ya no están activos, use Sample XML para adaptar el ejemplo al catálogo actual.

Las operaciones requieren el permiso de gestión de aprovisionamiento. Una orden inválida cancela el lote completo; los insertados comparten una transacción SQL. Los XML malformados, versiones desconocidas, entidades externas/DTD, cantidades o precios inválidos y archivos mayores a 10 MB se rechazan. El límite por archivo es de 1000 órdenes y 10000 detalles. Guardar PDF/XML usa un archivo temporal y reemplazo atómico para conservar el destino anterior si falla la generación.

La biblioteca se restaura por NuGet al compilar en Visual Studio o con `Installer/build-installer.ps1`; las DLL y el aviso de licencia se incluyen en el instalador. Se fija la dependencia de seguridad `System.Security.Cryptography.Pkcs` en **8.0.2**. La capa BE contiene el contrato XML; Servicios serializa/deserializa; BLL valida y prepara los borradores; DAL guarda el lote; UI maneja los diálogos y el reporte PDF.

Verificación: `dotnet test Arquitectura.Tests/Arquitectura.Tests.csproj --configuration Release`. Después de compilar el instalador, `powershell -NoProfile -File Scripts/test-ordenes-archivos.ps1 -ServerInstance "DESKTOP-TFHVP49"` ejecuta pruebas de componentes WinForms y SQL en una base aislada, conserva los XML/PDF generados y no cambia `UI/App.config`. También admite `"(localdb)\MSSQLLocalDB"` 16+.

## N03: evaluación de proveedores

En **Compras e inventario → Evaluación de proveedores**: elegir perfume y cantidad, pulsar **Comparar**, revisar el ranking y **Confirmar proveedor** con un fundamento. **Crear orden en Borrador** permite continuar por el circuito habitual de Contabilidad; reintentar devuelve el mismo borrador.

La evaluación cruza cotizaciones vigentes, precio, prestigio y antigüedad con ventas netas de descuentos, stock utilizable, compras pendientes y antecedentes de recepción. Muestra costo total, margen bruto estimado, reposición sugerida y advertencias. Los pesos predeterminados son 50/30/20 y pueden ajustarse desde **Criterios**. Guarda las comparaciones y decisiones en un historial. Si cambian los datos o pasa el día de evaluación, exige comparar nuevamente antes de confirmar o crear la orden.

La precarga incluye **DEMO Evaluacion - valor vs precio** (el proveedor recomendado no es el más barato; cambiar los pesos muestra el efecto) y **DEMO Evaluacion - sin margen** (ninguna oferta recomendable), con dos productos, un proveedor adicional y tres cotizaciones nuevas. El caso **DEMO Citrico sin stock** conserva una sola cotización para demostrar una comparación insuficiente. Las demos anteriores mantienen su progreso y no se reemplazan al reintentar.

Reglas, fórmulas, casos de uso y alcance: [N03_Evaluacion_Proveedores.md](Documentacion/N03_Evaluacion_Proveedores.md). Pruebas: [Verificacion_N03.md](Documentacion/Verificacion_N03.md). El instalador ejecuta la migración idempotente `SQL/Iteracion3_Evaluacion_Proveedores.sql`; la base local ya recibió esa migración y los datos demo adicionales.

## Generar el instalador

Con Visual Studio/MSBuild, .NET Framework 4.7.2 e Inno Setup 6 instalados:

```powershell
powershell -NoProfile -File Installer/build-installer.ps1
```

Compila la aplicación, la herramienta de precarga y el instalador. Los detalles de prerrequisitos se encuentran en [Installer/README.md](Installer/README.md). El modelo editable y sus verificaciones se describen en [Documentacion/EnterpriseArchitect/README.md](Documentacion/EnterpriseArchitect/README.md).

Para verificar los escenarios con la lógica real y cada actor (crea una base aislada y la conserva para inspección):

```powershell
powershell -NoProfile -File Scripts/test-demo-scenarios.ps1 -ServerInstance "DESKTOP-TFHVP49"
# O en LocalDB 16+:
powershell -NoProfile -File Scripts/test-demo-scenarios.ps1 -ServerInstance "(localdb)\MSSQLLocalDB"
```

La prueba cubre aprobación/rechazo, emisión, recepciones parciales/completas, cotización faltante, venta con descuento/FEFO, permisos, reintento sin reiniciar las demos y rollback ante una inconsistencia. No sustituye una prueba gráfica del asistente de instalación.
