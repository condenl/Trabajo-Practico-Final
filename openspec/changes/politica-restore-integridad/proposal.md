## Why

El sistema actualmente interrumpe el arranque del programa para todos los usuarios si existe alguna falla de integridad de datos (DVH/DVV). Esto causa indisponibilidad innecesaria para usuarios comunes que no son administradores y no tienen la responsabilidad ni capacidad de corregir estos problemas. Además, los administradores carecen de herramientas en la UI para analizar el registro afectado y ejecutar acciones de restauración (como recalcular o restaurar un backup).

## What Changes

- Modificar el flujo de arranque para no comprobar la integridad de base de datos en el `Main` de `Program.cs`.
- Ejecutar el control de integridad inmediatamente después de un inicio de sesión exitoso.
- Si se detecta un error de integridad:
  - Los usuarios administradores (con el tipo de permiso `VerReportes`) verán una pantalla de error con sugerencias, la ruta de los backups y opciones para recalcular o restaurar.
  - Los usuarios comunes podrán continuar utilizando la aplicación con total normalidad.
- Mostrar los datos detallados del registro afectado cuando el error es de tipo DVH (incluyendo el DNI desencriptado).
- Habilitar en la interfaz las siguientes políticas de restauración:
  - **Recalcular DV**: Recalcula el DVH para los registros afectados e invalida/actualiza el DVV de la tabla correspondiente.
  - **Restaurar Backup**: Habilita la selección de archivos `.bak` desde la ruta física sugerida (`/SQL`) para restaurar la base de datos a un estado consistente.

## Capabilities

### New Capabilities
- `politica-restore-integridad`: Control y políticas de recuperación de integridad de datos y restauración de base de datos desde la interfaz de usuario para administradores.

### Modified Capabilities

## Impact

- `UI.Program`: Se remueve la validación de arranque.
- `UI.Login`: Se valida la integridad después de iniciar sesión y se maneja la redirección.
- `UI.FormIntegridad`: Nueva interfaz gráfica de restauración.
- `BLL.UsuarioBLL`: Métodos para recalcular dígitos y disparar restauración.
- `DAL.UsuarioDAL` & `DAL.Conexion`: Lógica de actualización de DVH/DVV y comandos SQL de restauración.
