"""Revise the five business use cases and prepare exact Google Docs replacements."""
import json, re, sys
from pathlib import Path
from docx import Document
ROOT = Path(__file__).resolve().parents[1]
path = ROOT / 'Documentacion/casos de uso/Casos_de_Uso_N01_N02.md'
flows = [
[
'El Dueño / Departamento de Compras solicita preparar una compra.',
'El sistema presenta las opciones para registrar cotizaciones y preparar la orden.',
'Compras informa las cotizaciones recibidas y selecciona proveedor, productos, cantidades y precios.',
'El sistema muestra la orden en borrador con su importe total y señala los datos que requieren corrección.',
'Compras revisa el borrador y solicita enviarlo a Contabilidad.',
'El sistema informa si la comparación de ofertas permite presentar la compra o qué cotizaciones faltan.',
'Compras consulta el resultado y el sistema muestra la orden Pendiente de aprobación para su seguimiento.'
], [
'El Equipo de Contabilidad solicita consultar las compras pendientes de revisión.',
'El sistema presenta las órdenes pendientes con proveedor, productos e importes.',
'Contabilidad selecciona una orden para examinarla.',
'El sistema muestra el detalle y la información disponible para decidir.',
'Contabilidad evalúa la compra y solicita aprobarla.',
'El sistema confirma la aprobación y muestra el nuevo estado de la orden.'
], [
'Recepción / Depósito solicita registrar una entrega y selecciona la orden correspondiente.',
'El sistema muestra los productos y las cantidades pendientes de recibir.',
'Recepción informa los productos entregados, cantidades y datos identificatorios de sus lotes.',
'El sistema informa si la entrega es admisible o identifica los datos y diferencias que deben corregirse.',
'Recepción revisa los datos informados y confirma la recepción.',
'El sistema confirma el ingreso e informa si la orden quedó recibida total o parcialmente.',
'Recepción solicita consultar las existencias y el origen de la mercadería ingresada.',
'El sistema muestra el stock actualizado y su trazabilidad hacia el proveedor y la compra de origen.'
], [
'El vendedor solicita registrar al cliente que aceptó adherirse al programa de membresía.',
'El sistema presenta los datos y opciones de adhesión disponibles.',
'El vendedor informa los datos del cliente, su consentimiento de marketing y el beneficio acordado.',
'El sistema señala datos faltantes, un correo ya registrado o un beneficio fuera del rango permitido.',
'El vendedor completa o corrige la información y confirma el alta.',
'El sistema confirma el registro y muestra el resultado de la adhesión.',
'El vendedor consulta el beneficio y el sistema informa la membresía activa y su descuento aplicable.'
], [
'El vendedor solicita registrar una venta e indica los productos, cantidades y, si corresponde, el cliente.',
'El sistema muestra los productos elegidos y la identificación del cliente o la condición de cliente ocasional.',
'El vendedor revisa la selección y solicita confirmar la venta.',
'El sistema informa si puede concretarse con las existencias disponibles o identifica el impedimento.',
'El vendedor consulta el resultado de la operación.',
'El sistema muestra la venta registrada y el importe con el beneficio de membresía que corresponda.',
'El vendedor solicita consultar las existencias luego de la venta.',
'El sistema informa las existencias resultantes y permite relacionar los productos vendidos con sus lotes.'
]]
# Keep the observable confirmation order of the existing implementation: sale totals
# are returned after confirmation, rather than inventing a pre-confirmation preview.
flows[4][-2:] = [
'El vendedor verifica la información de la venta finalizada.',
'El sistema mantiene disponible el resultado con el subtotal, descuento y total de la operación.'
]
exceptions = [
[
'E1: Sin permiso. El sistema informa que el actor no puede preparar la compra y no acepta la solicitud.',
'E2: Cotización inválida. El sistema indica los datos inválidos o vencidos; Compras los corrige y vuelve a presentarlos.',
'E3: Datos inactivos. El sistema identifica al proveedor o producto no disponible; Compras modifica la selección.',
'E4: Comparativa insuficiente. El sistema informa qué productos no tienen dos cotizaciones vigentes de proveedores distintos; Compras completa la comparación y reintenta el envío. La orden permanece en Borrador.'
], [
'E1: Rechazo. Contabilidad solicita rechazar la compra e informa el motivo; el sistema confirma el rechazo. Si falta el motivo, solicita completarlo.',
'E2: Estado inválido. El sistema informa que la orden no está pendiente y no admite una nueva decisión; Contabilidad consulta su estado actual.',
'E3: Cambio concurrente. El sistema informa que la orden cambió; Contabilidad actualiza la consulta antes de continuar.',
'E4: Sin permiso contable. El sistema informa que el actor no está autorizado y no acepta la decisión.'
], [
'E1: Sin lotes. El sistema solicita informar al menos un lote; Recepción completa la entrega antes de reintentar.',
'E2: Lote inválido. El sistema identifica los datos faltantes, la cantidad no positiva o el vencimiento inválido; Recepción los corrige.',
'E3: Diferencia inválida. El sistema informa el producto ajeno a la orden o la cantidad que supera lo pendiente; Recepción revisa la entrega y vuelve a presentarla.',
'E4: Estado o permiso inválido. El sistema informa por qué la orden no puede recibirse o el actor no está autorizado; la recepción no se confirma.'
], [
'E1: Cliente duplicado. El sistema informa que el correo ya está registrado; el vendedor busca al cliente existente y evita un alta duplicada.',
'E2: Datos inválidos. El sistema identifica los datos obligatorios faltantes o el correo inválido; el vendedor los corrige y reintenta.',
'E3: Descuento inválido. El sistema informa que el beneficio debe estar entre 0 % y 30 %; el vendedor lo corrige. Si el cliente ya quedó registrado, consulta ese registro antes de reintentar la adhesión.',
'E4: Sin permiso de ventas. El sistema informa que el vendedor no está autorizado y no acepta el alta.'
], [
'E1: Detalle inválido. El sistema solicita al vendedor agregar productos y cantidades positivas; el vendedor corrige la selección.',
'E2: Producto inválido. El sistema identifica el producto inexistente o inactivo; el vendedor lo retira o reemplaza.',
'E3: Stock insuficiente. El sistema informa la falta de existencias y no confirma la venta; el vendedor ajusta cantidades o productos y reintenta.',
'E4: Sin permiso de ventas. El sistema informa que el actor no está autorizado y no acepta la venta.'
]]
for i in (3,4):
    exceptions[i][-1]='E4: Sin permiso de ventas. El sistema informa que el actor no está autorizado y no acepta la operación.'
text = path.read_text(encoding='utf-8')
sections = re.split(r'(?=## CU-)', text)
baseline = Document(sys.argv[1])
changes=[]
for i, section in enumerate(sections[1:]):
    code = re.search(r'CU-N\d+-\d+', section)[0]
    values = {'Flujo Normal': [f'{j}. {s}' for j,s in enumerate(flows[i],1)], 'Excepciones': exceptions[i]}
    if i==1:
        values['Poscondiciones']=['La orden queda Aprobada o Rechazada y se informa el resultado. Compras realiza su emisión posterior.']
    if i==4:
        values['Objetivo']=['Concretar una venta e informar el importe y el beneficio de membresía aplicable.']
    table=next(t for t in baseline.tables if any(code==c.text for r in t.rows for c in r.cells))
    for field, newlines in values.items():
        oldcell=next(r.cells[1] for r in table.rows if r.cells[0].text==field)
        oldlines=oldcell.text.split('\n')
        assert len(oldlines)==len(newlines),(code,field,oldlines,newlines)
        for old,new in zip(oldlines,newlines):
            if old!=new: changes.append({'old':old,'new':new,'code':code,'field':field})
        section=re.sub(r'(\| \*\*'+field+r'\*\* \| ).*?( \|)',lambda m:m[1]+'<br>'.join(newlines)+m[2],section)
    sections[i+1]=section
path.write_text(''.join(sections),encoding='utf-8')
model_path=ROOT/'Documentacion/EnterpriseArchitect/model.json'
model=json.loads(model_path.read_text(encoding='utf-8'))
for section in sections[1:]:
    code=re.search(r'CU-N\d+-\d+',section)[0]
    notes=section.split('\n',1)[1].replace('<br>','\n')
    for diagram in model:
        for node in diagram['nodes']:
            if node.get('kind')=='UseCase' and node['name'].startswith(code): node['notes']=notes
model_path.write_text(json.dumps(model,ensure_ascii=False,indent=2),encoding='utf-8')
(ROOT/'Documentacion/EnterpriseArchitect/actor-document-replacements.json').write_text(json.dumps(changes,ensure_ascii=False,indent=2),encoding='utf-8')
print(len(changes),'exact paragraph replacements prepared')
