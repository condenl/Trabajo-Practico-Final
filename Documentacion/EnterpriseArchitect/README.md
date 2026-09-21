# Diagramas de negocio en Enterprise Architect

Proyecto editable: `../../diagramas.EAP`.

Alcance: N01 y N02 de las iteraciones 1 y 2 del **Plan de Entregas TD 2026**. N03 pertenece a la iteración 3 y no se presenta como implementado. Los diagramas de procesos y secuencia representan actores y actividades organizacionales, sin un actor «Sistema».

| Requisito | N01 | N02 |
|---|---|---|
| Casos de uso principales | N01-01 | N02-01 |
| Actividad / proceso | N01-02 | N02-02 |
| Modelo conceptual | N01-03 | N02-03 |
| Clases del proceso, entidades BE | N01-04a, N01-04b | N02-04 |
| Clases de lógica de negocio BLL | N01-05 | N02-05 |
| Secuencias organizacionales | N01-06a, N01-06b | N02-06a, N02-06b, N02-06c |
| Modelo de datos del proceso | N01-07a, N01-07b | N02-07 |

Las secuencias de N02 complementan el modelado solicitado. Las especificaciones de los cinco casos de uso (objetivo, precondiciones, poscondiciones, disparador, flujo normal y excepciones) se incorporan en las notas de sus elementos UML; su fuente es `../casos de uso/Casos_de_Uso_N01_N02.md`.

Las propiedades de clases provienen de `BE/*.cs`; las operaciones públicas y parámetros, de `BLL/*.cs`. El modelo físico proviene de `SQL/Iteracion2_Aprovisionamiento.sql` y `SQL/Iteracion2_Ventas_Fidelizacion.sql`. `Usuarios` se muestra solo como referencia a la clave del módulo transversal. Las restricciones completas de cada tabla se conservan en sus notas. El modelo conceptual distingue el historial de membresías de la regla de una única membresía activa.

Las capturas PNG son exportaciones nativas del motor de diagramación de EA; no son imágenes Mermaid importadas. `manifest.json` relaciona cada imagen con el identificador y GUID de su diagrama.

Las capturas se incorporaron al [documento de Google Docs](https://docs.google.com/document/d/1kzEZpgX8bMnYrq-REzqYnlg_LAO2toCLVCEgpALjPnM/edit). La reconciliación del 20/09/2026 conserva las 12 tablas existentes, incluidas las cinco especificaciones de casos de uso de negocio, sin cambios de texto.

Se reemplazaron además los seis diagramas antiguos en sus ubicaciones originales: sección **2.3**, `N01-01`, `N02-01` y `N03-01`; sección **2.4**, `N01-02`, `N02-02` y `N03-02`. Las dos vistas de N03 son diseño organizacional de la iteración 3, no una ampliación de la implementación de las entregas 1 y 2. Se conservan editables en el paquete `N03 - Diseno organizacional - Iteracion 3`; `manifest-n03.json` identifica sus diagramas.

La revisión posterior completó las seis secuencias de negocio y las vistas globales, manteniendo el detalle por proceso:

- **2.5:** N01-06a/b, N02-06a/b/c y N03-06; sustituyen los flujos escritos y las versiones anteriores de las cuatro secuencias.
- **2.6:** las cinco tablas de casos de uso, con flujo normal y excepciones, se ubican antes de G07.
- **G07:** G07-01 ofrece la vista global de relaciones; los modelos conceptuales, BE y BLL de N01/N02 conservan el detalle. N03-04 desarrolla el diseño de evaluación de proveedores.
- **G08 / 8.1:** G08-01 ofrece el DER global y se conservan las vistas físicas de N01/N02; N03-07 representa la extensión de diseño.

`manifest-reconciliation.json` identifica las seis vistas nuevas. N03 y la coordinación de campañas se distinguen expresamente de la implementación entregada. Los fragmentos UML `alt`, `opt` y `break` explicitan las alternativas en las secuencias. Los antecedentes de compras y entregas fundamentan la evaluación de N03; sus tres criterios ponderados siguen siendo precio, prestigio y antigüedad.

La copia guardada contiene **26 diagramas EA únicos**, ninguno repetido, y **47 imágenes totales**. Se eliminaron del final las cuatro copias de casos de uso/actividad que ya estaban en 2.3/2.4; se sustituyeron las dos vistas globales antiguas. La comparación de píxeles RGBA confirma correspondencia exacta con los 26 PNG nativos (el pegado HTML puede cambiar la compresión PNG, por lo que no se compara solamente el hash del archivo). Se preservaron los 344 párrafos anteriores a 2.5 y el texto íntegro de las 12 tablas. Altura máxima de las capturas nuevas: 18 cm, con proporciones originales y sin recorte. No se agregaron capítulos ajenos al plan de entregas.

## Reproducción

Con el proyecto `diagramas.EAP` abierto en EA:

1. `python Scripts/build_ea_model.py` genera la especificación a partir del código y las reglas.
2. Ejecutar `Scripts/import_ea_model.ps1` mediante Windows PowerShell **de 32 bits**. Crea una copia de seguridad local y un paquete nuevo; se niega a sobrescribir un paquete existente.
3. Ejecutar `Scripts/refine_ea_layout.ps1` con el mismo PowerShell para aplicar los ajustes de distribución y las firmas de operaciones.

Para exportar un modelo ya existente sin modificar elementos, ejecutar `import_ea_model.ps1 -ExportOnly`. Los scripts usan la API oficial de automatización de EA, no SQL directo sobre el archivo EAP.

Para reproducir las dos vistas de N03, ejecutar `python Scripts/build_ea_n03.py`, luego `Scripts/import_ea_model.ps1 -ModelFile model-n03.json -PackageName 'N03 - Diseno organizacional - Iteracion 3' -ManifestFile manifest-n03.json` y finalmente `Scripts/refine_ea_n03.ps1`, también con PowerShell de 32 bits. En un paquete existente, agregar `-ExportOnly` a la importación.

Para reproducir la reconciliación después del modelo base:

1. `python Scripts/build_ea_reconciliation.py` genera `sequences-reconciled.json` (sustituye las cuatro secuencias originales) y `model-reconciliation.json` (seis vistas nuevas).
2. `Scripts/update_ea_sequences.ps1` actualiza los cuatro paquetes de secuencias generados, conserva sus diagramas/GUID y crea un respaldo del EAP. No ejecutar sobre paquetes con modificaciones manuales que deban conservarse.
3. `Scripts/import_ea_model.ps1 -ModelFile model-reconciliation.json -PackageName 'Vistas globales y diseno complementario de negocio' -ManifestFile manifest-reconciliation.json` crea el paquete complementario; si ya existe, usar `-ExportOnly`.
4. `python Scripts/route_ea_globals.py` y `Scripts/refine_ea_reconciliation.ps1` distribuyen los conectores globales sin atravesar las cajas.
5. `Scripts/refine_ea_sequence_frames.ps1` incorpora los fragmentos de interacción nativos y exporta las seis secuencias finales. Es idempotente sobre los fragmentos generados de esos paquetes.

Todos los scripts `.ps1` de EA requieren Windows PowerShell de 32 bits y el EAP correcto abierto. Los JSON de reconciliación son una capa posterior al modelo base; volver a generar únicamente `model.json` no aplica estas ampliaciones.

## Verificación del documento guardado

`python Scripts/verify_ea_document.py anterior.docx final.docx` compara las exportaciones de Google Docs: exige conservar el texto anterior a 2.5 y todas las tablas, verifica los píxeles de cada PNG, la ausencia de duplicados/recortes y la ubicación de cada diagrama en su sección. Requiere `python-docx` y Pillow.

La exportación final revisada tiene 56 páginas. Se ajustó la paginación para mantener los títulos junto a sus diagramas y comenzar cada especificación de caso de uso en una página propia. Las vistas globales se redujeron proporcionalmente para compartir página con su título y explicación; las vistas por proceso conservan el detalle.
