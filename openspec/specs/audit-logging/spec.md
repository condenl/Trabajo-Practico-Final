## ADDED Requirements

### Requirement: Registro Automático de Eventos
El sistema DEBE registrar automáticamente todas las operaciones críticas realizadas por los usuarios para garantizar la auditoría total. Los eventos mínimos a registrar son: LOGIN, LOGIN_FALLIDO, LOGOUT, ALTA_USUARIO, MODIFICACION, BLOQUEO y SEGURIDAD. El proceso de consulta de estos eventos DEBE estar documentado.

#### Scenario: Registro de Inicio de Sesión Exitoso
- **WHEN** el usuario ingresa sus credenciales correctamente y presiona "Ingresar"
- **THEN** el sistema DEBE registrar un evento con actividad "LOGIN" indicando el email del usuario y la fecha/hora actual.

#### Scenario: Registro de Intento Fallido
- **WHEN** el usuario ingresa una contraseña incorrecta
- **THEN** el sistema DEBE registrar un evento con actividad "LOGIN_FALLIDO" detallando que la contraseña fue incorrecta.

#### Scenario: Verificación de Caso de Uso Consulta Bitácora
- **WHEN** se revisa la documentación de bitácora
- **THEN** el caso de uso UC-T06a-01 (Consultar Bitácora) DEBE estar presente con su diagrama Mermaid y tabla analítica.

### Requirement: Búsqueda y Filtrado de Bitácora
El sistema DEBE permitir a los administradores realizar búsquedas en la bitácora utilizando filtros combinados de fecha (desde/hasta), usuario y tipo de actividad.

#### Scenario: Búsqueda por Rango de Fechas y Actividad
- **WHEN** el administrador selecciona un rango de fechas "01/04/2026" a "26/04/2026" y la actividad "BLOQUEO"
- **THEN** el sistema DEBE mostrar únicamente los eventos ocurridos en ese periodo que correspondan a bloqueos de cuenta.

#### Scenario: Búsqueda por Usuario Específico
- **WHEN** el administrador ingresa el email de un usuario en el filtro de búsqueda
- **THEN** el sistema DEBE listar cronológicamente todas las actividades realizadas por dicho usuario.

### Requirement: Visualización Detallada
La interfaz de bitácora DEBE mostrar de forma clara los campos: Fecha y Hora, Usuario, Actividad e Información Asociada (Detalle).

#### Scenario: Visualización en Grilla
- **WHEN** se ejecuta una búsqueda en la bitácora
- **THEN** los resultados DEBEN mostrarse en una grilla donde la columna "Detalle" sea auto-ajustable para facilitar la lectura de la información asociada.
