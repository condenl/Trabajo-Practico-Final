# Verificación de escenarios demo — 26/10/2026

## Datos incluidos

`SeedDemoScenarios.cs` amplía la herramienta existente, dentro de la misma transacción serializable que los actores. Se compila tanto desde `seed-demo-actors.ps1` como desde `build-installer.ps1`.

- Tres productos con códigos reservados `DEMO-FLORAL`, `DEMO-MADERA` y `DEMO-CITRICO`.
- Dos cotizaciones con cinco detalles en total; Cítrico tiene intencionalmente un solo proveedor cotizado.
- Ocho órdenes: borrador, pendiente, aprobada, rechazada con motivo, emitida, parcialmente recibida, recibida y otro borrador sin cotizaciones suficientes. Se identifican por `DEMO-OC-01` a `DEMO-OC-08` en `CreadaPor`, junto al correo de Compras.
- Quince detalles de compra, cinco lotes con certificados/origen ficticios y cinco movimientos de ingreso atribuidos a Depósito. Stock inicial: 14 Floral y 7 Madera.
- No se insertan ventas automáticamente. El cliente con membresía existente permite demostrar el descuento del 10 %.

Las aprobaciones referencian al usuario real de Contabilidad. Los identificadores se consultan por claves de demo, nunca se asumen números de identidad fijos. Los datos ajenos a estos marcadores no se modifican. Un escenario existente se conserva completo, aunque ya haya avanzado o consumido stock. Las fechas de la primera carga no se renuevan al reintentar.

## Pruebas ejecutadas

`Scripts/test-demo-scenarios.ps1` creó bases aisladas y ejecutó la BLL y DAL reales, iniciando sesión con cada actor; no son mocks ni pruebas gráficas.

| Motor | Base de la ejecución final | Resultado |
| --- | --- | --- |
| SQL Server 16, `DESKTOP-TFHVP49` | `Perfumeria_Demo_Test_ec4f20a405594a728a7869551373fae6` | 53 verificaciones correctas |
| LocalDB 17, `(localdb)\MSSQLLocalDB` | `Perfumeria_Demo_Test_fd28c8708d05496e94bc312aa4d3fdce` | 53 verificaciones correctas |

Se verificaron los ocho estados iniciales, stock y lotes; aprobación/rechazo y emisión por sus actores; rechazo de permisos indebidos; bloqueo por una cotización y desbloqueo al registrar la segunda; recepción parcial y completa; rechazo por exceder el pendiente sin escrituras parciales; venta con total $56.700, membresía y lotes FEFO; y rechazo de venta sin stock.

Se compararon instantáneas de las tablas antes/después de `--check` y del reintento `--apply`: cero cambios, incluso después de avanzar órdenes y vender. Para comprobar rollback se introdujo una inconsistencia en una orden y se retiró otro escenario **solo en las bases aisladas**: el seed comenzó a recrear el faltante y falló en la validación tardía; toda la operación se revirtió. Se corrigió la inconsistencia deliberada y se verificó nuevamente. Las bases de prueba se conservaron para inspección; también quedaron dos ensayos previos de 52 verificaciones en bases aisladas, más dos bases de la prueba de instalación/recuperación.

`Scripts/test-localdb-installer.ps1` pasó con el instalador ampliado: restauración nueva, migraciones, precarga, cuatro accesos, reintento con cero operaciones y recuperación explícita de un respaldo sin bootstrap. Bases conservadas: `Perfumeria_LocalDb_Test_c0e2908e4664` y `Perfumeria_LocalDb_Recovery_c0e2908e4664`.

Las 30 pruebas de `Arquitectura.Tests` pasaron, con las dos advertencias de estilo MSTest preexistentes. `test-sql-version-policy.ps1` pasó para 0/15 rechazados y 16/17/18 admitidos. El instalador se recompiló correctamente con Inno Setup 6; no se ejecutó el asistente gráfico ni se probó en una máquina externa.

## Base de desarrollo

La prevalidación de `DESKTOP-TFHVP49 / Trabajo Practico Final 3` pasó sin cambios. Se aplicaron 43 operaciones de escenarios, conservando los actores y datos previos. Las órdenes demo recibieron los IDs 2 a 9, respectivamente. Se verificaron los cuatro accesos, permisos, DVH/DVV y coherencia de escenarios. Un segundo `apply` produjo cero operaciones. No se ejecutaron las transiciones de prueba sobre esta base: las órdenes permanecen en sus estados iniciales, listas para la demo.

## Uso

Los escenarios y actores están disponibles en el script de desarrollo y en la opción **Cargar actores y escenarios demo** del instalador. Los pasos, credenciales y catálogo de escenarios están en el README principal. Para repetir exactamente la situación inicial se debe usar una nueva base de pruebas, no restaurar ni borrar una base con trabajo previo.
