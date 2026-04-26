## 1. Documentation Alignment

- [x] 1.1 Create the institutional header table in `Documentacion/Carpeta_Documentacion.md` using the format from `carpeta_modelo.md`.
- [x] 1.2 Update the "Historial de Revisión" to include version 1.0 (Arquitectura Base).
- [x] 1.3 Regenerate the "Índice" (Table of Contents) with correct links to all G and T sections.
- [x] 1.4 Ensure all Mermaid diagrams from `Diagramas/` are correctly embedded and rendered in the T01 section.
- [x] 1.5 Review and update the "G06. Diagrama de clases parcial" to ensure it reflects all current BLL and DAL classes.

## 2. Architectural Verification (Tests)

- [x] 2.1 Create a new project `Servicios.Tests` or `Arquitectura.Tests` if it doesn't exist.
- [x] 2.2 Implement a unit test using reflection to verify that `DAL` assembly does not reference `UI` or `BLL` assemblies.
- [x] 2.3 Implement a unit test using reflection to verify that `BLL` assembly does not reference `UI` assembly.
- [x] 2.4 Implement a unit test to verify that all classes in `DAL` with names ending in `DAL` have a private constructor and a static `ObtenerInstancia` method (Singleton pattern).
- [x] 2.5 Implement a unit test to verify that `UsuarioBLL` implements `IObservado` and `BitacoraBLL` implements `IObserver` (Observer pattern).

## 3. OOP Principles Documentation

- [x] 3.1 Add a detailed subsection in T01 of `Carpeta_Documentacion.md` explaining how **Cohesion** is achieved (e.g., Single Responsibility in BLL).
- [x] 3.2 Add a subsection explaining **Low Coupling** through the use of interfaces and the Observer pattern.
- [x] 3.3 Add a subsection explaining **Reusability** through the BE layer and shared Services.
- [x] 3.4 Add a subsection explaining **Extensibility** and **Encapsulamiento** with specific code examples from the project.

## 4. Final Validation

- [x] 4.1 Run all new architectural tests and ensure they pass.
- [x] 4.2 Perform a full read-through of `Carpeta_Documentacion.md` to ensure it meets all institutional guidelines.
