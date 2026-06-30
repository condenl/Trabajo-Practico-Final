## ADDED Requirements

### Requirement: Representative Class Diagrams
The system documentation SHALL provide high-fidelity class diagrams for each architectural layer (BE, BLL, DAL) that match the actual code structure and class signatures.

#### Scenario: Diagram Verification
- **WHEN** comparing the diagrams in `Carpeta_Documentacion.md` with the C# class definitions
- **THEN** all classes, properties, and methods in the diagram MUST exist in the code (with the exception of simulated auditing classes).
- **THEN** the diagrams SHALL be simplified to prioritize clarity and pattern visibility over exhaustive member listing.

### Requirement: Pattern Visualization
The documentation SHALL explicitly visualize the implementation of structural and behavioral design patterns using Mermaid subgraphs or specific notations.

#### Scenario: Pattern Discovery
- **WHEN** a developer reviews the security diagram
- **THEN** the Composite pattern (Componente, Familia, Patente) MUST be clearly identifiable through its recursive relationship.

### Requirement: DVV and Integrity Relationships
The documentation SHALL show the relationships between the integrity entities (DVH, DVV), the integrity services, and the data access layer.

#### Scenario: Integrity Flow
- **WHEN** reviewing the integrity model
- **THEN** `DVV` MUST have defined relationships with `Integridad` service and `DVVDAL`.
- **THEN** `UsuarioDAL` MUST show dependency on `Integridad` service.

### Requirement: Diagram Balancing (DER and Sequence)
All architectural diagrams SHALL be balanced with each other and with the codebase.

#### Scenario: DER Balancing
- **WHEN** comparing a DER diagram with a Class Diagram or SQL schema
- **THEN** all tables and columns MUST match the entities and properties in the BE layer and the SQL scripts.
- **THEN** relationships (1:N, N:M) MUST be consistent across all structural diagrams.

#### Scenario: Sequence Diagram Balancing
- **WHEN** reviewing a Sequence Diagram
- **THEN** all participants MUST represent existing classes or interfaces.
- **THEN** all messages/calls MUST match existing method names and signatures in the BLL or DAL layers.

### Requirement: Advanced Module Documentation
The system documentation SHALL include detailed sections for advanced technical modules, providing objectives and operational descriptions.

#### Scenario: Multi-language Documentation
- **WHEN** reviewing section T05
- **THEN** a sequence diagram showing the Observer pattern notification flow MUST be present.

#### Scenario: Audit and Undo Documentation
- **WHEN** reviewing section T06b
- **THEN** the role of `UsuarioAuditoria` in the undo process MUST be explicitly explained.

#### Scenario: Integrity and Security Documentation
- **WHEN** reviewing sections T04 and T07
- **THEN** the Composite pattern for roles and the dual-level (DVH/DVV) integrity strategy MUST be documented.


### Requirement: Removal of Ghost Classes
The system documentation SHALL be purged of any references to classes that are not part of the active codebase or planned architectural roadmap.

#### Scenario: Cleanup Validation
- **WHEN** searching for "SessionManager" in the repository documentation
- **THEN** zero results MUST be returned.
