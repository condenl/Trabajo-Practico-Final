## ADDED Requirements

### Requirement: System Startup Integrity
The system SHALL verify its operational integrity (database connectivity, critical configuration) before showing the login screen.

#### Scenario: Successful Startup
- **WHEN** the application starts and all integrity checks pass
- **THEN** the Login form SHALL be displayed

#### Scenario: Integrity Failure
- **WHEN** the application starts and a critical component (e.g., Database) is unavailable
- **THEN** the system SHALL display a critical error message and terminate or enter a recovery mode

### Requirement: Orderly System Shutdown
The system SHALL perform cleanup tasks and log a "SHUTDOWN" event before the application process terminates.

#### Scenario: Shutdown Audit
- **WHEN** the application is closed
- **THEN** a "SHUTDOWN" event SHALL be recorded in the Bitacora
