# Diagramas de Enterprise Architect

Estado al **01/11/2026**. Proyecto editable: `../../diagramas.EAP`. Capturas nativas incorporadas al [documento de Google Docs](https://docs.google.com/document/d/1kzEZpgX8bMnYrq-REzqYnlg_LAO2toCLVCEgpALjPnM/edit).

## Sincronización con N03 implementado

La secuencia N03-06 refleja `UC_EvaluacionProveedores`, `EvaluacionProveedorBLL`, `EvaluacionProveedorDAL`, `ArchivoEvaluacion` y la inserción transaccional de `AprovisionamientoDAL`. El actor Dueño / Departamento de Compras inicia comparar, confirmar y crear el borrador. Sus **52 mensajes** incluyen llamadas con nombres del código y respuestas discontinuas con el tipo real, dirigidas al emisor de la llamada. Se conserva el orden de activaciones y retornos. Las notas del diagrama explican las consultas auxiliares omitidas, la validación de vigencia, las excepciones y el reintento que devuelve la orden existente.

Las entidades se detallan en **N03-04**; BLL, interfaz de repositorio, DAL y serialización, en **N03-05**. **G07-01** comparte exactamente sus atributos y operaciones, además del resto de los procesos. `class-catalog.json` contiene 49 definiciones canónicas; las 14 vistas técnicas reutilizan esas definiciones. N01-05 incorpora los métodos de inserción por lote y la sobrecarga transaccional usada por N03. Las BE conservan propiedades y constructores reales; no se les inventan operaciones de negocio. MarketingBLL sigue identificado como diseño propuesto; N03 ya representa código implementado.

**N03-07 y G08-01** derivan las diez columnas y tres referencias de `SQL/Iteracion3_Evaluacion_Proveedores.sql`. Criterios, ofertas y resultados se conservan en `SnapshotXml`; no existen columnas independientes de pesos ni `EsPrioritario`. Proveedor elegido y orden son opcionales. El índice único filtrado de IdOrdenCompra se refleja con multiplicidad 0..1. Las relaciones son asociaciones «FK», sin flechas de herencia. El DER global se redistribuyó para evitar superponer EvaluacionProveedor y MovimientoStock.

Las dependencias BLL apuntan a los contratos de `DAL.Interfaces`; las implementaciones DAL usan realizaciones nativas, con línea discontinua y triángulo hueco hacia la interfaz. Persona y Componente son abstractas mediante la propiedad UML `Abstract`, con nombres en cursiva. Se mantienen las generalizaciones de subclase a superclase y las asociaciones con sus multiplicidades. EA 12.1 requiere convertir las realizaciones mediante su importador XMI nativo; los scripts conservan GUID y actualizan los IDs locales de los manifiestos.

## Organización y verificación de Google Docs

- **2.3:** casos de uso de negocio; **2.4:** sus especificaciones, conservadas íntegramente.
- **2.5:** actividades. Se sincronizó la captura N02-02 que todavía ubicaba Membresía en el carril anterior; el objeto y sus actividades productora/consumidora están en Personal de Ventas.
- **2.6:** secuencias técnicas con actores, métodos y retornos tipados.
- **G07:** clases globales y vistas por proceso, incluido el nuevo detalle N03-05 dentro de la sección existente.
- **G08 / 8.1:** DER global y modelos físicos por proceso.

La exportación final conserva **21 imágenes anteriores** sin cambios y **12 tablas**, los **351 párrafos anteriores a las secuencias** y todo el texto restante salvo cuatro cambios autorizados: introducción, nota de N03, rótulo de entidades y nuevo rótulo de lógica/persistencia. Hay **48 imágenes y 27 diagramas EA únicos**, sin duplicados ni recortes. El PDF de Google Docs tiene 59 páginas. Se corrigió el salto previo a N03-05 para mantener título y captura juntos.

`verification-n03-document.json` registra ubicación, preservación y coincidencias de imágenes. Google Docs limita cualquiera de las dimensiones a 2048 píxeles; el verificador compara proporción y píxeles remuestreados. La vista global G07-01 requiere además la revisión visual vinculada a los dos SHA256 exactos en `visual-review-n03.json`. El detalle legible se mantiene en las vistas por proceso y los PNG nativos de esta carpeta. No se presenta la reducción de Google como coincidencia exacta de píxeles.

`verification-n03-model.json` registra la comprobación nativa de las 14 vistas: **130 representaciones de clases y 188 mensajes**. También se verificaron las columnas, tipos, relaciones, multiplicidades y ausencia de cajas superpuestas de ambos DER actualizados. No se modificaron las reglas de la aplicación ni la base de datos en esta revisión.

## Reproducir esta revisión

Requisitos: Python con las dependencias de los scripts, Enterprise Architect abierto en `diagramas.EAP` y **Windows PowerShell de 32 bits** para la API COM. Todos los actualizadores comprueban la ruta del proyecto, crean un respaldo local y conservan los elementos anteriores editables en subpaquetes `Version anterior`.

Sobre el proyecto existente:

1. `python Scripts/build_ea_code_design.py`
2. `python Scripts/route_ea_globals.py --input code-design.json --output code-design-routed.json --all-logical`
3. `python Scripts/sync_ea_n03_technical.py`
4. `python Scripts/route_ea_globals.py --input n03-data-design.json --output n03-data-routed.json`
5. `Scripts/prepare_ea_n03_technical.ps1` registra N03-05 si falta y actualiza los clasificadores internos compartidos que no aparecen dibujados.
6. `Scripts/update_ea_code_design.ps1 -Codes 'N01-05,N03-04,N03-05,G07-01,N03-06'`
7. `Scripts/update_ea_code_design.ps1 -ModelFile n03-data-design.json -Codes N03-07`
8. `Scripts/update_ea_code_design.ps1 -ModelFile n03-data-routed.json -Codes G08-01`
9. `Scripts/verify_ea_code_design.ps1` y `Scripts/verify_ea_n03_data.ps1` verifican el modelo sin modificarlo.

Los comandos Python se ejecutan desde la raíz del repositorio; los .ps1 con el ejecutable de `SysWOW64/WindowsPowerShell/v1.0/powershell.exe`. Los manifiestos relacionan los diagramas con sus GUID y capturas. Los scripts históricos de importación inicial pueden reconstruir capas anteriores: después de usarlos hay que reaplicar esta capa final. No se escribe SQL directamente sobre el EAP.

Para verificar una nueva exportación DOCX del mismo documento frente a su copia previa:

```text
python Scripts/verify_ea_document.py ANTES.docx DESPUES.docx --paragraph-replacements Documentacion/EnterpriseArchitect/n03-document-replacements.json --reviewed-images Documentacion/EnterpriseArchitect/visual-review-n03.json --report Documentacion/EnterpriseArchitect/verification-n03-document.json
```

Los documentos de revisión y respaldos locales no se incorporan al repositorio; se conservan el EAP, sus fuentes JSON, scripts, capturas nativas y evidencia de las verificaciones.
