## Why

Actualmente, existen diagramas en formato `.mmd` dentro de la carpeta `Diagramas` que no están plenamente integrados o actualizados en el documento principal de arquitectura (`Documentacion/Carpeta_Documentacion.md`). Consolidar estos diagramas directamente en el archivo Markdown mejora la portabilidad de la documentación, facilita su visualización en plataformas compatibles con Mermaid (como GitHub o visores de Markdown) y elimina la redundancia de archivos externos, centralizando la "fuente de verdad" de la documentación técnica.

## What Changes

- **Integración de Diagramas**: Se embeberán los contenidos de los archivos `.mmd` en las secciones correspondientes de `Carpeta_Documentacion.md`.
- **Eliminación de Redundancia**: Una vez integrados, se eliminará la carpeta `Diagramas` y sus archivos contenidos.
- **Actualización de Referencias**: Se ajustarán las referencias internas en la documentación que apunten a los archivos externos ahora embebidos.

## Capabilities

### New Capabilities
- Ninguna.

### Modified Capabilities
- `documentacion-proyecto`: Se actualiza el proceso de gestión de diagramas para que residan exclusivamente dentro de los archivos de documentación Markdown.

## Impact

- **Documentación**: `Documentacion/Carpeta_Documentacion.md` será modificado extensivamente para incluir los bloques Mermaid.
- **Sistema de Archivos**: Eliminación de la carpeta `Diagramas/`.
