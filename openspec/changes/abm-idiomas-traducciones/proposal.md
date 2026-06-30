## Why

Actualmente, el sistema cuenta con soporte para multi-idioma (Español e Inglés) y almacenamiento de traducciones en la base de datos, pero no provee ninguna interfaz gráfica para que los administradores gestionen (ABM - Agregar, Borrar, Modificar) los idiomas y las traducciones. Toda adición o modificación de idiomas y etiquetas debe realizarse de forma manual mediante consultas directas SQL, lo que atenta contra la usabilidad y la autonomía de administración del sistema.

## What Changes

- Agregar el valor de permiso `Traduccion_Gestion` en el enumerador de seguridad del sistema.
- Incorporar un botón "Traducciones" en el menú principal (`FormPrincipal`) que sólo sea visible para los usuarios que posean el permiso `Traduccion_Gestion`.
- Crear un nuevo control de usuario `UC_Traducciones` que contenga la interfaz para la gestión de idiomas y traducciones.
- Proveer en `UC_Traducciones` una vista de tabla (DataGrid / DataGridView) para visualizar, agregar, editar y eliminar Idiomas y sus correspondientes Traducciones.
- Implementar la lógica del negocio (BLL) y de acceso a datos (DAL) necesaria para insertar, actualizar y borrar físicamente Idiomas y Traducciones de la base de datos.

## Capabilities

### New Capabilities
- `abm-idiomas-traducciones`: Permite la gestión y mantenimiento (ABM) de idiomas y etiquetas de traducción del sistema por medio de una interfaz gráfica exclusiva para administradores con el permiso correspondiente.

### Modified Capabilities

## Impact

- `BE.Seguridad.TipoPermiso`: Se agrega la opción `Traduccion_Gestion`.
- `UI.FormPrincipal`: Se agrega el botón "Traducciones" controlado por el permiso de gestión de traducciones.
- `UI.UserControls.UC_Traducciones`: Nuevo control de usuario que concentra la pantalla de ABM.
- `BLL.IdiomaBLL` & `BLL.TraduccionBLL`: Se agregan métodos de modificación (Alta, Baja, Modificación) que hoy no existen.
- `DAL.IdiomaDAL` & `DAL.TraduccionDAL`: Se agregan comandos SQL / Stored Procedures para inserción, modificación y eliminación física de Idiomas y Traducciones.
