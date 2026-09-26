# Diagramas de negocio en Enterprise Architect

Proyecto editable: `../../diagramas.EAP`.

Alcance: N01 y N02 de las iteraciones 1 y 2 del **Plan de Entregas TD 2026**. N03 pertenece a la iteración 3 y no se presenta como implementado. Los casos de uso y actividades representan actores y actividades organizacionales, sin un actor «Sistema». Desde la revisión del 26/09/2026, las secuencias muestran su realización técnica con clases y nombres de métodos, por pedido del autor.

| Requisito | N01 | N02 |
|---|---|---|
| Casos de uso principales | N01-01 | N02-01 |
| Actividad / proceso | N01-02 | N02-02 |
| Modelo conceptual | N01-03 | N02-03 |
| Clases del proceso, entidades BE | N01-04a, N01-04b | N02-04 |
| Clases de lógica de negocio BLL | N01-05 | N02-05 |
| Secuencias de realización en clases | N01-06a, N01-06b | N02-06a, N02-06b, N02-06c |
| Modelo de datos del proceso | N01-07a, N01-07b | N02-07 |

Las secuencias de N02 complementan el modelado solicitado. Las especificaciones de los cinco casos de uso (objetivo, precondiciones, poscondiciones, disparador, flujo normal y excepciones) se incorporan en las notas de sus elementos UML; su fuente es `../casos de uso/Casos_de_Uso_N01_N02.md`.

Las propiedades de clases provienen de `BE/*.cs`; las operaciones públicas y parámetros, de `BLL/*.cs`. El modelo físico proviene de `SQL/Iteracion2_Aprovisionamiento.sql` y `SQL/Iteracion2_Ventas_Fidelizacion.sql`. `Usuarios` se muestra solo como referencia a la clave del módulo transversal. Las restricciones completas de cada tabla se conservan en sus notas. El modelo conceptual distingue el historial de membresías de la regla de una única membresía activa.

Las capturas PNG son exportaciones nativas del motor de diagramación de EA; no son imágenes Mermaid importadas. `manifest.json` relaciona cada imagen con el identificador y GUID de su diagrama.

Las capturas se incorporaron al [documento de Google Docs](https://docs.google.com/document/d/1kzEZpgX8bMnYrq-REzqYnlg_LAO2toCLVCEgpALjPnM/edit). La reconciliación del 20/09/2026 conserva las 12 tablas existentes, incluidas las cinco especificaciones de casos de uso de negocio, sin cambios de texto.

Se reemplazaron además los seis diagramas antiguos en sus ubicaciones originales: sección **2.3**, `N01-01`, `N02-01` y `N03-01`; sección **2.4**, `N01-02`, `N02-02` y `N03-02`. Las dos vistas de N03 son diseño organizacional de la iteración 3, no una ampliación de la implementación de las entregas 1 y 2. Se conservan editables en el paquete `N03 - Diseno organizacional - Iteracion 3`; `manifest-n03.json` identifica sus diagramas.

La revisión posterior completó las seis secuencias de negocio y las vistas globales, manteniendo el detalle por proceso:

- **2.5:** N01-06a/b, N02-06a/b/c y N03-06; sustituyen los flujos escritos y las versiones anteriores de las cuatro secuencias.
- **2.6:** las cinco tablas de casos de uso, con flujo normal y excepciones, se ubican antes de G07.
- **G07:** G07-01 ofrece la vista global de relaciones con los mismos atributos y operaciones principales que las vistas por proceso. Los modelos conceptuales siguen separados del diseño de clases BE/BLL. N03-04 desarrolla el diseño de evaluación de proveedores.
- **G08 / 8.1:** G08-01 ofrece el DER global y se conservan las vistas físicas de N01/N02; N03-07 representa la extensión de diseño.

`manifest-reconciliation.json` identifica las seis vistas nuevas. N03 y la coordinación de campañas se distinguen expresamente de la implementación entregada. Las secuencias técnicas usan fragmentos UML `alt`, `opt` y `loop`. Los antecedentes de compras y entregas fundamentan la evaluación de N03; sus tres criterios ponderados siguen siendo precio, prestigio y antigüedad.

La copia guardada contiene **26 diagramas EA únicos**, ninguno repetido, y **47 imágenes totales**. Se eliminaron del final las cuatro copias de casos de uso/actividad que ya estaban en 2.3/2.4; se sustituyeron las dos vistas globales antiguas. Se preservaron los 344 párrafos anteriores a 2.5 y el texto íntegro de las 12 tablas. No se agregaron capítulos ajenos al plan de entregas. En la revisión del 26/09, Google Docs redujo siete capturas grandes a 2048 píxeles de ancho: se verifican por proporción y comparación de píxeles remuestreados; las otras 19 coinciden exactamente. Los PNG de esta carpeta y el EAP conservan la resolución nativa. Las vistas por proceso permiten leer el detalle que la vista global presenta como panorama.

## Correspondencia con el código (26/09/2026)

Se actualizaron seis secuencias y siete vistas de clases. `class-catalog.json` reúne 40 definiciones canónicas obtenidas del código o marcadas explícitamente como propuestas. `code-design.json` genera las 13 vistas y `code-design-routed.json` agrega rutas de conectores. Los atributos y operaciones de cada clase se reutilizan sin diferencias entre las vistas global y por proceso.

Las líneas de vida son objetos UML tipados por clases (`ClassifierID`); sus mensajes usan nombres exactos de métodos del receptor. Las firmas, tipos, visibilidad, parámetros, constructores y operaciones estáticas se almacenan como miembros UML editables. No se agregaron reglas ficticias a las entidades BE: las operaciones principales se muestran en sus BLL colaboradoras.

`EvaluacionProveedor`, `EvaluacionProveedorBLL` y `MarketingBLL` son propuestas de diseño. Sus operaciones no se presentan como código existente. La secuencia de membresía aclara que el código actual guarda al cliente antes de validar el descuento; esta revisión no modifica la implementación. Las actualizaciones preservan los elementos anteriores en subpaquetes `Version anterior` y respaldan el EAP localmente.

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

Para aplicar la capa final de correspondencia con el código, después de la reconciliación:

1. `python Scripts/build_ea_code_design.py` extrae miembros y verifica que los receptores de las secuencias declaren sus métodos.
2. `python Scripts/route_ea_globals.py --input code-design.json --output code-design-routed.json --all-logical` distribuye los conectores de todas las vistas de clases.
3. `Scripts/update_ea_code_design.ps1` actualiza las 13 vistas, preserva versiones anteriores y exporta los PNG nativos. Cada ejecución agrega un archivo de respaldo y subpaquetes históricos; no es una operación de limpieza.
4. `Scripts/verify_ea_code_design.ps1` verifica, sin escribir, los atributos, firmas, clasificadores y mensajes del modelo EA activo.

`Scripts/refine_ea_code_frames.ps1` permite reajustar únicamente los límites de fragmentos de secuencia y reexportar sus capturas, sin recrear elementos.

## Verificación del documento guardado

`python Scripts/verify_ea_document.py anterior.docx final.docx` compara las exportaciones de Google Docs: exige conservar el texto anterior a 2.5 y todas las tablas, verifica cada PNG (exacto o reducido por Google Docs a 2048 píxeles, con error medio menor a 3/255), la ausencia de duplicados/recortes y la ubicación de cada diagrama en su sección. Distingue explícitamente las imágenes reducidas en el informe. Requiere `python-docx` y Pillow. `inspect_ea_doc_export.py` muestra tamaños y opciones de paginación para diagnóstico.

Se revisa además la exportación PDF para comprobar títulos junto a sus diagramas, notas completas y comienzos de casos de uso en página propia. La captura global de clases ocupa 16 cm de ancho, sin recorte; para leer todos sus miembros se recomienda el PNG nativo o el proyecto EA.

Verificación del 26/09/2026: el modelo activo pasó el control de **105 representaciones de clases, 64 mensajes y 13 diagramas**. La exportación final del documento tiene **57 páginas**; se comprobó visualmente la sección revisada y se compactó únicamente el interlineado de las excepciones de recepción para evitar una palabra aislada. `verification-code-design.json` registra la correspondencia de imágenes, sus secciones, las siete reducciones de resolución y la preservación del texto y tablas. El enlace sobre «La captura global» en Google Docs abre el PNG nativo del repositorio.
