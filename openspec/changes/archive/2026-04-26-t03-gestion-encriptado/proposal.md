## Why

El sistema requiere proteger información sensible para garantizar la confidencialidad e integridad, cumpliendo con estándares de seguridad que exigen que datos críticos (como contraseñas) no sean almacenados en texto plano.

## What Changes

- Consolidación del mecanismo de **Hash (SHA256)** como el único método de protección de datos ("encriptado" unidireccional).
- Limpieza de métodos de encriptación reversible (AES) para mantener la simplicidad y seguridad del sistema.
- Actualización de la documentación global del sistema (`Carpeta_Documentacion.md`) incluyendo objetivos, descripción técnica del algoritmo de hashing y diagramas.

## Capabilities

### New Capabilities
- `encryption-services`: Provee servicios de hashing para protección de datos mediante SHA256.

### Modified Capabilities
- (ninguna)

## Impact

- **Servicios**: Refactorización de la clase `Encriptador` para exponer únicamente el método `Hash`.
- **Documentación**: Actualización de `Carpeta_Documentacion.md` con la sección T03 enfocada en hashing.
