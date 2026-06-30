## MODIFIED Requirements

### Requirement: User Identity Verification
The system SHALL verify user identity by comparing provided email and password against stored credentials. Passwords MUST be hashed before comparison. Use cases for this process MUST be documented with diagrams and descriptions. The login process MUST also allow the user to select their preferred language for the session.

#### Scenario: Successful Authentication with Language Selection
- **WHEN** a user enters valid email and password and selects a language
- **THEN** the system SHALL grant access, initialize the session, and set the system language to the selection

#### Scenario: Invalid Credentials
- **WHEN** a user enters incorrect email or password
- **THEN** the system SHALL deny access, increment failed attempts, and log the event

#### Scenario: Documentation Check
- **WHEN** reviewing the documentation for authentication
- **THEN** use case UC-T02-01 (Iniciar Sesión) MUST be present with its Mermaid diagram and analytical table.
