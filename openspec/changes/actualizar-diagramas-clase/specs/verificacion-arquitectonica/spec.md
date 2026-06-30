## ADDED Requirements

### Requirement: Diagram Balancing Verification
The system documentation SHALL be periodically audited to ensure that class diagrams are balanced with the actual code implementation. Any discrepancy in class names, public members, or relationships MUST be resolved.

#### Scenario: Catch documentation drift
- **WHEN** an automated or manual audit compares the `BE` class diagram with the `BE.dll` metadata
- **THEN** all classes in the diagram MUST be matched to existing types in the assembly.

## MODIFIED Requirements

### Requirement: Automated Layer Isolation Test
The system SHALL have automated tests that use reflection to verify that no project references or class usages violate the 4-layer architecture rules (e.g., DAL referencing UI).

#### Scenario: Catch illegal dependency
- **WHEN** a developer accidentally adds a reference from DAL to UI
- **THEN** the architectural unit test MUST fail.

### Requirement: Singleton Instantiation Test
The system SHALL have automated tests to verify that DAL classes cannot be instantiated via a public constructor and only via the Singleton accessor.

#### Scenario: Prevent direct instantiation
- **WHEN** a test attempts to instantiate `UsuarioDAL` via `new UsuarioDAL()`
- **THEN** it MUST result in a compilation error or runtime exception (if tested via reflection).
