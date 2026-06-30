## ADDED Requirements

### Requirement: Horizontal Check Digit (DVH)
The system SHALL calculate a horizontal check digit for each record in the `Usuario` table to ensure the integrity of the individual data fields. The calculation MUST use a SHA-based algorithm and include the content of the attributes, the position of each character, and the position of the attribute within the entity to prevent position-swapping attacks.

#### Scenario: Update User Data
- **WHEN** a user's data is modified through the system
- **THEN** the system SHALL recalculate the DVH for that record and update it in the database.

### Requirement: Vertical Check Digit (DVV)
The system SHALL calculate a vertical check digit for the `Usuario` table to detect the addition, removal, or swapping of records. The DVV MUST be a SHA-based hash of the aggregate DVH values for that table and MUST be stored in a separate `DVV` entity.

#### Scenario: Recalculate DVV
- **WHEN** any record's DVH is updated or a new record is added
- **THEN** the system SHALL recalculate the DVV for the entire table and update the `DVV` entity.

### Requirement: Database Integrity Verification at Startup
The system SHALL perform a full integrity check of the `Usuario` table at application startup, before allowing access to the login screen. This check involves recalculating all DVHs and the DVV and comparing them with the stored values.

#### Scenario: Integrity Check Success
- **WHEN** the calculated DVHs and DVV match the stored values
- **THEN** the system SHALL proceed to the login screen.

### Requirement: Integrity Breach Handling
The system SHALL detect any mismatch between calculated and stored check digits. If an error is detected, the system MUST block normal application flow and notify the administrator.

#### Scenario: Integrity Breach Detected
- **WHEN** a recalculated DVH or DVV does not match the stored value
- **THEN** the system SHALL display a critical error message, record the event in the Bitacora as a security failure, and prevent non-admin users from accessing the system.
