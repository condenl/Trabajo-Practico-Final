## Context

Actualmente, el sistema valida la integridad de base de datos (`DVH`/`DVV`) al momento de arrancar la aplicación (`Program.cs`), cerrando el proceso ante cualquier fallo de verificación. Esto interrumpe la disponibilidad de la aplicación para todos los usuarios.

El objetivo es mover la validación de integridad al momento de inicio de sesión exitoso, de modo que solo los usuarios administradores (aquellos con permiso `VerReportes`) vean la pantalla de error y sean requeridos a resolver el problema a través de políticas de restauración (recalcular dígitos o restaurar un backup). Los usuarios sin dicho permiso continuarán usando la aplicación normalmente.

## Goals / Non-Goals

**Goals:**
- Mover el control de integridad de arranque a pos-login.
- Permitir a usuarios comunes acceder y usar la aplicación con normalidad ante fallas de integridad.
- Mostrar la pantalla de error detallado e interfaz de políticas de restauración solo a usuarios administradores.
- Implementar la política "Recalcular DV" actualizando registros de usuario corruptos y el DVV de la tabla.
- Implementar la política "Restaurar Backup" para recuperar la base de datos desde un archivo `.bak` seleccionado por UI.

**Non-Goals:**
- Implementar backups automáticos periódicos (fuera de alcance de la tarea).
- Agregar controles de integridad a otras tablas que no sean `Usuario` en esta fase.

## Decisions

### 1. Validación de integridad pos-login
- **Decisión**: Realizar la comprobación en `UI/Login.cs` después de que `visitanteBLL.Login` retorne un objeto usuario válido.
- **Alternativa**: Realizar la comprobación en el arranque y pedir login inmediato si hay error. Se descartó porque no se sabría el tipo de usuario hasta que se loguee, y no se podría permitir el acceso directo de usuarios comunes.

### 2. Actualización de DVV y DVH en base de datos
- **Decisión**: Agregar `RecalcularDV()` en `UsuarioDAL` que recorre y calcula el DVH de cada usuario, actualiza los registros afectados mediante `sp_ModificarDVH` y recalcula el DVV mediante `ActualizarDVV()`.
- **Alternativa**: Recalcular la tabla completa. Se descartó por rendimiento; es mejor calcular y comparar en memoria y guardar solo los modificados.

### 3. Ejecución de RESTORE en ADO.NET
- **Decisión**: Ejecutar comandos SQL directamente con conexión a la base de datos `master` usando `SqlConnectionStringBuilder` y comandos de cierre de conexiones activas (`SINGLE_USER WITH ROLLBACK IMMEDIATE`).
- **Alternativa**: Utilizar comandos de shell o utilidades de SQL CMD. Se descartó para evitar dependencias externas en el entorno de ejecución del cliente.

## Risks / Trade-offs

- **[Riesgo] Conexiones activas bloquean el restore** → **[Mitigación]** Se fuerza a la base de datos a entrar en modo `SINGLE_USER WITH ROLLBACK IMMEDIATE` antes del comando `RESTORE`, y se vuelve a habilitar `MULTI_USER` inmediatamente después.
- **[Riesgo] Permisos del servicio SQL Server para leer el backup** → **[Mitigación]** El diálogo de restauración sugiere y se posiciona en la carpeta `/SQL` de la solución, y se maneja la excepción de SQL mostrando un mensaje amigable si el motor no puede leer el archivo.
