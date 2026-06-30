## 1. Modificaciones de Seguridad y Menú Principal

- [x] 1.1 Agregar el permiso Traduccion_Gestion al enum BE.Seguridad.TipoPermiso.cs
- [x] 1.2 Agregar el botón btnTraducciones en FormPrincipal y vincularlo en FormPrincipal.Designer.cs
- [x] 1.3 Implementar el control de visibilidad del botón btnTraducciones según el permiso en FormPrincipal.cs

## 2. Capa de Acceso a Datos (DAL)

- [x] 2.1 Implementar métodos Insertar, Modificar y Eliminar en DAL/IdiomaDAL.cs
- [x] 2.2 Implementar métodos Insertar, Modificar y Eliminar en DAL/TraduccionDAL.cs

## 3. Capa de Negocio (BLL)

- [x] 3.1 Implementar métodos Insertar, Modificar y Eliminar en BLL/IdiomaBLL.cs
- [x] 3.2 Implementar métodos Insertar, Modificar y Eliminar en BLL/TraduccionBLL.cs

## 4. Control de Usuario de ABM (UC_Traducciones)

- [x] 4.1 Crear la interfaz gráfica del control de usuario UC_Traducciones (DataGridViews de Idiomas y Traducciones, entradas de texto y botones CRUD)
- [x] 4.2 Registrar UC_Traducciones.cs y UC_Traducciones.Designer.cs en UI/UI.csproj
- [x] 4.3 Implementar los eventos de botones para Idiomas (Agregar, Modificar, Eliminar) en UC_Traducciones.cs
- [x] 4.4 Implementar los eventos de botones para Traducciones (Agregar, Modificar, Eliminar) en UC_Traducciones.cs
- [x] 4.5 Implementar traducción dinámica e interfaz IIdiomaObserver en UC_Traducciones.cs
