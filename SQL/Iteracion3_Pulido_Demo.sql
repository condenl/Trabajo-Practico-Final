/* Idempotente. Nuevos permisos administrativos y textos predeterminados del acceso.
   No modifica usuarios, claves, roles operativos ni traducciones personalizadas. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Permisos TABLE (Nombre NVARCHAR(150), Codigo NVARCHAR(100));
INSERT @Permisos VALUES
    (N'Ver auditoría de usuarios', N'Auditoria_Ver'),
    (N'Restaurar usuarios auditados', N'Auditoria_Restaurar'),
    (N'Gestionar soporte', N'Soporte_Gestion');
INSERT dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
SELECT p.Nombre, N'Patente', p.Codigo FROM @Permisos p
WHERE NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente c WHERE c.Tipo=N'Patente' AND c.PermisoAsociado=p.Codigo);

-- El rol administrativo que ya distribuye el proyecto conserva estas funciones.
INSERT dbo.Seguridad_Familia_Hijo (IdPadre, IdHijo)
SELECT padre.Id, hijo.Id
FROM dbo.Seguridad_Componente padre CROSS JOIN dbo.Seguridad_Componente hijo
WHERE padre.Tipo=N'Familia' AND padre.Nombre=N'Administrador'
  AND hijo.Tipo=N'Patente' AND hijo.PermisoAsociado IN (N'Auditoria_Ver',N'Auditoria_Restaurar',N'Soporte_Gestion')
  AND NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Familia_Hijo l WHERE l.IdPadre=padre.Id AND l.IdHijo=hijo.Id);

DECLARE @Textos TABLE (Idioma NVARCHAR(100), Etiqueta NVARCHAR(100), Anterior NVARCHAR(1000), Nuevo NVARCHAR(1000));
INSERT @Textos VALUES
 (N'Español',N'login_title',N'SCA: SISTEMA DE CONTROL DE ACCESO',N'Perfumería'),
 (N'Español',N'login_welcome',N'Bienvenido al sistema de gestión de edificios inteligentes! Inicia sesión para continuar.',N'Gestioná compras, inventario, ventas y proveedores. Iniciá sesión para continuar.'),
 (N'Español',N'login_form_title',N'Login',N'Perfumería · Iniciar sesión'),
 (N'Español',N'login_user',N'Usuario',N'Correo electrónico'),
 (N'Español',N'login_no_user',N'¿Aún no tenés un usuario? Create una cuenta haciendo click abajo',N'¿Todavía no tenés una cuenta?'),
 (N'Español',N'login_help',N'En caso de no poder iniciar sesión',N'¿Necesitás ayuda para acceder?'),
 (N'English',N'login_title',N'SCA: ACCESS CONTROL SYSTEM',N'Perfumery'),
 (N'English',N'login_welcome',N'Welcome to the smart building management system! Log in to continue.',N'Manage purchases, inventory, sales and suppliers. Sign in to continue.'),
 (N'English',N'login_form_title',N'Login',N'Perfumery · Sign in'),
 (N'English',N'login_user',N'User',N'Email'),
 (N'English',N'login_no_user',N'Don''t have an account yet? Create one by clicking below',N'Don''t have an account yet?'),
 (N'English',N'login_help',N'If you cannot log in',N'Need help signing in?');
UPDATE t SET Texto=p.Nuevo
FROM dbo.Traduccion t JOIN dbo.Idioma i ON i.Id=t.IdIdioma
JOIN @Textos p ON p.Idioma=i.Nombre AND p.Etiqueta=t.NombreEtiqueta
WHERE t.Texto=p.Anterior;
INSERT dbo.Traduccion (IdIdioma, NombreEtiqueta, Texto)
SELECT i.Id,p.Etiqueta,p.Nuevo FROM @Textos p JOIN dbo.Idioma i ON i.Nombre=p.Idioma
WHERE NOT EXISTS (SELECT 1 FROM dbo.Traduccion t WHERE t.IdIdioma=i.Id AND t.NombreEtiqueta=p.Etiqueta);

COMMIT TRANSACTION;
GO
