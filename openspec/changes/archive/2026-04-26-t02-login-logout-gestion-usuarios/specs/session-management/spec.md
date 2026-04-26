## ADDED Requirements

### Requirement: Singleton Session Instance
The system SHALL maintain a single, globally accessible instance of the active user session.

#### Scenario: Accessing Active User
- **WHEN** any component requests `Session.Instance.Usuario`
- **THEN** it SHALL receive the same `Usuario` object that was set during login

### Requirement: Role-Based Authorization State
The system SHALL load the full role and permission tree for the authenticated user and store it in the session.

#### Scenario: Profile Loading
- **WHEN** a user logs in
- **THEN** the `Session` object SHALL contain the `Usuario` with its corresponding `RolAsignado` populated

### Requirement: Session Termination (Logout)
The system SHALL provide a mechanism to clear the active session and return to the login state.

#### Scenario: User Logout
- **WHEN** the user selects "Logout" or closes the main form
- **THEN** the `Session` instance SHALL be cleared and the Login form SHALL be displayed
