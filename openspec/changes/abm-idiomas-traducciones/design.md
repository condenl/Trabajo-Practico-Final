## Context

El sistema tiene base de datos con tablas `Idioma` y `Traduccion` y una arquitectura multicapas en C#. La traducción de etiquetas en tiempo de ejecución funciona a través del patrón Observer (`GestorIdioma` y la interfaz `IIdiomaObserver`). Sin embargo, no hay soporte de UI para modificar los idiomas y traducciones.

Se propone crear la interfaz `UC_Traducciones` y los métodos CRUD necesarios en la BLL y DAL para habilitar la administración de estos datos sensibles.

## Goals / Non-Goals

**Goals:**
- Agregar el tipo de permiso `Traduccion_Gestion` en `BE.Seguridad.TipoPermiso` para controlar la visibilidad del botón "Traducciones" en `FormPrincipal`.
- Crear el componente `UC_Traducciones` con dos tablas (una para Idiomas y otra para Traducciones).
- Permitir la adición, edición y borrado de registros de Idiomas y Traducciones mediante operaciones CRUD en las clases `IdiomaBLL`, `TraduccionBLL`, `IdiomaDAL`, y `TraduccionDAL`.
- Asegurar que al eliminar un idioma se borren en cascada las traducciones dependientes para mantener la integridad referencial.

**Non-Goals:**
- Proveer traducción automática usando servicios externos en la nube (ej: Google Translate API). Las traducciones deben ingresarse manualmente por el usuario.

## Decisions

### 1. Inclusión de la opción Traduccion_Gestion en el Enum de Permisos
- **Decisión**: Añadir `Traduccion_Gestion` al enum `TipoPermiso` en `BE.Seguridad.TipoPermiso.cs`.
- **Razón**: Permite gobernar la visibilidad del botón de acceso a la pantalla usando el mismo modelo recursivo de permisos que ya tiene el sistema (`usuario.TienePermiso(...)`).

### 2. Estructura de la pantalla UC_Traducciones
- **Decisión**: Utilizar dos `DataGridView` en la misma pantalla. El primero lista los idiomas y, al seleccionar uno, el segundo refresca y lista las traducciones correspondientes a ese idioma. Se agregarán campos de entrada de texto y botones (Guardar, Modificar, Eliminar) para cada sección.
- **Razón**: Es una interfaz clásica de maestro-detalle sumamente intuitiva y simple de codificar en WinForms sin requerir múltiples diálogos adicionales.

### 3. Operaciones CRUD en BLL/DAL existentes
- **Decisión**: Reutilizar y expandir las clases `IdiomaBLL`, `IdiomaDAL`, `TraduccionBLL` y `TraduccionDAL` incorporando los métodos `Insertar`, `Modificar` y `Eliminar` por medio de **Stored Procedures** (`sp_Idioma_Insertar`, `sp_Idioma_Modificar`, `sp_Idioma_Eliminar`, `sp_Traduccion_Insertar`, `sp_Traduccion_Modificar`, `sp_Traduccion_Eliminar`).
- **Razón**: Sigue las directivas de seguridad corporativa y de auditoría del proyecto que prohíben el uso de sentencias SQL directas en el código de aplicación.

## Risks / Trade-offs

- **[Riesgo] Pérdida de integridad referencial al eliminar un idioma** → **[Mitigación]** La eliminación de un idioma en la base de datos se debe hacer de forma segura. El Stored Procedure `sp_Idioma_Eliminar` se encarga de eliminar primero todos los registros de la tabla `Traduccion` asociados a dicho idioma (`IdIdioma`) antes de borrar el registro de `Idioma`, previniendo errores de FK.
