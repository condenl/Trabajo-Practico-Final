## 1. Modificaciones en el Arranque y Login

- [x] 1.1 Remover la validación de integridad al inicio del programa en Program.cs
- [x] 1.2 Implementar la comprobación de integridad pos-login en Login.cs
- [x] 1.3 Agregar lógica para redirigir a FormIntegridad (Admin) o continuar normalmente (Usuario común) en Login.cs

## 2. Capa de Negocio, Datos y Servicios

- [x] 2.1 Enriquecer el mensaje de error de DVH en Servicios/Integridad.cs desencriptando el DNI
- [x] 2.2 Implementar la función de restauración física de base de datos en DAL/Conexion.cs conectándose a master
- [x] 2.3 Implementar el método RecalcularDV en DAL/UsuarioDAL.cs para actualizar DVH afectados y recalcular DVV
- [x] 2.4 Exponer RecalcularDV y RestaurarBaseDeDatos en BLL/UsuarioBLL.cs

## 3. Interfaz de Usuario y Registro

- [x] 3.1 Crear la interfaz FormIntegridad con cuadro de texto para errores y ruta de backups
- [x] 3.2 Implementar los eventos de botones Recalcular DV, Restaurar Backup y Salir en FormIntegridad.cs
- [x] 3.3 Registrar FormIntegridad.cs y FormIntegridad.Designer.cs en UI/UI.csproj

## 4. Multi-idioma y Traducciones de Auditoría

- [x] 4.1 Implementar la interfaz IIdiomaObserver en FormIntegridad.cs y mapear las etiquetas
- [x] 4.2 Agregar las traducciones de FormIntegridad en español e inglés a la base de datos
- [x] 4.3 Generar los inserts de traducción de auditoría para RESTORE y RESTORE_INTEGRIDAD
- [x] 4.4 Persistir las sentencias de inserción en SQL/MultiIdioma_Full_Data.sql
