## 1. Cleanup and Initial Audit

- [x] 1.1 Perform a global search for "SessionManager" in `Documentacion/` and remove all occurrences.
- [x] 1.2 Review `Documentacion/Carpeta_Documentacion.md` to identify all Mermaid blocks needing synchronization.
- [x] 1.3 Compare existing class signatures in `BE/`, `BLL/`, and `DAL/` with current documentation to list mismatches.

## 2. Update Business Entities (BE) Diagrams

- [x] 2.1 Update the `Usuario` class diagram to include the `DVH` property.
- [x] 2.2 Add the `DVV` class to the BE diagram and show its relationship with integrity services.
- [x] 2.3 Create a subgraph for the **Composite Pattern** including `Componente`, `Familia`, and `Patente`.
- [x] 2.4 Add the `IObserver` and `IIdiomaObserver` interfaces to represent the **Observer Pattern**.
- [x] 2.5 Add the simulated `UsuarioAuditoria` class with the same structure as `Usuario`.

## 3. Update Business Logic (BLL) & Services Diagrams

- [x] 3.1 Add `SeguridadBLL` to the BLL layer diagram with its core methods (CargarEstructuraPermisos, etc.).
- [x] 3.2 Update `GestorIdioma` as a Singleton subject in the Observer pattern diagram.
- [x] 3.3 Ensure `BitacoraBLL` is correctly shown as an `IObserver` implementation.
- [x] 3.4 Replace any legacy session logic with `Session.Instance` references.

## 4. Update Data Access (DAL) Diagrams

- [x] 4.1 Add `DVVDAL` to the DAL layer diagram.
- [x] 4.2 Verify and update all Singleton relationships (using static Instancia property).
- [x] 4.3 Ensure method signatures in DAL match the current implementation (e.g., `sp_DVV_Consultar` usage).

## 5. Verification & Balancing

- [x] 5.1 Run a final check to ensure all Mermaid diagrams in `Carpeta_Documentacion.md` are technically accurate.
- [x] 5.2 Validate that the documentation reflects the decoupled security and localization modules as per the new specs.
- [x] 5.3 Confirm that "SessionManager" is completely gone from the repository.
