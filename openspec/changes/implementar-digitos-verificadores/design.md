## Context

The system currently stores user data and authentication information without any mechanism to detect external modifications (SQL injection, manual DB edits). To comply with security requirements, a check digit system (DVH/DVV) is needed to ensure data integrity.

## Goals / Non-Goals

**Goals:**
- Detect unauthorized additions, deletions, or modifications in the `Usuario` table.
- Detect swapping of data between records or fields.
- Prevent application usage if integrity is compromised.
- Use SHA-256 for cryptographic hashing.

**Non-Goals:**
- Implementing check digits for all entities (only `Usuario` for now).
- Automatic recovery of corrupted data (detection and notification only).

## Decisions

### 1. Cryptographic Algorithm
- **Decision**: Use SHA-256.
- **Rationale**: Provides a strong collision-resistant hash that is widely supported in .NET (`System.Security.Cryptography`).

### 2. DVH Calculation Logic
- **Decision**: The DVH for a `Usuario` record will be calculated by iterating through sensitive attributes (Email, Nombre, Apellido, Password, etc.) and including their ordinal position and each character's position.
- **Algorithm**: For each attribute at index `i`, and each character at index `j`, the value `v` will be used to form a string `v + i + j`. All these strings will be concatenated and hashed.
- **Rationale**: Meets the requirement of detecting position-swapping both between attributes and within attribute values.

### 3. DVV Storage and Logic
- **Decision**: Create a `DVV` table with columns `NombreEntidad` (PK) and `Valor`.
- **Rationale**: Allows scalability if more entities need vertical check digits in the future. The `Usuario` DVV will be the SHA-256 hash of all `Usuario.DVH` values concatenated in a deterministic order (e.g., by ID).

### 4. Integration into UI Lifecycle
- **Decision**: Modify the entry point (`Program.cs`) to call `UsuarioBLL.Instancia.VerificarIntegridad()` before showing `LoginForm`.
- **Rationale**: Ensures the system is in a valid state before any user interaction occurs. The logic is delegated to `Servicios.Integridad`.

### 5. Centralization of Integrity Logic
- **Decision**: Centralize all integrity logic (calculation and verification) in a dedicated service (`Servicios.Integridad`) and remove `IntegridadBLL`.
- **Rationale**: By moving the mathematical and cryptographic logic (including the verification loop) to a cross-cutting service, we promote maximum reuse and a lean architecture. `UsuarioBLL` acts as an orchestrator for the UI-startup check.

## Risks / Trade-offs

- **[Risk] Performance on Startup** → Calculating hashes for thousands of users might delay application launch. **Mitigation**: Use efficient string building (`StringBuilder`) and perform the check in a single DB pass if possible.
- **[Risk] Manual Database Maintenance** → DBAs or developers might break integrity during migrations. **Mitigation**: Document the recalculation process and provide an administrative function to update DVHs/DVV after authorized changes.
- **[Trade-off] String vs Binary Storage** → Storing hashes as strings (Hex) is easier for debugging but takes more space. **Decision**: Use Hex strings for simplicity in the current implementation scope.
