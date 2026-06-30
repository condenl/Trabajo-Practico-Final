## Context

The current architectural documentation in `Carpeta_Documentacion.md` contains significant discrepancies with the implementation. The most critical issue is the presence of a `SessionManager` class in the diagrams and descriptions which does not exist in the source code. Simultaneously, modern patterns like the Composite pattern for security roles and the Observer pattern for multi-language support are implemented but lack visual representation.

## Goals / Non-Goals

**Goals:**
- **Full Synchronization**: Align Mermaid diagrams with class signatures in `BE`, `BLL`, and `DAL`.
- **Pattern Visualization**: Explicitly model the Composite pattern for security and the Observer pattern for localization.
- **Integrity Modeling**: Include `DVH` and `DVV` in the domain and data diagrams.
- **Service Layer Expansion**: Add `SeguridadBLL` to the business logic diagrams.
- **Auditing Strategy**: Visualize the `UsuarioAuditoria` entity.
- **Cleanup**: Remove all legacy `SessionManager` artifacts.

**Non-Goals:**
- Implementation of new business logic (except for the simulated `UsuarioAuditoria` class if needed for the demo).
- Changing the existing project structure.

## Decisions

1. **Decision: Unified Mermaid Diagram Update.**
   - **Rationale**: Centralize all diagram updates in `Carpeta_Documentacion.md` to maintain a single source of truth.
   - **Alternatives**: Creating separate `.mermaid` files (rejected for consistency with current docs).

2. **Decision: Simulation of UsuarioAuditoria.**
   - **Rationale**: To fulfill the auditing requirement without introducing breaking changes to the database or existing logic, `UsuarioAuditoria` will be modeled as a conceptual entity in the diagrams and a simple DTO if necessary.
   - **Alternatives**: Implementing a full persistence layer for auditing (out of scope).

3. **Decision: Pattern-Centric Subgraphs.**
   - **Rationale**: Use Mermaid subgraphs to highlight the **Composite Pattern** (Componente/Familia/Patente) and the **Observer Pattern** (GestorIdioma/IIdiomaObserver) to improve readability.

4. **Decision: Systematic Removal of SessionManager.**
   - **Rationale**: All text and diagram references to `SessionManager` will be replaced with the actual session management logic (likely `Session.Instance` or direct `Usuario` handling).

## Risks / Trade-offs

- **[Risk]**: Diagram complexity increasing due to pattern details.
  - **Mitigation**: Use modular diagrams per layer (BE, BLL, DAL) instead of one giant diagram.
- **[Risk]**: Inconsistency between new diagrams and old descriptive text.
  - **Mitigation**: Perform a global search and replace for "SessionManager" and related legacy terms.
