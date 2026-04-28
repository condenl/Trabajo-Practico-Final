## MODIFIED Requirements

### Requirement: Mermaid Diagram Integration
The documentation SHALL embed Mermaid diagrams for the Component Architecture, ERD (Persistence Schema), and Navigation Map.

#### Scenario: View diagrams
- **WHEN** rendering the Markdown file
- **THEN** at least three Mermaid diagrams (Component, ERD, Navigation) MUST be visible and accurate.

#### Scenario: Diagram Source Consolidation
- **WHEN** all diagrams are embedded in `Carpeta_Documentacion.md`
- **THEN** the external source files in the `Diagramas/` directory MUST be removed to ensure a single source of truth.
