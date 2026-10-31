# Verificación de reportes e intercambio de órdenes

Fecha: 31/10/2026.

## Funcionalidad

En Aprovisionamiento → Órdenes de compra hay tres botones sobre la grilla: Reporte PDF, Exportar XML e Importar XML. Las exportaciones incluyen todo el listado visible y sus detalles. El reporte utiliza PDFsharp/MigraDoc GDI 6.2.4; el intercambio XML usa `System.Xml.Serialization.XmlSerializer`.

El XML conserva los datos históricos de origen. Al importar, se crean órdenes nuevas en Borrador, con fecha y responsable actuales, productos resueltos por código y proveedor por CUIT. Los catálogos deben existir y estar activos. No se copian aprobaciones, recepciones ni movimientos de stock. La confirmación advierte que repetir la importación genera otras copias y ofrece No como opción predeterminada.

## Pruebas automáticas

- `dotnet test Arquitectura.Tests/Arquitectura.Tests.csproj --configuration Release --no-restore`: **49 pruebas correctas, 0 errores**. Incluyen serialización/deserialización, cultura decimal, validaciones, permisos, conservación de archivos ante errores, preparación de borradores y PDF con varias páginas.
- `Scripts/test-ordenes-archivos.ps1 -ServerInstance "DESKTOP-TFHVP49"`: **19 verificaciones correctas** sobre SQL Server 16.
- El mismo script con `-ServerInstance "(localdb)\MSSQLLocalDB"`: **19 verificaciones correctas** sobre SQL Server 17.
- Las verificaciones de integración ejecutan los manejadores reales de los botones WinForms con repositorios SQL, prueban cancelación, exportación completa, importación, actualización de grilla, permisos y rollback real cuando falla la segunda orden del lote. Esta parte usa modo de prueba sin diálogos nativos.
- La auditoría de paquetes de NuGet no detectó dependencias vulnerables en los orígenes consultados. Se fija `System.Security.Cryptography.Pkcs` en 8.0.2.

## Prueba manual de interfaz y revisión del PDF

Se abrió una instancia visual de `UC_Aprovisionamiento` mediante `TestOrdenesArchivos.exe`, con inicio de sesión real del dueño y una base aislada. Se comprobaron los tres botones visibles y legibles. Con el modo de prueba desactivado:

1. Reporte PDF abrió Guardar como y generó un reporte de 16 órdenes.
2. Exportar XML abrió Guardar como y generó el listado de 16 órdenes.
3. Importar XML abrió el selector nativo, leyó el archivo inicial de 8 órdenes y presentó la confirmación. Al aceptar, creó los borradores 19 a 26 y actualizó la grilla.

Se renderizaron e inspeccionaron visualmente las 8 páginas del reporte inicial. También se generó y revisó una orden de 90 renglones en 5 páginas, incluyendo un código de 50 caracteres y un nombre de 150 caracteres sin espacios. Se corrigió el ajuste de esos textos y se verificó que las columnas no se superponen, los encabezados se repiten, los pies quedan separados y el total aparece al final. El ajuste agrega saltos visuales en el PDF sin cambiar los valores del XML.

## Aislamiento y artefactos

Las pruebas conservaron estas bases para inspección; no modificaron la base de desarrollo ni `UI/App.config`:

- SQL Server 16: `Perfumeria_Archivos_Test_9b6c0d763e544b619af1c3213e8ce8c4`.
- SQL Server 17: `Perfumeria_Archivos_Test_be92a1f8160c4501aa8aad911225f5ba`.

Los XML/PDF y el host visual de SQL Server 16 quedaron en `C:/Users/lucia/AppData/Local/Temp/PerfumeriaOrdenesArchivos-7971b7e63b1543b0b5b1ed44ffd88580`. Los de SQL Server 17, en `C:/Users/lucia/AppData/Local/Temp/PerfumeriaOrdenesArchivos-0c343ed7ed9b40949c6e87bb543cb4a6`.

`Installer/build-installer.ps1` compiló correctamente la aplicación y regeneró `Installer/Output/Perfumeria_Setup_Iteracion2.exe`, incluyendo las dependencias del reporte, el README actualizado y `Licencias_Reportes.txt`. No se ejecutó el asistente completo del instalador en otra computadora como parte de esta verificación.
