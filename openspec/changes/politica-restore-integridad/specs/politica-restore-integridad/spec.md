## ADDED Requirements

### Requirement: Verificar Integridad en Inicio de Sesión
El sistema SHALL verificar la integridad de la base de datos (DVH y DVV) inmediatamente después de que un usuario inicie sesión con éxito. El chequeo inicial de integridad en el arranque del sistema (antes de mostrar el Login) SHALL ser removido.

#### Scenario: Usuario administrador inicia sesión con errores de integridad
- **WHEN** un usuario con permiso "VerReportes" inicia sesión con éxito y existen errores de integridad en la base de datos
- **THEN** el sistema SHALL abrir un diálogo de recuperación mostrando los detalles de los errores y sugerencias de backups

#### Scenario: Usuario común inicia sesión con errores de integridad
- **WHEN** un usuario sin permiso "VerReportes" inicia sesión con éxito y existen errores de integridad en la base de datos
- **THEN** el sistema SHALL omitir el error de integridad y permitirle continuar utilizando la aplicación normalmente

### Requirement: Interfaz de Recuperación de Integridad y Sugerencia de Backups
El sistema SHALL proveer una interfaz de recuperación de integridad exclusiva para usuarios con permiso "VerReportes". Esta interfaz SHALL mostrar la lista de errores detallada y sugerir la revisión de backups indicando la ruta física del proyecto en la que residen.

#### Scenario: Visualización del detalle de errores de DVH
- **WHEN** se presenta un error de integridad de tipo DVH
- **THEN** la interfaz de recuperación SHALL mostrar los datos desencriptados del registro de usuario afectado (DNI, Nombre, Apellido, Email, Activo, Intentos Fallidos, Fecha Alta)

#### Scenario: Sugerencia de ruta de backups en UI
- **WHEN** se despliega la interfaz de recuperación
- **THEN** la interfaz SHALL visualizar la ruta absoluta en la que existen los archivos de respaldo (.bak) de la base de datos

### Requirement: Políticas de Restauración e Integridad por UI
La interfaz de recuperación de integridad SHALL proveer las opciones de Recalcular DV y Restaurar Backup.

#### Scenario: Ejecución de Recalcular DV exitoso
- **WHEN** el usuario hace clic en "Recalcular DV" y la base de datos actualiza correctamente los registros afectados y el DVV de la tabla sin dejar más errores
- **THEN** el sistema SHALL mostrar un mensaje de éxito, registrar el evento "RESTORE_INTEGRIDAD" en la bitácora y cerrar la UI permitiendo el acceso al menú principal

#### Scenario: Restauración de backup de base de datos exitosa
- **WHEN** el usuario selecciona un archivo de respaldo (.bak) válido y ejecuta la restauración con éxito sin dejar errores de integridad
- **THEN** el sistema SHALL restaurar la base de datos, validar la integridad, registrar el evento "RESTORE" en la bitácora de la base de datos restaurada y permitir el ingreso al sistema

### Requirement: Soporte Multi-idioma para Recuperación de Integridad y Auditoría
La interfaz de recuperación de integridad (incluyendo etiquetas, botones y mensajes de diálogo) y los textos relacionados con sus eventos de auditoría SHALL soportar traducción automática a los idiomas Español e Inglés según la configuración activa.

#### Scenario: Traducción de la interfaz de FormIntegridad al Español
- **WHEN** el idioma seleccionado es Español
- **THEN** todas las etiquetas, títulos de ventanas, botones y mensajes de confirmación de FormIntegridad SHALL visualizarse en Español

#### Scenario: Traducción de la interfaz de FormIntegridad al Inglés
- **WHEN** el idioma seleccionado es Inglés
- **THEN** todas las etiquetas, títulos de ventanas, botones y mensajes de confirmación de FormIntegridad SHALL visualizarse en Inglés

#### Scenario: Traducción de eventos de auditoría (RESTORE y RESTORE_INTEGRIDAD)
- **WHEN** el usuario consulta los reportes o bitácoras en diferentes idiomas
- **THEN** los nombres de las actividades de auditoría correspondientes a "RESTORE" y "RESTORE_INTEGRIDAD" SHALL aparecer traducidos según el idioma seleccionado
