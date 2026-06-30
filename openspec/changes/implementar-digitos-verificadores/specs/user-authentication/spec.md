## MODIFIED Requirements

### Requirement: User Identity Verification
The system SHALL verify user identity by comparing provided email and password against stored credentials ONLY IF the database integrity check has been performed and passed. Passwords MUST be hashed before comparison. Use cases for this process MUST be documented with diagrams and descriptions.

#### Scenario: Successful Authentication
- **WHEN** a user enters valid email and password AND integrity check has passed
- **THEN** the system SHALL grant access and initialize the session

#### Scenario: Authentication Blocked by Integrity Failure
- **WHEN** the integrity check has failed
- **THEN** the system SHALL NOT present the login screen and SHALL NOT allow any identity verification attempts.

#### Scenario: Invalid Credentials
- **WHEN** a user enters incorrect email or password AND integrity check has passed
- **THEN** the system SHALL deny access, increment failed attempts, and log the event

#### Scenario: Documentation Check
- **WHEN** reviewing the documentation for authentication
- **THEN** use case UC-T02-01 (Iniciar Sesión) MUST be present with its Mermaid diagram and analytical table.
