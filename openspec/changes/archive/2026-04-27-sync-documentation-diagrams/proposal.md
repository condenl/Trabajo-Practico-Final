## Why

Mantener la sincronización entre la documentación técnica y la implementación real del código es fundamental para la mantenibilidad del proyecto y el cumplimiento de los estándares de calidad. Actualmente, existen discrepancias menores o niveles de detalle inconsistentes en los diagramas de la `Carpeta_Documentacion.md`. Específicamente, el diagrama de componentes requiere una simplificación para centrarse en la arquitectura de alto nivel (grafos) sin el ruido visual de los detalles de implementación de clases internas.

## What Changes

- **Simplificación del Diagrama de Componentes (T01.3)**: Se modificará el diagrama Mermaid para que solo muestre los nodos de componentes y sus relaciones direccionales, eliminando la definición de miembros o clases internas dentro de los subgrafos.
- **Sincronización del Diagrama de Clases (G06)**: Se revisará la estructura de clases en el código (BE, BLL, DAL) y se actualizará el diagrama para reflejar fielmente las propiedades, métodos y relaciones actuales.
- **Sincronización del Modelo de Datos (G07.1 / T01.4)**: Se verificarán las tablas y campos en el código/DER y se asegurará que ambos diagramas ER sean consistentes entre sí y con la persistencia.
- **Validación de Consistencia**: Revisión general de otros diagramas menores (Navegación T01.5, Secuencias) para asegurar que los nombres de métodos y clases coincidan con el código fuente.

## Capabilities

### Modified Capabilities
- `documentacion-proyecto`: Actualización de los requerimientos de representación visual para los diagramas arquitectónicos y técnicos.

## Impact

- **Documentación**: Cambios extensivos en `Documentacion/Carpeta_Documentacion.md`.
- **Código**: Ningún impacto (No se modifica código).
