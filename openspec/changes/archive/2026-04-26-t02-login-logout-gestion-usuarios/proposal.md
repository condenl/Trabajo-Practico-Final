## Why

The system requires a formal, secure, and audited authentication and session management mechanism. This change provides the foundational security layer, ensuring that user identity is verified, profiles are correctly assigned, and the entire application lifecycle (startup, login, logout, shutdown) is properly managed and audited.

## What Changes

- **Login/Logout Policy**: Formalization of the authentication flow, session duration, and cleanup procedures.
- **Session Management (Singleton)**: Ensuring a single, thread-safe global access point for the active user session in `Servicios.Session`.
- **System Lifecycle Orchestration**:
  - **Startup**: Database connectivity and integrity checks in `Program.Main`.
  - **Login**: Credential verification, profile assignment, and audit trail.
  - **Logout**: Session destruction and cleanup in `UsuarioBLL.Logout`.
  - **Shutdown**: Final audit logging and resource release.
- **UI Integration**: Adaptation of the Login form and MDI Principal form to respect the authenticated profile.
- **Documentation**: Comprehensive update of `Carpeta_Documentacion.md` following `carpeta_modelo.md` for the T02 section.

## Capabilities

### New Capabilities
- `user-authentication`: Robust verification of identity through encrypted credentials and account locking policies.
- `session-management`: Singleton implementation for global session state.
- `system-lifecycle-management`: Automated processes for system boot, user session transitions, and controlled shutdown.

## Impact

- **BE**: Entities like `Usuario` and `Rol` are central to session state.
- **BLL**: `UsuarioBLL` implements the `Login`/`Logout` logic and audit notifications.
- **DAL**: `UsuarioDAL` manages credential retrieval and failed login tracking.
- **Servicios**: `Session` (Singleton) and `Encriptador` provide core security services.
- **Documentation**: Major update to `Carpeta_Documentacion.md`.
