## Context

The application needs to support multiple languages dynamically. The current architecture follows a layered approach (UI, BLL, DAL, MPP, BE, Servicios). The multi-language system must integrate into these layers, using a database for persistence and the Observer pattern for dynamic UI updates.

## Goals / Non-Goals

**Goals:**
- Centralized language management in the `Servicios` layer.
- Database persistence for languages and translations.
- Dynamic UI updates without restarting the application.
- Decoupling of translation logic from specific UI controls.
- Support for adding new languages via database without code changes.

**Non-Goals:**
- Translation of dynamic user-generated content (e.g., audit log entries).
- Use of .NET Resource (.resx) files for UI strings.

## Decisions

### 1. Observer Pattern Implementation
- **Subject**: A singleton `GestorIdioma` in the `Servicios` layer will maintain the current language state and a list of observers.
- **Observer**: UI components (Forms, UserControls) will implement the `IObserver` interface.
- **Rationale**: This ensures all active UI elements are notified simultaneously when the language changes, enabling a seamless transition.

### 2. Denormalized Data Model
- **Idioma**: `ID`, `Nombre`, `IsDefault`.
- **Traduccion**: `ID`, `IdIdioma`, `NombreEtiqueta` (Key), `Texto`.
- **Rationale**: Removing the separate `Etiqueta` entity simplifies the schema and the DAL/MPP logic. The key is stored directly as a string in the `Traduccion` table.

### 3. UI Interaction
- ComboBoxes in `Login` and `FormPrincipal` will trigger `GestorIdioma.CambiarIdioma(idioma)`.
- These ComboBoxes are **optional**; users may choose to interact with them to change the interface language at any time.
- Each `IObserver` will implement `Update(idioma)` by calling a local `ActualizarTextos()` method that fetches strings from `GestorIdioma`.

## Risks / Trade-offs

- **[Risk] Database Load** → **[Mitigation]** Since translations are requested on-demand from the database, ensure efficient queries in the DAL/MPP layers. Indexing the `NombreEtiqueta` and `IdIdioma` columns is recommended.
- **[Risk] Missing Translations** → **[Mitigation]** If a translation is not found for the selected language, the system will attempt to return the key name or a default "missing" string.
- **[Risk] Implementation Overhead** → **[Mitigation]** Use the `Tag` property of WinForms controls to store the translation key, simplifying the `ActualizarTextos()` logic.
