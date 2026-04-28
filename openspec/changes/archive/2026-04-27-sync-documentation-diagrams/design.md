## Context

El proyecto requiere que la documentación técnica (`Carpeta_Documentacion.md`) sea un reflejo exacto de la implementación del código. Actualmente, el diagrama de componentes (T01.3) es demasiado detallado, lo que dificulta la comprensión de la arquitectura de alto nivel. Además, se han realizado cambios en el código (como el cambio de tipo de dato de DNI a `string` y la adición de métodos en `Encriptador.cs`) que deben verse reflejados en todos los diagramas técnicos para mantener la integridad de la documentación.

## Goals / Non-Goals

**Goals:**
- Simplificar el diagrama de componentes (T01.3) para que sea un diagrama de grafos puro.
- Sincronizar el diagrama de clases (G06) con la estructura actual de BE, BLL, DAL y MPP.
- Asegurar que TODOS los diagramas ER (G07.1, T01.4, T02.7, T03.4, T06a.6) utilicen los tipos de datos correctos (DNI como string).
- Validar y sincronizar diagramas de clases técnicos (T03.3, T06a.3).
- Validar que el Mapa de Navegación (T01.5) refleje las opciones reales de la UI.
- Validar que los diagramas de secuencia (T01.2, T02.4, T02.5, T03.5, T06a.4, T06a.5) utilicen nombres de métodos reales del código.

**Non-Goals:**
- No se modificará el código fuente de la aplicación.
- No se agregarán nuevos diagramas, solo se actualizarán los existentes.

## Decisions

### 1. Representación Simplificada de Componentes en T01.3
Se eliminará el contenido de las clases dentro de los subgrafos de Mermaid en la sección T01.3. Los componentes se representarán únicamente como cajas conectadas por flechas de relación.
- **Rationale**: Mejora la legibilidad arquitectónica y cumple con el requerimiento directo del usuario.

### 2. Extracción de Metadatos del Código para G06
Se realizará un escaneo de los archivos `.cs` en las carpetas `BE`, `BLL`, `DAL` y `MPP` para identificar propiedades y métodos públicos.
- **Rationale**: Garantiza que el diagrama de clases sea una representación 1:1 de la implementación actual.

### 3. Estandarización de Tipos en DER
Se unificará el tipo de dato del DNI como `string` en todos los diagramas ERD, alineándose con la migración a AES para encriptado reversible.

## Risks / Trade-offs

- **[Riesgo] Desactualización futura** → La documentación es manual y puede volver a quedar desfasada.
  - **Mitigación**: Este cambio establece la línea base de sincronización actual.
- **[Riesgo] Omisión de relaciones** → Al simplificar diagramas, se pueden perder dependencias importantes.
  - **Mitigación**: Se mantendrán todas las flechas de relación (invoca, utiliza, implementa) aunque se elimine el contenido de los nodos.
