## ADDED Requirements

### Requirement: Composite Pattern for Security
The system SHALL implement a recursive security structure using the Composite pattern. This allows treating individual permissions (Patente) and groups of permissions (Familia) uniformly through a common interface or abstract class (Componente).

#### Scenario: Recursive Permission Check
- **WHEN** the system traverses a user's permissions
- **THEN** it SHALL treat `Familia` and `Patente` as `Componente` objects, enabling infinite nesting of roles.

### Requirement: Multi-language Observer Pattern
The system SHALL allow dynamic language switching using the Observer pattern. UI components or service managers MUST subscribe to a `GestorIdioma` to receive notifications when the global language state changes.

#### Scenario: Dynamic UI Update
- **WHEN** the user selects a new language in the UI
- **THEN** all subscribed `IIdiomaObserver` components MUST receive the `ActualizarIdioma` call and update their text.

### Requirement: Verifier Digit Integrity
The system SHALL maintain data integrity for critical tables (e.g., `Usuario`) using horizontal (DVH) and vertical (DVV) verifier digits.

#### Scenario: Integrity Verification
- **WHEN** a record is loaded
- **THEN** the system SHALL recalculate the DVH and compare it with the stored value to detect unauthorized modifications.

## MODIFIED Requirements

### Requirement: 4-Layer Architecture
The system SHALL be structured into four distinct layers: Presentation (UI), Business Logic (BLL), Data Access (DAL), and Business Entities (BE). Each layer MUST have a clear responsibility and follow a strict top-down dependency rule (UI depends on BLL, BLL on DAL/BE, DAL on BE).

#### Scenario: Verify layer separation
- **WHEN** analyzing the project structure
- **THEN** four separate projects/namespaces (UI, BLL, DAL, BE) MUST exist and follow the dependency rules.

### Requirement: Singleton Pattern in DAL
The system SHALL implement the Singleton pattern for all DAL classes (e.g., `UsuarioDAL`, `RolDAL`, `BitacoraDAL`) to ensure a single point of access and efficient resource management.

#### Scenario: Single instance access
- **WHEN** a BLL component requests an instance of a DAL class
- **THEN** it MUST receive the same instance every time via a static `ObtenerInstancia()` or similar method.

### Requirement: Observer Pattern for Auditoria
The system SHALL use the Observer pattern to decouple the logging/auditory process from the business logic. `UsuarioBLL` (and others) MUST act as an Observable, while `BitacoraBLL` MUST act as an Observer.

#### Scenario: Automatic event logging
- **WHEN** a business operation (like login) occurs in BLL
- **THEN** all registered observers (like BitacoraBLL) MUST be notified and log the event without direct coupling.

### Requirement: OOP Principles Compliance
The system code SHALL adhere to the following OOP criteria:
- **Cohesion**: Each class MUST have a single, well-defined responsibility.
- **Coupling**: Interactions between layers MUST be minimized and prefer interfaces.
- **Reusability**: Shared logic MUST be encapsulated in BE or Service layers.
- **Extensibility**: The system MUST allow adding new roles or entities with minimal changes to existing core logic.

#### Scenario: High cohesion check
- **WHEN** examining `BitacoraBLL`
- **THEN** it MUST only contain logic related to event registration and notification.
