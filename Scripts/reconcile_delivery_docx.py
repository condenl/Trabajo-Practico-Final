from __future__ import annotations

import sys
from pathlib import Path

from docx import Document
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Inches, Pt, RGBColor


def set_cell_fill(cell, color: str) -> None:
    tc_pr = cell._tc.get_or_add_tcPr()
    shading = tc_pr.find(qn("w:shd"))
    if shading is None:
        shading = OxmlElement("w:shd")
        tc_pr.append(shading)
    shading.set(qn("w:fill"), color)


def set_cell_margins(cell, top=100, start=120, bottom=100, end=120) -> None:
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    tc_mar = tc_pr.first_child_found_in("w:tcMar")
    if tc_mar is None:
        tc_mar = OxmlElement("w:tcMar")
        tc_pr.append(tc_mar)
    for margin, value in (("top", top), ("start", start), ("bottom", bottom), ("end", end)):
        node = tc_mar.find(qn(f"w:{margin}"))
        if node is None:
            node = OxmlElement(f"w:{margin}")
            tc_mar.append(node)
        node.set(qn("w:w"), str(value))
        node.set(qn("w:type"), "dxa")


def set_table_borders(table) -> None:
    tbl_pr = table._tbl.tblPr
    borders = tbl_pr.find(qn("w:tblBorders"))
    if borders is None:
        borders = OxmlElement("w:tblBorders")
        tbl_pr.append(borders)
    for edge in ("top", "left", "bottom", "right", "insideH", "insideV"):
        node = OxmlElement(f"w:{edge}")
        node.set(qn("w:val"), "single")
        node.set(qn("w:sz"), "4")
        node.set(qn("w:space"), "0")
        node.set(qn("w:color"), "D9D9D9")
        borders.append(node)


def move_after(reference, element) -> None:
    reference.addnext(element)


def normalize_section_measurements(document: Document) -> None:
    """Google Docs can export twip attributes as decimals; Word accepts them but python-docx does not."""
    for section in document.sections:
        page_margins = section._sectPr.find(qn("w:pgMar"))
        if page_margins is None:
            continue
        for name in ("top", "right", "bottom", "left", "header", "footer", "gutter"):
            attribute = qn(f"w:{name}")
            value = page_margins.get(attribute)
            if value and "." in value:
                page_margins.set(attribute, str(round(float(value))))


def replace_heading(document: Document, starts_with: str, replacement: str) -> None:
    paragraph = next((p for p in document.paragraphs if p.text.strip().startswith(starts_with)), None)
    if paragraph is None:
        if any(p.text.strip() == replacement for p in document.paragraphs):
            return
        raise ValueError(f"No se encontro el encabezado: {starts_with}")
    paragraph.text = replacement
    for run in paragraph.runs:
        run.font.color.rgb = RGBColor(0, 0, 0)


def remove_paragraph(paragraph) -> None:
    element = paragraph._element
    element.getparent().remove(element)


def remove_verification_appendix(document: Document) -> None:
    title = "2.6 Evidencia de implementación y verificación de las Iteraciones 1 y 2"
    paragraphs = document.paragraphs
    start = next((i for i, paragraph in enumerate(paragraphs) if paragraph.text.strip() == title), None)
    if start is None:
        return
    for paragraph in paragraphs[start : start + 6]:
        remove_paragraph(paragraph)


def remove_use_case_specifications(document: Document) -> None:
    title = "2.6 Especificación de Casos de Uso Principales"
    heading = next((p for p in document.paragraphs if p.text.strip() == title), None)
    if heading is None:
        return
    parent = heading._element.getparent()
    element = heading._element
    while element is not None:
        following = element.getnext()
        if element.tag != qn("w:sectPr"):
            parent.remove(element)
        element = following


def add_use_case_specifications(document: Document) -> None:
    title = "2.6 Especificación de Casos de Uso Principales"
    if any(paragraph.text.strip() == title for paragraph in document.paragraphs):
        return

    heading = document.add_paragraph(title, style="Heading 2")
    heading.paragraph_format.keep_with_next = True
    heading.paragraph_format.page_break_before = True
    for run in heading.runs:
        run.font.color.rgb = RGBColor(0, 0, 0)

    use_cases = (
        (
            "Preparar y enviar orden de compra a revisión",
            "CU-N01-01",
            "Registrar cotizaciones comparables, crear una orden preliminar y remitirla a Contabilidad.",
            "El usuario posee permiso de aprovisionamiento. El proveedor y los productos están activos.",
            "La orden queda pendiente de aprobación contable y la operación queda registrada en bitácora.",
            "El Dueño o responsable de Compras decide reponer uno o más productos.",
            "1. Registra las cotizaciones vigentes recibidas de proveedores.\n"
            "2. Selecciona un proveedor y agrega los productos, cantidades y precios.\n"
            "3. El sistema valida proveedor, productos, cantidades y precios.\n"
            "4. El sistema calcula el total y guarda la orden en estado Borrador.\n"
            "5. El usuario solicita enviarla a revisión.\n"
            "6. El sistema verifica que cada producto tenga al menos dos cotizaciones vigentes de proveedores distintos.\n"
            "7. El sistema cambia la orden a Pendiente de aprobación y registra el evento.",
            "E1: Sin permiso. Se rechaza la operación.\n"
            "E2: Cotización vencida, precio no positivo, plazo negativo o producto duplicado. No se registra.\n"
            "E3: Proveedor o producto inexistente o inactivo. No se crea la orden.\n"
            "E4: Faltan dos cotizaciones comparables para algún producto. La orden permanece en Borrador.",
        ),
        (
            "Revisar orden de compra",
            "CU-N01-02",
            "Permitir que Contabilidad apruebe o rechace una compra antes de emitirla al proveedor.",
            "El usuario posee permiso contable y la orden está Pendiente de aprobación.",
            "La orden queda Aprobada o Rechazada con la identidad del aprobador o el motivo de rechazo.",
            "Contabilidad recibe una orden pendiente para controlar su conveniencia económica.",
            "1. Contabilidad consulta la orden, su proveedor, sus productos y el total.\n"
            "2. Verifica la documentación y la disponibilidad presupuestaria.\n"
            "3. Selecciona Aprobar.\n"
            "4. El sistema valida el estado actual de la orden.\n"
            "5. El sistema registra al usuario aprobador, cambia el estado a Aprobada y deja trazabilidad en bitácora.\n"
            "6. El responsable de Compras emite la orden aprobada al proveedor.",
            "E1: Rechazo. Contabilidad informa un motivo obligatorio y la orden queda Rechazada.\n"
            "E2: La orden no está Pendiente de aprobación. No se permite decidir nuevamente.\n"
            "E3: La orden cambió de estado durante la operación. Se solicita volver a consultarla.\n"
            "E4: Sin permiso contable. Se rechaza la operación.",
        ),
        (
            "Recepcionar mercadería y registrar trazabilidad",
            "CU-N01-03",
            "Registrar la recepción total o parcial de una orden y crear lotes trazables con su ingreso de stock.",
            "El usuario posee permiso de recepción. La orden está Emitida o Recibida parcialmente.",
            "Se crean los lotes y movimientos de ingreso. La orden queda Recibida o Recibida parcialmente.",
            "El proveedor entrega mercadería correspondiente a una orden emitida.",
            "1. Recepción selecciona la orden emitida.\n"
            "2. Informa producto, código de lote, origen, certificado, cantidad y vencimiento.\n"
            "3. El sistema valida que el producto pertenezca a la orden y que el lote sea válido.\n"
            "4. El sistema controla que la recepción acumulada no supere lo comprado.\n"
            "5. Registra cada lote con el proveedor y la orden de origen.\n"
            "6. Registra los movimientos de ingreso y actualiza el stock disponible.\n"
            "7. Marca la orden como Recibida si se completaron todas las cantidades o como Recibida parcialmente en caso contrario.\n"
            "8. Registra la recepción en bitácora.",
            "E1: No se informan lotes. No se registra la recepción.\n"
            "E2: Código, origen o certificado faltante, cantidad no positiva o lote vencido. Se rechaza el lote.\n"
            "E3: Producto ajeno a la orden o cantidad superior a la comprada. Se cancela la recepción.\n"
            "E4: Orden en estado no recepcionable o usuario sin permiso. Se rechaza la operación.",
        ),
        (
            "Adherir cliente al programa de membresía",
            "CU-N02-01",
            "Registrar al cliente que acepta participar del programa de fidelización y asignarle su beneficio.",
            "El vendedor posee permiso de ventas y el cliente presta conformidad para asociarse.",
            "El cliente queda registrado y, si corresponde, obtiene una membresía activa con su porcentaje de descuento.",
            "Durante la atención, el cliente acepta asociarse a la perfumería.",
            "1. El vendedor solicita nombre, apellido, correo y preferencias autorizadas.\n"
            "2. El sistema valida los datos obligatorios y el formato del correo.\n"
            "3. El sistema verifica que el correo no esté registrado.\n"
            "4. El vendedor confirma la creación de la membresía y el porcentaje de descuento.\n"
            "5. El sistema valida que el descuento esté entre 0 % y 30 %.\n"
            "6. El sistema registra al cliente, genera su número de socio y crea la membresía.\n"
            "7. El sistema registra el alta para auditoría.",
            "E1: El cliente ya existe. Se informa la duplicidad y no se crea otro registro.\n"
            "E2: Datos obligatorios incompletos o correo inválido. Se solicita corregirlos.\n"
            "E3: Descuento fuera del rango permitido. No se crea la membresía.\n"
            "E4: Sin permiso de ventas. Se rechaza la operación.",
        ),
        (
            "Registrar venta con beneficio de membresía",
            "CU-N02-02",
            "Concretar una venta, aplicar el beneficio vigente del cliente y descontar stock por lote usando FEFO.",
            "El vendedor posee permiso de ventas. Los productos están activos y tienen stock disponible.",
            "La venta y sus detalles quedan registrados, el stock se descuenta por lote y la operación queda auditada.",
            "El cliente confirma los productos que desea comprar.",
            "1. El vendedor agrega los productos y cantidades solicitadas.\n"
            "2. Si el cliente se identifica, el sistema recupera su membresía activa.\n"
            "3. El sistema verifica productos activos y stock suficiente.\n"
            "4. El sistema asigna las unidades a los lotes disponibles con vencimiento más próximo.\n"
            "5. Calcula subtotal, descuento de membresía y total.\n"
            "6. El vendedor confirma la operación.\n"
            "7. El sistema registra la venta y sus detalles, descuenta el stock y registra el evento en bitácora.\n"
            "8. La venta finaliza con el cliente identificado o como cliente ocasional sin descuento.",
            "E1: Venta sin productos o con cantidades no positivas. No se registra.\n"
            "E2: Producto inexistente o inactivo. Se informa el producto afectado.\n"
            "E3: Stock insuficiente. Se cancela la venta sin modificar existencias.\n"
            "E4: Sin permiso de ventas. Se rechaza la operación.",
        ),
    )

    labels = (
        "Nombre del Caso de Uso",
        "Código",
        "Objetivo",
        "Precondiciones",
        "Poscondiciones",
        "Eventos Disparadores",
        "Flujo Normal",
        "Excepciones",
    )
    widths = (Inches(1.65), Inches(4.7))
    for case_index, values in enumerate(use_cases):
        case_heading = document.add_paragraph(values[0], style="Heading 3")
        case_heading.paragraph_format.keep_with_next = True
        if case_index > 0:
            case_heading.paragraph_format.page_break_before = True
        for run in case_heading.runs:
            run.font.color.rgb = RGBColor(0, 0, 0)

        table = document.add_table(rows=len(labels), cols=2)
        set_table_borders(table)
        table.autofit = False
        for row_index, (label, value) in enumerate(zip(labels, values)):
            left, right = table.rows[row_index].cells
            left.width = widths[0]
            right.width = widths[1]
            left.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            right.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            set_cell_margins(left)
            set_cell_margins(right)
            left.text = label
            right.text = value
            if row_index == 0:
                set_cell_fill(left, "1F4E78")
                set_cell_fill(right, "1F4E78")
                for cell in (left, right):
                    for paragraph in cell.paragraphs:
                        for run in paragraph.runs:
                            run.bold = True
                            run.font.color.rgb = RGBColor(255, 255, 255)
            else:
                set_cell_fill(left, "EAF2F8")
                for run in left.paragraphs[0].runs:
                    run.bold = True
            for cell in (left, right):
                for paragraph in cell.paragraphs:
                    paragraph.paragraph_format.space_after = Pt(0)
                    paragraph.paragraph_format.line_spacing = 1
                    for run in paragraph.runs:
                        run.font.size = Pt(9)


def main() -> None:
    if len(sys.argv) != 3:
        raise SystemExit("Uso: reconcile_delivery_docx.py entrada.docx salida.docx")

    source = Path(sys.argv[1])
    target = Path(sys.argv[2])
    document = Document(source)
    normalize_section_measurements(document)

    anchor = next((p for p in document.paragraphs if p.text.strip() == "Negocio Perfumeria"), None)
    if anchor is None:
        anchor = next((p for p in document.paragraphs if p.text.strip().startswith("Negocio Perfumer")), None)
    if anchor is None:
        raise ValueError("No se encontro la seccion Negocio Perfumeria")

    if not any(p.text.strip() == "Correspondencia con el plan de entregas" for p in document.paragraphs):
        heading = document.add_paragraph("Correspondencia con el plan de entregas", style="Heading 2")
        heading.paragraph_format.keep_with_next = True
        for run in heading.runs:
            run.font.color.rgb = RGBColor(0, 0, 0)

        intro = document.add_paragraph(
            "La primera entrega documenta G01 a G08 y el analisis y diseno de N01. "
            "La segunda entrega implementa N01, desarrolla N02 de punta a punta y prepara A01. "
            "N03 se conserva como proceso futuro de valor agregado para la tercera iteracion."
        )

        table = document.add_table(rows=1, cols=3)
        set_table_borders(table)
        table.autofit = False
        widths = (Inches(0.85), Inches(3.85), Inches(1.65))
        headers = ("Codigo", "Proceso", "Estado al cierre de Iteracion 2")
        for cell, header, width in zip(table.rows[0].cells, headers, widths):
            cell.width = width
            cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            set_cell_fill(cell, "1F4E78")
            set_cell_margins(cell)
            cell.text = header
            for paragraph in cell.paragraphs:
                for run in paragraph.runs:
                    run.bold = True
                    run.font.color.rgb = RGBColor(255, 255, 255)
                    run.font.size = Pt(9)

        rows = (
            ("N01", "Aprovisionamiento, aprobacion contable, inventario y trazabilidad", "Disenado e implementado"),
            ("N02", "Venta en mostrador y fidelizacion de clientes", "Disenado e implementado"),
            ("N03", "Evaluacion y recomendacion de proveedores", "Previsto para Iteracion 3"),
            ("A01", "Instalador inicial y preparacion de la base de datos", "Fuente preparada"),
        )
        for row_index, values in enumerate(rows, start=1):
            cells = table.add_row().cells
            for cell, value, width in zip(cells, values, widths):
                cell.width = width
                cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
                set_cell_margins(cell)
                if row_index % 2 == 0:
                    set_cell_fill(cell, "EAF2F8")
                cell.text = value
                for paragraph in cell.paragraphs:
                    for run in paragraph.runs:
                        run.font.size = Pt(9)

        move_after(anchor._p, heading._p)
        move_after(heading._p, intro._p)
        move_after(intro._p, table._tbl)

    replace_heading(
        document,
        "Proceso 1:",
        "Proceso 1 N01 Aprovisionamiento de Perfumes Trazabilidad y Aprobacion Contable",
    )
    replace_heading(
        document,
        "Proceso 2:",
        "Proceso 2 N02 Venta y Fidelizacion de Clientes",
    )
    replace_heading(
        document,
        "Proceso 3:",
        "Proceso 3 N03 Evaluacion y Seleccion de Proveedores prevista para Iteracion 3",
    )

    sequence_n03 = next(
        (p for p in document.paragraphs if p.text.strip().startswith("Proceso 3:")),
        None,
    )
    if sequence_n03 is not None:
        sequence_n03.text = "Proceso 3 N03 Evaluacion y seleccion de proveedores prevista para Iteracion 3"

    implementation = next(
        (p for p in document.paragraphs if p.text.strip().startswith("Correspondencia con la implementaci")),
        None,
    )
    if implementation is not None:
        implementation.text = (
            "Correspondencia con la implementacion: N01 se refleja en ProveedorBLL, "
            "AprovisionamientoBLL e InventarioBLL; N02 se refleja en VentaBLL, que registra clientes, "
            "membresias, ventas y egresos FEFO; N03 queda documentado y previsto para la Iteracion 3."
        )

    remove_verification_appendix(document)
    remove_use_case_specifications(document)
    add_use_case_specifications(document)

    target.parent.mkdir(parents=True, exist_ok=True)
    document.save(target)


if __name__ == "__main__":
    main()
