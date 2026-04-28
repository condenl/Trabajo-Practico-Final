## Context

El proyecto utiliza Mermaid para sus diagramas técnicos. Actualmente, algunos de estos diagramas se encuentran en archivos `.mmd` independientes dentro de la carpeta `Diagramas/`, mientras que el documento principal `Documentacion/Carpeta_Documentacion.md` contiene versiones parciales o referencias a estos archivos. Para mejorar la mantenibilidad, se ha decidido embeber el código fuente de los diagramas directamente en el archivo Markdown de documentación.

## Goals / Non-Goals

**Goals:**
- Integrar `01_componentes_arquitectura.mmd` en la sección `T01.3 Diagrama de Componentes`.
- Integrar `02_esquema_persistencia_DER.mmd` en la sección `G07.1 Diagrama Entidad-Relación`.
- Integrar `03_mapa_navegacion.mmd` en la sección `T01.5 Mapa de Navegación`.
- Eliminar la carpeta `Diagramas/` tras la migración exitosa.

**Non-Goals:**
- No se realizarán cambios funcionales en los diagramas (solo migración de ubicación).
- No se modificarán otros archivos de documentación fuera de `Carpeta_Documentacion.md`.

## Decisions

### 1. Bloques Mermaid Nativos
Se utilizarán bloques de código con el identificador `mermaid` en el archivo Markdown.
- **Rationale**: Es el estándar de facto para documentación en GitHub y otros visores modernos de Markdown, eliminando la necesidad de gestionar archivos de imagen o archivos externos `.mmd`.

### 2. Eliminación de Archivos Fuente Externos
Los archivos `.mmd` serán eliminados tras confirmar su correcta integración.
- **Rationale**: Mantener los archivos externos después de embeberlos crearía una duplicidad de la información que llevaría inevitablemente a inconsistencias en el futuro.

## Risks / Trade-offs

- **[Riesgo] Tamaño del archivo Markdown** → El archivo `Carpeta_Documentacion.md` se volverá más extenso.
  - **Mitigación**: La extensión es manejable y el beneficio de centralización supera el costo del tamaño del archivo.
- **[Riesgo] Errores de sintaxis durante el copiado** → Un error al copiar el código Mermaid podría romper la visualización.
  - **Mitigación**: Se realizará una validación visual del contenido embebido respecto a los archivos fuente antes de borrar estos últimos.
