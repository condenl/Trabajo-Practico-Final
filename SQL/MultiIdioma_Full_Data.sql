-- Clean up existing translations if needed
-- DELETE FROM Traduccion;
-- DELETE FROM Idioma;

-- Languages
IF NOT EXISTS (SELECT 1 FROM Idioma WHERE Nombre = 'Español')
    INSERT INTO Idioma (Nombre, EsDefault) VALUES ('Español', 1);

IF NOT EXISTS (SELECT 1 FROM Idioma WHERE Nombre = 'English')
    INSERT INTO Idioma (Nombre, EsDefault) VALUES ('English', 0);

DECLARE @EsId INT = (SELECT Id FROM Idioma WHERE Nombre = 'Español');
DECLARE @EnId INT = (SELECT Id FROM Idioma WHERE Nombre = 'English');

-- SPANISH
-- FormPrincipal & Login General
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'menu_bitacora', 'Bitácora');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'menu_logout', 'Cerrar Sesión');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'menu_usuarios', 'Usuarios');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'menu_roles', 'Roles');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'menu_soporte_admin', 'Soporte Técnico');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'main_form_title', 'Menú Principal');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_user', 'Usuario');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_confirm_logout', '¿Está seguro que desea cerrar la sesión actual?');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'title_confirm_logout', 'Confirmar Cierre de Sesión');

-- Login Form
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_welcome', 'Bienvenido al sistema de gestión de edificios inteligentes! Inicia sesión para continuar.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_title', 'SCA: SISTEMA DE CONTROL DE ACCESO');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_btn', 'Iniciar Sesión');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_pass', 'Contraseña');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_register_btn', 'Registrar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_no_user', '¿Aún no tenés un usuario? Create una cuenta haciendo click abajo');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_help', 'En caso de no poder iniciar sesión');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_soporte_btn', 'Contactá con el soporte');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_form_title', 'Login');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_complete_fields', 'Por favor, complete todos los campos.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_invalid_credentials', 'Email o contraseña incorrectos.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_welcome', 'Bienvenido');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_unexpected_error', 'Ocurrió un error inesperado');

-- UC_Bitacora
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'chk_filtrar_fechas', 'Filtrar por fechas');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_desde', 'Desde:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_hasta', 'Hasta:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_usuario', 'Usuario:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_actividad', 'Actividad:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_limpiar', 'Limpiar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_buscar', 'Buscar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_all_activities', 'Todas');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_LOGIN', 'Inicio de Sesión');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_LOGIN_FALLIDO', 'Inicio de Sesión Fallido');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_ALTA_USUARIO', 'Alta de Usuario');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_MODIFICACION', 'Modificación');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_BLOQUEO', 'Bloqueo');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_SEGURIDAD', 'Seguridad');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_error', 'Error');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_error_consulting_log', 'Error al consultar la bitácora');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_date_time', 'Fecha y Hora');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_user', 'Usuario');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_detail', 'Detalle');

-- UC_Registro
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_titulo_registro', 'REGISTRO DE NUEVO USUARIO');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_volver', 'Volver');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_registrar', 'Registrar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_email', 'Email');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_apellido', 'Apellido');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_nombre', 'Nombre');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_dni', 'DNI');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_password', 'Contraseña');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_user_registered_success', '¡Usuario registrado con éxito! Ya puedes iniciar sesión.');

-- UC_Roles
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_crear_rol', 'Crear Rol');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_rol', 'Rol:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_modificar_rol', 'Modificar Rol');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_eliminar_rol', 'Eliminar Rol');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_permisos', 'Permisos:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_quitar_permiso', '<< Quitar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_agregar_permiso', 'Agregar >>');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_error_loading_roles', 'Error al cargar roles');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_error_loading_permissions', 'Error al cargar permisos');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_error_loading_role_permissions', 'Error al cargar permisos del rol');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_enter_role_name', 'Ingrese un nombre para el nuevo rol.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_role_created_success', 'Rol creado con éxito.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_name_empty_warn', 'El nombre no puede estar vacío.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_role_modified_success', 'Rol modificado.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_confirm_delete_role', '¿Desea eliminar el rol ''{0}''?');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_confirm_delete_role_title', 'Confirmar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_role_deleted_success', 'Rol eliminado.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_select_role_permission_warn', 'Seleccione un rol y un permiso disponible.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_select_assigned_permission_warn', 'Seleccione un permiso asignado para quitar.');

-- UC_Soporte (Client)
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_titulo_soporte', 'FORMULARIO DE SOPORTE');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_cancelar', 'Cancelar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_enviar', 'Enviar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_tipo_problema', 'Tipo de problema');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_descripcion_problema', 'Descripción del problema');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_error_connection', 'Error de conexión');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_request_registered_success', 'Tu solicitud fue registrada con éxito. Un administrador revisará tu caso pronto.');

-- UC_SoporteAdmin
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_guardar', 'Guardar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_filter', 'FILTRO:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_detail', 'DETALLE:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_observations', 'OBSERVACIONES:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_status', 'ESTADO:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_all_requests', '-- Todas las solicitudes --');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_user_email', 'Usuario / Email');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_date', 'Fecha');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_problem', 'Problema');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_status', 'Estado');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_date', 'FECHA');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_user', 'USUARIO');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_reason', 'MOTIVO');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_report_desc', 'DESCRIPCIÓN DEL REPORTE');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_select_request_warn', 'Por favor, seleccione una solicitud de la lista para actualizarla.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_select_status_warn', 'Debe seleccionar un estado para la solicitud.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_request_updated_success', 'La solicitud fue actualizada y registrada exitosamente.');

-- UC_Usuarios
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_agregar', 'Agregar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_modificar', 'Modificar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_baja', 'Dar de baja');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'btn_alta', 'Dar de alta');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'grid_assigned_role', 'Rol Asignado');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_active', 'Activo');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_no_role', 'Sin Rol');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_user_added_success', 'Usuario agregado correctamente.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_select_user_warn', 'Debe seleccionar un usuario de la grilla.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_user_modified_success', 'Usuario modificado correctamente.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_self_deactivate_stop', 'Por cuestiones de seguridad, no podés dar de baja tu propia cuenta mientras estás en sesión.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_user_already_status', 'El usuario ya se encuentra en ese estado.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_user_status_changed_success', 'El estado del usuario fue actualizado correctamente.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_complete_all_text_fields', 'Por favor, complete todos los campos de texto.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_password_required', 'La contraseña es obligatoria para crear un usuario.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_dni_digits_only', 'El DNI debe contener solo números.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_dni_numbers_only', 'El DNI debe contener solo números.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_complete_all_fields', 'Por favor, complete todos los campos de texto.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_warning', 'Aviso');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_success', 'Éxito');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_denied', 'Acceso Denegado');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_info', 'Información');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_confirm', 'Confirmar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_notice', 'Aviso');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_critical_error', 'Error Crítico');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'lbl_permissions_error', 'Falta de Permisos');


-- ENGLISH
-- FormPrincipal & Login General
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'menu_bitacora', 'Audit Log');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'menu_logout', 'Logout');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'menu_usuarios', 'Users');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'menu_roles', 'Roles');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'menu_soporte_admin', 'Technical Support');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'main_form_title', 'Main Menu');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_user', 'User');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_confirm_logout', 'Are you sure you want to close the current session?');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'title_confirm_logout', 'Confirm Logout');

-- Login Form
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_welcome', 'Welcome to the smart building management system! Log in to continue.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_title', 'SCA: ACCESS CONTROL SYSTEM');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_btn', 'Login');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_pass', 'Password');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_register_btn', 'Register');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_no_user', 'Don''t have an account yet? Create one by clicking below');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_help', 'If you cannot log in');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_soporte_btn', 'Contact support');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_form_title', 'Login');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_complete_fields', 'Please complete all fields.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_invalid_credentials', 'Incorrect email or password.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_welcome', 'Welcome');

-- UC_Bitacora
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'chk_filtrar_fechas', 'Filter by dates');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_desde', 'From:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_hasta', 'To:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_usuario', 'User:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_actividad', 'Activity:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_limpiar', 'Clear');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_buscar', 'Search');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_all_activities', 'All');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_LOGIN', 'Login');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_LOGIN_FALLIDO', 'Failed Login');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_ALTA_USUARIO', 'User Creation');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_MODIFICACION', 'Modification');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_BLOQUEO', 'Block');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_SEGURIDAD', 'Security');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_error', 'Error');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_error_consulting_log', 'Error consulting audit log');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_date_time', 'Date and Time');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_user', 'User');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_detail', 'Detail');

-- UC_Registro
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_titulo_registro', 'NEW USER REGISTRATION');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_volver', 'Back');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_registrar', 'Register');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_email', 'Email');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_apellido', 'Last Name');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_nombre', 'First Name');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_dni', 'ID Number');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_password', 'Password');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_user_registered_success', 'User successfully registered! You can now log in.');

-- UC_Roles
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_crear_rol', 'Create Role');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_rol', 'Role:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_modificar_rol', 'Modify Role');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_eliminar_rol', 'Delete Role');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_permisos', 'Permissions:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_quitar_permiso', '<< Remove');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_agregar_permiso', 'Add >>');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_error_loading_roles', 'Error loading roles');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_error_loading_permissions', 'Error loading permissions');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_error_loading_role_permissions', 'Error loading role permissions');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_enter_role_name', 'Enter a name for the new role.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_role_created_success', 'Role successfully created.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_name_empty_warn', 'Name cannot be empty.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_role_modified_success', 'Role successfully modified.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_confirm_delete_role', 'Do you want to delete role ''{0}''?');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_confirm_delete_role_title', 'Confirm');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_role_deleted_success', 'Role successfully deleted.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_select_role_permission_warn', 'Select a role and an available permission.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_select_assigned_permission_warn', 'Select an assigned permission to remove.');

-- UC_Soporte (Client)
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_titulo_soporte', 'SUPPORT FORM');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_cancelar', 'Cancel');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_enviar', 'Send');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_tipo_problema', 'Problem Type');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_descripcion_problema', 'Problem Description');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_error_connection', 'Connection error');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_request_registered_success', 'Your request was successfully registered. An administrator will review your case soon.');

-- UC_SoporteAdmin
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_guardar', 'Save');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_filter', 'FILTER:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_detail', 'DETAIL:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_observations', 'OBSERVATIONS:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_status', 'STATUS:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_all_requests', '-- All requests --');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_user_email', 'User / Email');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_date', 'Date');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_problem', 'Problem');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_status', 'Status');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_date', 'DATE');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_user', 'USER');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_reason', 'REASON');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_report_desc', 'REPORT DESCRIPTION');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_select_request_warn', 'Please select a request from the list to update it.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_select_status_warn', 'You must select a status for the request.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_request_updated_success', 'The request was successfully updated and registered.');

-- UC_Usuarios
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_agregar', 'Add');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_modificar', 'Modify');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_baja', 'Deactivate');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'btn_alta', 'Activate');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'grid_assigned_role', 'Assigned Role');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_active', 'Active');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_no_role', 'No Role');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_user_added_success', 'User added successfully.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_select_user_warn', 'Please select a user from the grid.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_user_modified_success', 'User modified successfully.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_self_deactivate_stop', 'For security reasons, you cannot deactivate your own account while in session.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_user_already_status', 'The user is already in that state.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_user_status_changed_success', 'User status updated successfully.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_complete_all_text_fields', 'Please complete all text fields.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_password_required', 'Password is required to create a user.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_dni_digits_only', 'ID number must contain digits only.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_dni_numbers_only', 'ID number must contain digits only.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_complete_all_fields', 'Please complete all text fields.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_warning', 'Warning');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_success', 'Success');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_denied', 'Access Denied');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_info', 'Information');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_confirm', 'Confirm');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_notice', 'Notice');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_critical_error', 'Critical Error');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'lbl_permissions_error', 'Permission Error');

-- FormIntegridad and Audit Translations
DECLARE @EsId INT = (SELECT Id FROM Idioma WHERE Nombre = 'Español');

-- Spanish (Español)
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_form_title', 'Soporte Técnico - Restauración de Integridad');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_lbl_titulo', 'Se han detectado errores de integridad en la base de datos:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_lbl_sugerencia', 'Se sugiere revisar y seleccionar los backups de la base de datos en la ruta:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_btn_recalcular', 'Recalcular DV');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_btn_restaurar', 'Restaurar Backup');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_btn_salir', 'Salir');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_no_errors', 'No se detectaron errores de integridad.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_recalcular_success', 'Dígitos verificadores (DVH y DVV) recalculados con éxito. Se reestableció la integridad de la base de datos.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_recalcular_warning', 'Se ejecutó el recalculo de DV, pero la base de datos todavía contiene errores de integridad.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_recalcular_error', 'Error al recalcular dígitos verificadores');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_restore_success', 'Base de datos restaurada con éxito e integridad validada correctamente.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_restore_warning', 'Base de datos restaurada, pero aún se detectan errores de integridad con los nuevos datos. Por favor, verifique el backup o recalcule.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_restore_error', 'Error al restaurar la base de datos');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_msg_restore_perm_error', 'Asegúrese de que el servidor SQL tiene permisos de lectura sobre el archivo seleccionado.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'integridad_title_select_backup', 'Seleccionar Backup de Base de Datos');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_RESTORE', 'Restauración de Base de Datos');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'activity_RESTORE_INTEGRIDAD', 'Restauración de Integridad');

-- English (Inglés)
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_form_title', 'Technical Support - Integrity Restore');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_lbl_titulo', 'Database integrity errors have been detected:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_lbl_sugerencia', 'It is suggested to review and select database backups in the path:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_btn_recalcular', 'Recalculate DV');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_btn_restaurar', 'Restore Backup');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_btn_salir', 'Exit');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_no_errors', 'No integrity errors detected.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_recalcular_success', 'Verifier digits (DVH and DVV) successfully recalculated. Database integrity restored.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_recalcular_warning', 'DV recalculation executed, but the database still contains integrity errors.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_recalcular_error', 'Error recalculating verifier digits');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_restore_success', 'Database successfully restored and integrity validated.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_restore_warning', 'Database restored, but integrity errors are still detected in the new data. Please verify the backup or recalculate.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_restore_error', 'Error restoring database');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_msg_restore_perm_error', 'Ensure the SQL server has read permissions on the selected file.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'integridad_title_select_backup', 'Select Database Backup');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_RESTORE', 'Database Restore');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'activity_RESTORE_INTEGRIDAD', 'Integrity Restore');

-- Translations for Languages & Translations ABM (UC_Traducciones)
-- Spanish (Español)
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'menu_traducciones', 'Traducciones');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'traducciones_gb_idiomas', 'Idiomas');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'traducciones_gb_traducciones', 'Traducciones');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'traducciones_lbl_nom_idioma', 'Nombre del Idioma:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'traducciones_chk_default', 'Idioma por Defecto');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'traducciones_lbl_etiqueta', 'Nombre de Etiqueta/Clave:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'traducciones_lbl_texto', 'Texto Traducido:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_select_language_warn', 'Debe seleccionar un idioma primero.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_select_item_warn', 'Por favor, seleccione un elemento.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_confirm_delete', '¿Está seguro de que desea eliminar este elemento?');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'msg_success_action', 'Operación completada con éxito.');

-- English (Inglés)
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'menu_traducciones', 'Translations');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'traducciones_gb_idiomas', 'Languages');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'traducciones_gb_traducciones', 'Translations');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'traducciones_lbl_nom_idioma', 'Language Name:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'traducciones_chk_default', 'Default Language');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'traducciones_lbl_etiqueta', 'Tag Name / Key:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'traducciones_lbl_texto', 'Translated Text:');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_select_language_warn', 'You must select a language first.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_select_item_warn', 'Please select an item.');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_confirm_delete', 'Are you sure you want to delete this item?');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'msg_success_action', 'Operation completed successfully.');
GO
