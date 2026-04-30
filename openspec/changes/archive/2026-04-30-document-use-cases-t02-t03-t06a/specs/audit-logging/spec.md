## MODIFIED Requirements

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
