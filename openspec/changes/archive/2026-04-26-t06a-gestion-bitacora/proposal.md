## Why

El sistema requiere un mecanismo robusto de auditoría para garantizar la trazabilidad de todas las operaciones realizadas por los usuarios. Esto es fundamental para la seguridad, el cumplimiento de normativas y el seguimiento de actividades críticas dentro de la aplicación.

## What Changes

- Registro automático de todas las operaciones de usuario (Login, Logout, CRUD de usuarios/roles, etc.).
- Almacenamiento persistente de eventos con: fecha, hora, usuario, actividad e información detallada.
- Implementación de un buscador avanzado en la interfaz de usuario con filtros combinados (fecha, usuario, actividad).
- Actualización de la documentación global del sistema (`Carpeta_Documentacion.md`) para incluir la sección T06a.

## Capabilities

### New Capabilities
- `audit-logging`: Registro integral de operaciones y visualización de bitácora con capacidades de filtrado avanzado.

### Modified Capabilities
- (ninguna)

## Impact

- **BE**: Entidad `EventoBitacora` para el modelo de datos.
- **BLL**: `BitacoraBLL` para la lógica de registro y consulta (implementando patrón Observer).
- **DAL**: `BitacoraDAL` para la persistencia de eventos.
- **MPP**: `BitacoraMPP` para el mapeo de datos de bitácora.
- **UI**: `UC_Bitacora` para la visualización y búsqueda de eventos.
- **Documentación**: Actualización de `Carpeta_Documentacion.md`.
