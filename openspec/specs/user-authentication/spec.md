## ADDED Requirements

### Requirement: User Identity Verification
The system SHALL verify user identity by comparing provided email and password against stored credentials. Passwords MUST be hashed before comparison.

#### Scenario: Successful Authentication
- **WHEN** a user enters valid email and password
- **THEN** the system SHALL grant access and initialize the session

#### Scenario: Invalid Credentials
- **WHEN** a user enters incorrect email or password
- **THEN** the system SHALL deny access, increment failed attempts, and log the event

### Requirement: Account Locking
The system SHALL block a user account after 4 consecutive failed login attempts to prevent brute-force attacks.

#### Scenario: User Account Blocked
- **WHEN** a user reaches 4 failed attempts
- **THEN** the system SHALL set the user's status to Inactive and log a SECURITY alert

### Requirement: Authentication Auditing
The system SHALL record every login attempt (success or failure) in the Bitacora.

#### Scenario: Audit Login Success
- **WHEN** a user logs in successfully
- **THEN** the system SHALL record a "LOGIN" event in the Bitacora with the user's email
