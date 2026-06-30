## ADDED Requirements

### Requirement: Botón de Acceso en FormPrincipal
El menú principal (`FormPrincipal`) SHALL incluir un botón etiquetado "Traducciones" (o su traducción al idioma activo). Este botón SHALL ser visible únicamente para aquellos usuarios que cuenten con el permiso `Traduccion_Gestion`.

#### Scenario: Usuario con permiso Traduccion_Gestion inicia sesión
- **WHEN** un usuario que posee el permiso `Traduccion_Gestion` inicia sesión con éxito
- **THEN** el botón "Traducciones" en el FormPrincipal SHALL ser visible para el usuario

#### Scenario: Usuario sin permiso Traduccion_Gestion inicia sesión
- **WHEN** un usuario que no posee el permiso `Traduccion_Gestion` inicia sesión con éxito
- **THEN** el botón "Traducciones" en el FormPrincipal SHALL permanecer oculto para el usuario

### Requirement: Vista de ABM de Idiomas y Traducciones
El sistema SHALL contar con un control de usuario `UC_Traducciones` que contenga tablas (DataGridView) con el listado de todos los idiomas y las traducciones registradas en el sistema.

#### Scenario: Visualización del ABM de Traducciones
- **WHEN** el usuario hace clic en el botón "Traducciones"
- **THEN** el sistema SHALL cargar y mostrar el control `UC_Traducciones` visualizando los idiomas y las etiquetas existentes en formato tabla

### Requirement: Operaciones de ABM para Idiomas
El sistema SHALL permitir agregar, modificar y eliminar físicamente Idiomas desde la interfaz de usuario `UC_Traducciones`.

#### Scenario: Agregar un nuevo Idioma
- **WHEN** el usuario ingresa el nombre de un nuevo idioma (ej: "Portugués"), especifica si es por defecto, y confirma la acción de guardado
- **THEN** el sistema SHALL validar el ingreso, persistir el nuevo idioma en la base de datos y refrescar la tabla de idiomas

#### Scenario: Modificar un Idioma existente
- **WHEN** el usuario selecciona un idioma existente, edita sus datos y confirma la modificación
- **THEN** el sistema SHALL actualizar el idioma en la base de datos y refrescar la tabla

#### Scenario: Eliminar un Idioma existente
- **WHEN** el usuario selecciona un idioma existente y confirma la eliminación
- **THEN** el sistema SHALL eliminar físicamente el idioma y sus traducciones asociadas de la base de datos y refrescar la vista

### Requirement: Operaciones de ABM para Traducciones
El sistema SHALL permitir agregar, modificar y eliminar Traducciones para cada etiqueta del idioma seleccionado.

#### Scenario: Agregar una nueva Traducción
- **WHEN** el usuario ingresa una nueva etiqueta, selecciona el idioma, ingresa el texto traducido y confirma la acción
- **THEN** el sistema SHALL persistir la traducción en la base de datos y actualizar la lista de traducciones

#### Scenario: Modificar una Traducción existente
- **WHEN** el usuario selecciona una traducción existente, edita el texto traducido y confirma
- **THEN** el sistema SHALL actualizar el texto de la traducción en la base de datos y refrescar la vista

#### Scenario: Eliminar una Traducción
- **WHEN** el usuario selecciona una traducción y confirma la eliminación
- **THEN** el sistema SHALL eliminar físicamente el registro de traducción de la base de datos y actualizar la vista
