## 1. Documentation Update (Carpeta_Documentacion.md)

- [x] 1.1 Formalize section `T02. Gestión de Login/Logout y Gestión de Usuarios` in `Carpeta_Documentacion.md`.
- [x] 1.2 Document the **Login Policy**: Flow of credential verification, hashing (SHA256), and account locking (4 attempts).
- [x] 1.3 Document the **Logout Policy**: Session cleanup, Singleton destruction, and audit notification.
- [x] 1.4 Detail the **System Lifecycle** processes:
    - **Startup**: `IntegrityBLL.CheckDatabaseConnection` and `STARTUP` event logging.
    - **Shutdown**: Ensuring `LOGOUT` and `SYSTEM_CLOSE` events are registered upon closing `FormPrincipal`.
- [x] 1.5 Embed Mermaid diagrams:
    - Class Diagram focusing on `Session` (Singleton) and `UsuarioBLL`.
    - Sequence Diagram for the authentication flow.
    - Updated DER showing `USUARIO` and `ROL` relationship.
- [x] 1.6 Describe algorithmic design for `UsuarioBLL.Login` and `Session` pattern.

## 2. Code Verification (No functional changes expected)

- [x] 2.1 Verify `Servicios.Session` thread-safety (lock) matches documentation.
- [x] 2.2 Confirm `BLL.UsuarioBLL.Login` correctly suma intentos fallidos and notifies Bitacora.
- [x] 2.3 Ensure `UI.Program.Main` correctly handles the database connection check failure.

## 3. Final Validation

- [x] 3.1 Perform a full read-through of the T02 section to ensure it follows `carpeta_modelo.md` structure.
- [x] 3.2 Verify that diagrams are rendered correctly in Markdown.
