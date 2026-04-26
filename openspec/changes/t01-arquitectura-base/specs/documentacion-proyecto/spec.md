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
