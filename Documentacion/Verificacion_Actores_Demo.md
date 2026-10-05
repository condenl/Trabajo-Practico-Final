# Verificación de actores demo 05/10/2026

## Resultados

- La aplicación Release y el instalador Inno Setup 6 compilan.
- Las 20 pruebas de Arquitectura.Tests pasan. Hay dos advertencias de estilo MSTest preexistentes.
- Base de desarrollo: `DESKTOP-TFHVP49 / Trabajo Practico Final 3`, SQL Server 16.0.1000.6.
- Se crearon cuatro usuarios, cuatro perfiles específicos, dos proveedores ficticios y un cliente con membresía. No se alteraron credenciales ni perfiles previos.
- Se verificó `UsuarioBLL.Login` para las cuatro claves del README, el conjunto exacto de permisos (incluidos los permisos denegados) y la integridad DVH/DVV posterior.
- Repetir el seed de desarrollo produjo cero operaciones de precarga.

## Instalación sobre una base nueva

Se ejecutó el mismo `InicializarBaseDatos.ps1` que empaqueta el instalador, con los binarios Release y `SeedDemoActors.exe` en un directorio temporal independiente. Se restauró el respaldo incluido y se aplicaron las migraciones y la opción de datos demo.

El primer ensayo detectó dígitos verificadores inválidos en el respaldo distribuido. Se corrigió la inicialización para establecer DVH/DVV solamente cuando esa ejecución acaba de restaurar una base nueva. La precarga sobre bases existentes sigue rechazando discrepancias de integridad y no las recalcula silenciosamente.

La segunda prueba restauró `Perfumeria_Demo_Installer_Ready_20261005`, cargó los actores y verificó los cuatro accesos. El instalador configura `UI.exe.config` y `SeedDemoActors.exe.config` con la misma base. Las contraseñas no dependen de Visual Studio ni del nombre del equipo; se almacenan utilizando el hash de la aplicación.

Se conservaron dos bases aisladas para diagnóstico: `Perfumeria_Demo_Installer_Test_20261005` (primer ensayo bloqueado) y `Perfumeria_Demo_Installer_Ready_20261005` (ensayo correcto). No sustituyen la base de desarrollo. No se borraron datos ni se restauró sobre una base preexistente.

La prueba cubre el flujo de preparación de base y autenticación real, no una ejecución completa del asistente de instalación en otra computadora ni una VM sin dependencias. Para la facultad, seleccionar **Cargar actores de demostración** y usar una instancia local SQL Server 16.x. El usuario de Windows que ejecute el EXE necesita acceso SQL integrado a esa base.

## Diagramas

Las seis secuencias EA tienen 64 llamadas y 64 retornos tipados, verificados por dirección y anidamiento. Sus capturas están reemplazadas en Google Docs. La exportación final tiene 56 páginas; el control conserva las 12 tablas y los 344 párrafos previos a 2.5, sin duplicados ni recortes entre los 26 diagramas EA. Se revisó visualmente la paginación afectada.
