# Verificación de N03 — 01/11/2026

- **64 pruebas unitarias correctas**, incluyendo 15 casos de evaluación: ponderación, elección distinta del menor precio, demanda/stock/pendientes, precio neto de descuentos, cotizaciones futuras/vencidas, proveedor inactivo, margen, empates, antigüedad, pesos, identidad del actor y evidencia serializada.
- **20 verificaciones de integración correctas en SQL Server 16**: base aislada `Perfumeria_Evaluacion_Test_d107113d78f9404fa7b311b537b4ea6c`.
- **20 verificaciones de integración correctas en LocalDB 17**: base aislada `Perfumeria_Evaluacion_Test_576178c98c6b47108a7fd862f5bbe2c3`.
- Los casos de integración ejercitan los botones WinForms, los repositorios SQL, la decisión fundamentada, creación del borrador, reintento, dos solicitudes concurrentes, invalidación por cambios de datos/stock, insuficiencia de ofertas y actor no autorizado. No son pruebas de clics físicos: utilizan eventos de controles dentro de un formulario de prueba.
- **53 verificaciones de regresión correctas** de los escenarios N01/N02 sobre SQL Server 16: base `Perfumeria_Demo_Test_ec6e51a9013a48b595ab4cffeb30d8fe`, incluyendo compras, revisión contable, recepciones, ventas con membresía, FEFO, preservación del progreso e idempotencia/rollback del seed ampliado.
- **19 verificaciones de regresión correctas** de PDF/XML, importación atómica y permisos por actor en `Perfumeria_Archivos_Test_738779e8b14b486c84333c49706995c9`, con el módulo N03 y el seed ampliado ya incluidos.
- El modelo EA pendiente de sesiones anteriores verificó **119 representaciones de clases, 156 mensajes y 13 diagramas**; se integra en un commit de documentación separado. Esa comprobación conserva el diseño previo y no implica haber regenerado sus vistas técnicas N03 para las nuevas clases.

La revisión manual con entrada real de escritorio utilizó el host visual de la primera base SQL Server 16. Se eligió el producto de comparación, se solicitó el ranking, se escribió un fundamento, se confirmó al proveedor y se creó el borrador **11**, vinculado a la evaluación **6**. Se revisaron la legibilidad de controles, el resumen, el detalle por proveedor y los mensajes de confirmación. El host usa los componentes y clases de producción y una base aislada.

La base local de desarrollo recibió únicamente la migración aditiva y los registros demo N03: **12 operaciones de precarga**, preservando las órdenes DEMO-OC-01 a 08 y sus estados. Se verificaron nuevamente los actores y los escenarios; repetir `apply` produjo **0 operaciones**. No se ejecutaron compras de prueba en esa base.

Las dos primeras ejecuciones de integración detectaron que el host de prueba sin formulario no daba contexto de selección a la grilla. Se corrigió el host para alojar el control en un formulario real y se repitieron los casos; las bases aisladas iniciales también se conservaron. No se trata de fallos pendientes.

Reproducción:

```powershell
dotnet test Arquitectura.Tests/Arquitectura.Tests.csproj --configuration Release
powershell -NoProfile -File Installer/build-installer.ps1
powershell -NoProfile -File Scripts/test-evaluacion-proveedores.ps1 -ServerInstance "DESKTOP-TFHVP49"
powershell -NoProfile -File Scripts/test-evaluacion-proveedores.ps1 -ServerInstance "(localdb)\MSSQLLocalDB"
```

Se conserva `UI/App.config` tal como lo configuró el usuario. No se cambiaron permisos de SQL Server ni credenciales de Windows. La comprobación de prerrequisitos del instalador mantiene autenticación integrada; se retiró su explicación nueva sobre permisos, por pedido del usuario.
