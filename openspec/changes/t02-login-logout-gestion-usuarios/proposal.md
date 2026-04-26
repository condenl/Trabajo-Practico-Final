## Why

The system lacks a formal authentication and session management mechanism. This change is necessary to secure the application, manage user profiles (roles), and provide an audit trail for system access, ensuring that only authorized users can perform specific actions based on their assigned permissions.

## What Changes

- **Login Mechanism**: Verification of user identity using username and encrypted password.
- **Session Management**: Implementation of the Singleton pattern to maintain the active user session globally.
- **Profile Assignment**: Automatic loading and assignment of user roles and permissions upon login.
- **Audit Logging**: Recording login/logout events and unauthorized access attempts in the bitácora.
- **System Lifecycle**: Formalizing procedures for system startup (integrity checks), login (identity verification), logout (session cleanup), and shutdown.
- **UI Updates**: Integration of a dedicated Login form and session-aware navigation in the main form.

## Capabilities

### New Capabilities
- `user-authentication`: Handles the verification of user credentials and identity.
- `session-management`: Implements the Singleton pattern to manage the global session state and active user profile.
- `system-lifecycle-management`: Orchestrates the processes for application startup, shutdown, and user session transitions.

### Modified Capabilities
- (None)

## Impact

- **BE**: `Usuario` and `Rol` entities will be central to the authentication flow.
- **Servicios**: `Session` class will be implemented as a Singleton. `LogManager` will be used for auditing.
- **BLL/DAL/MPP**: `UsuarioBLL`, `UsuarioDAL`, and `UsuarioMPP` will handle the business logic and persistence for authentication.
- **UI**: `Login` form will be the entry point, and `FormPrincipal` will adapt based on the active session.
