## 1. Database and Entities

- [x] 1.1 Create SQL script for `Idioma` and `Traduccion` tables (Denormalized schema).
- [x] 1.2 Implement BE entities: `Idioma.cs`, `Traduccion.cs`.
- [x] 1.3 Insert initial data for English and Spanish with core UI translations mapping keys directly to text.
- [x] 1.4 Prepare comprehensive SQL script with all identified UI translation keys (`SQL\MultiIdioma_Full_Data.sql`).

## 2. Data Access and Business Logic

- [x] 2.1 Create `IdiomaDAL` and `TraduccionDAL`.
- [x] 2.2 Create `IdiomaMPP` and `TraduccionMPP` for object mapping.
- [x] 2.3 Create `IdiomaBLL` and `TraduccionBLL`.

## 3. Services Layer

- [x] 3.1 Ensure `IObserver` interface is available in `BE.Interfaces` or `Servicios`.
- [x] 3.2 Implement `GestorIdioma` (Subject) in the `Servicios` layer as a Singleton.
- [x] 3.3 Implement `CargarIdioma(Idioma idioma)` to set current language.
- [x] 3.4 Implement `Traducir(string key)` to fetch translation directly from database/DAL.

## 4. UI Integration - Login

- [x] 4.1 Implement `IObserver` in `Login.cs`.
- [x] 4.2 Add `cmbIdioma` to `Login.cs` and populate it from `IdiomaBLL`.
- [x] 4.3 Implement `ActualizarTextos()` and register `Login` as an observer in `GestorIdioma`.
- [x] 4.4 Update `cmbIdioma_SelectedIndexChanged` to trigger language change in `GestorIdioma`.

## 5. UI Integration - Main Form and Controls

- [x] 5.1 Implement `IObserver` in `FormPrincipal.cs`.
- [x] 5.2 Add `cmbIdioma` to `FormPrincipal.cs` and sync with current session language.
- [x] 5.3 Implement `ActualizarTextos()` in `FormPrincipal.cs` to update menu items and titles.
- [x] 5.4 Update all UserControls (`UC_Usuarios`, `UC_Roles`, etc.) to implement `IObserver` and register/unregister accordingly.
- [x] 5.5 Remove hardcoded Spanish fallbacks in `UC_Usuarios.cs` and `UC_SoporteAdmin.cs`.

## 6. Verification

- [x] 6.1 Verify that changing language in `Login` updates all visible texts immediately.
- [x] 6.2 Verify that the selected language persists after login in `FormPrincipal`.
- [x] 6.3 Verify that adding a new language to the database makes it available in the UI without code changes.
- [x] 6.4 Verify fallback behavior when a translation is missing.
- [x] 6.5 Verify that no hardcoded strings are used as defaults in specialized UserControls.
