## Context

El sistema requiere un registro exhaustivo de auditoría (Bitácora) para todas las operaciones realizadas por los usuarios. La arquitectura actual del proyecto ya contempla las capas necesarias (BE, BLL, DAL, MPP, UI) y utiliza el patrón Observer para desacoplar la lógica de negocio del registro de auditoría.

## Goals / Non-Goals

**Goals:**
- Implementar un sistema de auditoría centralizado.
- Garantizar que todas las operaciones críticas (Login, Logout, CRUD de Usuarios) sean registradas automáticamente.
- Proporcionar una interfaz de búsqueda con filtros combinados para el análisis de la bitácora.
- Seguir los estándares de arquitectura en 4 capas definidos para el proyecto.

**Non-Goals:**
- Implementar "Control de cambios" (T06b), el cual es una extensión futura para trazabilidad de estados de entidades.
- Implementar mecanismos de limpieza automática de bitácora en esta etapa.

## Decisions

- **Patrón Observer**: `UsuarioBLL` actúa como sujeto observado (`IObservado`) y `BitacoraBLL` actúa como observador (`IObserver`). Esto permite que cualquier acción en la gestión de usuarios notifique automáticamente a la bitácora sin acoplamiento fuerte.
- **Arquitectura en Capas**:
    - **BE**: Clase `EventoBitacora` que encapsula: Id, FechaHora, UsuarioEmail, Actividad e InformacionAsociada.
    - **BLL**: `BitacoraBLL` implementada como Singleton para coordinar el registro y la consulta.
    - **DAL**: `BitacoraDAL` que interactúa con SQL Server mediante procedimientos almacenados (`sp_Bitacora_Insertar`, `sp_Bitacora_Consultar`).
    - **MPP**: `BitacoraMPP` para el mapeo de `DataTable` a objetos `EventoBitacora`.
    - **UI**: `UC_Bitacora` como User Control para visualización y filtrado.
- **Búsqueda Combinada**: Se utiliza una lógica en el procedimiento almacenado que acepta parámetros nulos, permitiendo filtrar opcionalmente por Rango de Fechas, Usuario y/o Tipo de Actividad de forma simultánea.

## Risks / Trade-offs

- **Carga de Base de Datos** → El registro de cada operación añade un INSERT a la BD. Mitigation: Se utilizan procedimientos almacenados optimizados y el registro es asíncrono desde el punto de vista del flujo de la UI (aunque síncrono en BLL para garantizar el registro).
- **Crecimiento de la Tabla** → La tabla de bitácora puede crecer rápidamente. Mitigation: En etapas posteriores (T07/T08) se podrían considerar políticas de archivado o purga.
