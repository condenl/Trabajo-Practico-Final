## ADDED Requirements

### Requirement: Standardized Headers and Footers
All project documentation in `Documentacion/` SHALL include a header table matching the layout in `carpeta_modelo.md`, containing University, Faculty, Subject, Student, and Date information.

#### Scenario: Verify header format
- **WHEN** opening `Carpeta_Documentacion.md`
- **THEN** the first section MUST contain a Markdown table with the institutional metadata.

### Requirement: Revision History and Index
The main documentation file SHALL include a "Historial de Revisión" table and a clickable "Índice" (Table of Contents) as required by the institutional standards.

#### Scenario: Check index links
- **WHEN** clicking a link in the Índice
- **THEN** the browser/editor MUST navigate to the corresponding section in the document.

### Requirement: Mermaid Diagram Integration
The documentation SHALL embed Mermaid diagrams for the Component Architecture, ERD (Persistence Schema), and Navigation Map.

#### Scenario: View diagrams
- **WHEN** rendering the Markdown file
- **THEN** at least three Mermaid diagrams (Component, ERD, Navigation) MUST be visible and accurate.

#### Scenario: Diagram Source Consolidation
- **WHEN** all diagrams are embedded in `Carpeta_Documentacion.md`
- **THEN** the external source files in the `Diagramas/` directory MUST be removed to ensure a single source of truth.

#### Scenario: Simplified Component Diagram Representation
- **WHEN** rendering the Component Architecture diagram in section T01.3
- **THEN** the diagram MUST only display the component nodes and their relationships, avoiding the inclusion of internal class details or members within the nodes.

#### Scenario: Code-Documentation Alignment
- **WHEN** reviewing any diagram in `Carpeta_Documentacion.md`
- **THEN** all class names, method names, and data types MUST strictly match the current implementation in the codebase.

### Requirement: Standardized Use Case Format
All technical modules SHALL be documented using a standardized format that includes a visual Mermaid diagram and an analytical description table.

#### Scenario: Mermaid Diagram Presence
- **WHEN** a module's use cases are reviewed
- **THEN** they MUST include at least one Mermaid diagram representing the actors and functional units.

#### Scenario: Analytical Table Completeness
- **WHEN** a use case is described
- **THEN** the table MUST include: Code, Objective, Preconditions, Postconditions, Triggering Events, Normal Flow, and Alternative Flows (Exceptions).
