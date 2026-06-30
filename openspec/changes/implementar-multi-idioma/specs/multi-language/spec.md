## ADDED Requirements

### Requirement: Language Storage and Management
The system SHALL store multiple languages and their associated translations in a relational database using a denormalized schema (Mapping language and key directly to text).

#### Scenario: New Language Availability
- **WHEN** a new language is added to the `Idioma` table
- **THEN** it MUST automatically appear as an option in all language selection ComboBoxes

### Requirement: Dynamic Translation Retrieval
The system SHALL provide a centralized mechanism to retrieve the translation of a specific key (string) for the currently selected language.

#### Scenario: Successful Translation Retrieval
- **WHEN** requested for a translation with a valid key and active language
- **THEN** the system SHALL return the corresponding text from the database

### Requirement: Observer Notification for Language Change
The system SHALL implement the Observer pattern to notify all active UI components whenever the language is changed.

#### Scenario: Dynamic UI Update
- **WHEN** the user selects a different language from a ComboBox
- **THEN** all registered UI components MUST be notified and update their displayed texts immediately

### Requirement: Missing Translation Fallback
The system SHALL handle cases where a translation is missing for a specific key in the selected language.

#### Scenario: Fallback to Key Name
- **WHEN** a translation is requested but not found for the selected language
- **THEN** the system SHALL return the name of the key (string) formatted to indicate it is missing (e.g., `[key_name]`)

### Requirement: Zero Hardcoded UI Strings
The system SHALL NOT contain hardcoded UI strings in the source code of Forms or UserControls for labels, buttons, or messages that require localization. All such content MUST be retrieved from the database using translation keys.

#### Scenario: Strictly Dynamic UI
- **WHEN** inspecting the source code of `UC_Usuarios` or `UC_SoporteAdmin`
- **THEN** no hardcoded Spanish or English strings SHOULD be present as fallbacks in the C# code for UI element content.
