## Context

The system is built on a 4-layer architecture (.NET 4.7.2) consisting of:
1. **UI**: WinForms (MDI)
2. **BLL**: Business Logic Layer
3. **DAL**: Data Access Layer (with Singleton pattern)
4. **BE**: Business Entities (shared across all layers)

While the architecture is implemented, the existing documentation (`Carpeta_Documentacion.md`) requires structural refinement to match the institutional "Carpeta Modelo" standards (margins, headers, specific sections). Additionally, there is no automated way to ensure that the architectural boundaries are respected during further development.

## Goals / Non-Goals

**Goals:**
- **Standardized Documentation**: Reorganize `Carpeta_Documentacion.md` to strictly follow `carpeta_modelo.md` formatting and section hierarchy.
- **Diagram Consistency**: Ensure all Mermaid diagrams (Component, ERD, Navigation) are accurate and embedded in the documentation.
- **Architectural Guardrails**: Introduce unit tests that verify architectural rules (e.g., DAL cannot reference UI, BLL must only use BE and DAL).
- **OOP Formalization**: Explicitly document how Cohesion, Coupling, Reusability, Extensibility, and Encapsulation are achieved in the current code.

**Non-Goals:**
- Refactoring existing business logic or data access code.
- Implementing new functional features (T01 is purely foundational/architectural).
- Changing the database schema.

## Decisions

### 1. Documentation Structure Alignment
**Decision**: Reconstruct the header and footer of `Carpeta_Documentacion.md` using Markdown tables to mimic the complex layout required by `carpeta_modelo.md`.
**Rationale**: Institutional standards require specific metadata (University name, Faculty, Student name, etc.) in every section. Tables provide the closest visual approximation in Markdown.
**Alternatives**: Using pure text (too messy) or PDF-only documentation (harder to maintain via Git).

### 2. Architectural Unit Testing
**Decision**: Use reflection-based unit tests to check assembly references and class namespaces.
**Rationale**: Ensures that developer errors (like adding a reference from DAL to UI) are caught at build time, maintaining the integrity of the 4-layer model.
**Alternatives**: Manual review (error-prone) or specialized architectural analysis tools (may introduce too many dependencies).

### 3. Diagram Management
**Decision**: Maintain diagrams in `.mmd` files in the `Diagramas/` folder and embed them in the Markdown documentation.
**Rationale**: Promotes "Documentation as Code" and ensures diagrams can be updated independently of the text.

## Risks / Trade-offs

- **[Risk] Formatting Limitations** → [Mitigation] Markdown cannot perfectly replicate physical margins (3cm left, etc.). The design will focus on structural elements and content hierarchy over precise physical typography.
- **[Risk] Reflection Test Fragility** → [Mitigation] Tests will target namespaces and assembly names, which are stable throughout the project lifecycle.
