from __future__ import annotations

import sys
from pathlib import Path

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Inches


ROOT = Path(__file__).resolve().parents[1]
DIAGRAMS = ROOT / "Documentacion" / "Diagramas"


def insert_diagram_after(document: Document, anchor: str, image_name: str) -> None:
    paragraph = next((item for item in document.paragraphs if item.text.strip() == anchor), None)
    if paragraph is None:
        raise ValueError(f"No se encontro el parrafo de anclaje: {anchor}")

    image_paragraph = document.add_paragraph()
    image_paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    image_paragraph.paragraph_format.page_break_before = True
    image_paragraph.add_run().add_picture(str(DIAGRAMS / image_name), width=Inches(6.15))
    paragraph._p.addnext(image_paragraph._p)


def main() -> None:
    if len(sys.argv) != 3:
        raise SystemExit("Uso: add_sequence_diagrams_to_docx.py entrada.docx salida.docx")

    source = Path(sys.argv[1])
    target = Path(sys.argv[2])
    target.parent.mkdir(parents=True, exist_ok=True)

    document = Document(source)
    insert_diagram_after(
        document,
        "8. Recepción/Depósito → Contabilidad: remitir constancia de recepción y diferencias.",
        "secuencia_aprovisionamiento.png",
    )
    insert_diagram_after(
        document,
        "8. Responsable de clientes → Marketing: aportar el segmento habilitado para campañas.",
        "secuencia_venta_fidelizacion.png",
    )
    insert_diagram_after(
        document,
        "8. Dueño/Compras → Proveedor seleccionado: priorizarlo para la próxima compra del producto.",
        "secuencia_evaluacion_proveedores.png",
    )
    document.save(target)


if __name__ == "__main__":
    main()
