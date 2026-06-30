## Why

Ensure the integrity of the data stored in the database by detecting unauthorized modifications, additions, deletions, or data swaps performed outside the system. This is critical for security and reliability, especially for sensitive entities like Users.

## What Changes

- **Horizontal Check Digit (DVH)**: Implement a DVH field in the `Usuario` entity to verify the integrity of each individual record.
- **Vertical Check Digit (DVV)**: Implement a DVV mechanism and entity to detect if records have been added, removed, or swapped.
- **Integrity Algorithm**: Implement a SHA-based calculation that includes the attribute content, character position, and attribute position to prevent position-swapping attacks.
- **Startup Verification**: Add an integrity check process at application startup, before the login screen is presented.
- **Admin Notification**: Implement a mechanism to notify the administrator if an integrity error is detected, preventing access to the system until resolved.

## Capabilities

### New Capabilities
- `data-integrity-verification`: Covers the logic for DVH/DVV calculation, storage, and the verification process at startup.

### Modified Capabilities
- `user-authentication`: The authentication process will now be preceded by an integrity check of the `Usuario` entity.

## Impact

- **BE**: Update `Usuario` class to include DVH. Create `DVV` entity.
- **Servicios**: Implement a dedicated `Integridad` service to handle DVH/DVV calculations.
- **DAL**: Encapsulate the lifecycle management of integrity digits by calling the `Integridad` service during CRUD operations.
- **BLL**: Orchestrate integrity verification at startup and remove redundant `IntegridadBLL`.
- **UI**: Modify `Program.cs` or the main entry point to perform checks before `LoginForm`.
- **Database**: Add `DVH` column to `Usuario` table and create `DVV` table.
