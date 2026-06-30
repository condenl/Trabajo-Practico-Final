## Why

The system currently lacks a centralized, dynamic way to manage multiple languages. To support internationalization without hardcoding strings or relying on static resource files, a database-driven localization system is required. This will allow for real-time language switching and easy addition of new languages without code changes.

## What Changes

- **Database-driven Localization**: Implement tables to store languages and their respective translations using a denormalized schema.
- **Observer Pattern Integration**: Use the Observer pattern to notify all active UI components when the system language changes.
- **Optional UI Language Selectors**: Add ComboBoxes to the Login form and Main Form to allow users to switch languages dynamically if they choose to.
- **Decoupled Architecture**: Ensure the translation logic is independent of the UI layer, residing in the BLL/DAL/BE layers.
- **Dynamic Content Loading**: UI components will refresh their titles, labels, and messages upon language change.

## Capabilities

### New Capabilities
- `multi-language`: Manages the lifecycle of language data, including retrieval from the database and notifying observers of language updates.

### Modified Capabilities
- `user-authentication`: The login process will now include an optional language selection that persists during the session.

## Impact

- **Database**: New tables for `Idioma` and `Traduccion`.
- **BE**: New entities for language and translation management.
- **DAL/MPP/BLL**: New classes to handle data access and business logic for multi-language.
- **UI**: All forms and user controls that display text must implement the `IObserver` interface (or a specialized version) to react to language changes.
- **Services**: A `GestorIdioma` service to manage the current language and observers.
