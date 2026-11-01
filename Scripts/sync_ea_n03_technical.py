"""Update only the N03 physical model and its global projection from the shipped DDL.

Run after build_ea_code_design.py. Existing business models and unrelated diagrams
are preserved; this does not execute SQL or write the EAP database directly.
"""
import json
import re
from copy import deepcopy
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Documentacion/EnterpriseArchitect'
path = OUT / 'model-reconciliation.json'
models = json.loads(path.read_text('utf-8-sig'))
ddl = (ROOT / 'SQL/Iteracion3_Evaluacion_Proveedores.sql').read_text('utf-8-sig')
attrs, fks = [], []
for line in ddl.splitlines():
    match = re.match(r'\s+(\w+)\s+(INT|DATETIME2|NVARCHAR\(\d+\)|NVARCHAR\(MAX\))\s+(.+)', line)
    if not match:
        continue
    name, sqltype, rest = match.groups()
    nullable = 'NOT NULL' not in rest
    fk = re.search(r'REFERENCES dbo\.(\w+)\((\w+)\)', rest)
    attrs.append(dict(name=name, type=sqltype, stereotype='PK' if 'PRIMARY KEY' in rest else 'FK' if fk else '', notes='NULL' if nullable else 'NOT NULL'))
    if fk:
        fks.append(dict(source='EvaluacionProveedor', target=fk[1], name=name, kind='Association', stereotype='FK',
            sourceCardinality='0..1' if name == 'IdOrdenCompra' else '0..*', targetCardinality='0..1' if nullable else '1'))
assert len(attrs) == 10 and len(fks) == 3, (attrs, fks)
global_der = next(d for d in models if d['code'] == 'G08-01')
table = next(n for n in global_der['nodes'] if n['name'] == 'EvaluacionProveedor')
table.update(attributes=deepcopy(attrs), stereotype='table', w=490, h=360,
    notes='SQL/Iteracion3_Evaluacion_Proveedores.sql. Tabla implementada. SnapshotXml conserva solicitud, fuentes y resultados. IdOrdenCompra tiene índice único filtrado.')
global_der['links'] = [l for l in global_der['links'] if 'EvaluacionProveedor' not in (l['source'], l['target'])] + deepcopy(fks)
global_der['notes'] = 'DER global: claves de las tablas existentes y columnas completas de EvaluacionProveedor, implementada en la migración N03. Las relaciones FK opcionales y la unicidad de IdOrdenCompra respetan el DDL.'
for n in global_der['nodes']:
    if n['kind'] == 'Note':
        n['notes'] = 'Atributos no clave de N01/N02: consultar sus vistas por proceso.\nEvaluacionProveedor: tabla implementada; pesos y resultados se conservan en SnapshotXml.\nIdProveedorSeleccionado e IdOrdenCompra son referencias opcionales.\nUn mismo borrador solo puede vincularse a una evaluación.'
# Reflow the global grid: the expanded evaluation table must not cover the
# MovimientoStock table in the following row. Keep the existing row/column order.
rows = {}
for node in global_der['nodes']:
    if node['kind'] != 'Note':
        rows.setdefault(node['y'], []).append(node)
y = 20
for row in sorted(rows):
    for column, node in enumerate(sorted(rows[row], key=lambda item: item['x'])):
        node.update(x=20 + column * 640, y=y)
    y += max(node['h'] for node in rows[row]) + 130
for node in global_der['nodes']:
    if node['kind'] == 'Note':
        node.update(x=20, y=y + 20, w=1770, h=120)
view = next(d for d in models if d['code'] == 'N03-07')
nodes = []
for name, x in [('Producto', 20), ('Proveedor', 400), ('OrdenCompra', 780)]:
    source = next(n for n in global_der['nodes'] if n['name'] == name)
    n = deepcopy(source)
    n.update(x=x, y=30, w=330, h=160, fontSize=12)
    nodes.append(n)
n = deepcopy(table)
n.update(x=285, y=280, w=560, h=420, fontSize=12)
nodes += [n, dict(key='note', name='', kind='Note', x=25, y=780, w=1090, h=135, fontSize=12,
    notes='SnapshotXml conserva criterios, ofertas, datos utilizados y fundamentos del ranking.\nLa decisión agrega proveedor, motivo, responsable y fecha; no se sobrescribe.\nIdOrdenCompra es opcional y único: reintentar devuelve el mismo borrador.\nNo existen columnas EsPrioritario ni pesos separados en esta tabla.')]
view.update(nodes=nodes, links=deepcopy(fks), notes=global_der['notes'])
path.write_text(json.dumps(models, ensure_ascii=False, indent=2), 'utf-8')
physical = [d for d in models if d['code'] in ('N03-07', 'G08-01')]
(OUT / 'n03-data-design.json').write_text(json.dumps(physical, ensure_ascii=False, indent=2), 'utf-8')
print('N03 physical model: 10 actual columns, 3 FKs, optional selection/order and unique order reference')
