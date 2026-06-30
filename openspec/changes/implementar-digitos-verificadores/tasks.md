## 1. Database and Entities

- [x] 1.1 Add `DVH` column (NVARCHAR(MAX)) to the `Usuario` table in the database.
- [x] 1.2 Create the `DVV` table with columns `NombreEntidad` (PK, NVARCHAR(50)) and `Valor` (NVARCHAR(MAX)).
- [x] 1.3 Update `BE.Usuario` class to include the `DVH` property.
- [x] 1.4 Create `BE.DVV` class.

## 2. Integrity Logic Core

- [x] 2.1 Implement `IntegrityService` (or update `Servicios.Encriptador`) to provide SHA-256 hashing.
- [x] 2.2 Implement the DVH calculation algorithm (position-sensitive design) encapsulated in `DAL.UsuarioDAL`.
- [x] 2.3 Implement the DVV calculation algorithm (hash of aggregated DVHs) encapsulated in `DAL.UsuarioDAL`.

## 3. Data Access Layer (DAL)

- [x] 3.1 Update `DAL.UsuarioDAL` to include `DVH` in `Insert`, `Update`, and `Select` operations.
- [x] 3.2 Create `DAL.DVVDAL` to handle persistence for vertical check digits.
- [x] 3.3 Ensure the `UsuarioDAL` recalculates DVH and DVV upon any state-changing operation (Add/Update/Delete).

## 4. System Integration and Startup

- [x] 4.1 Implement `IntegrityBLL.VerifyIntegrity()` to perform the full database check.
- [x] 4.2 Modify `UI.Program` (main entry point) to execute the integrity check before launching the `LoginForm`.
- [x] 4.3 Implement error handling in `Program.cs` to display a critical message to the administrator and block access if integrity fails.

## 5. Verification and Testing

- [x] 5.1 Manually modify a record in the database and verify the system detects the corruption.
- [x] 5.2 Swap two records or two field values in the database and verify the system detects it.
- [x] 5.3 Test the successful path (integrity check passes) and ensure login works correctly.
