"""Native EA specification for the two existing N03 business views in Docs.

N03 remains a future iteration; this script does not implement its application logic.
"""
import json
from build_ea_model import OUT, diagrams, diagram, node, link, activity

first = len(diagrams)
d = diagram('N03-01', 'N03 · Casos de uso de evaluación de proveedores', 'Use Case',
            'Diseño organizacional previsto para la iteración 3. No implementado en las iteraciones 1 y 2. Conserva CU15-CU20 del documento original.')
for key, name, y in [('owner','Dueño / Compras',70), ('supplier','Proveedor',300), ('accounting','Equipo de Contabilidad',530)]:
    node(d,key,name,30,y,160,100,'Actor')
cases = [
    ('CU15','Solicitar catálogos y antecedentes comerciales'),
    ('CU16','Remitir precios, escalas y certificaciones'),
    ('CU17','Emitir informe de costos, márgenes y rentabilidad'),
    ('CU18','Evaluar prestigio, precio y antigüedad'),
    ('CU19','Asignar proveedor prioritario por producto'),
    ('CU20','Formalizar acuerdo comercial preferencial'),
]
for i,(key,label) in enumerate(cases):
    node(d,key,key+'\n'+label,350,35+i*150,320,105,'UseCase')
for role,case in [('owner','CU15'),('supplier','CU16'),('accounting','CU17'),('owner','CU18'),('owner','CU19'),('owner','CU20'),('supplier','CU20')]:
    link(d,role,case)
link(d,'CU19','CU18',kind='Dependency',stereotype='include')
link(d,'CU20','CU19',kind='Dependency',stereotype='extend')

activity('N03-02','N03 · Actividad de evaluación y selección de proveedores',
 ['Dueño / Compras','Proveedores','Equipo de Contabilidad'],[
 ('start','',0,0,'StateNode'),
 ('products','Identificar productos con baja rotación o riesgo de pérdida',0,1,'Activity'),
 ('ask','Solicitar precios, antecedentes y certificaciones',0,2,'Activity'),
 ('reply','Remitir propuestas comerciales y trayectoria',1,2,'Activity'),
 ('consolidate','Consolidar propuestas comparables por producto',0,3,'Activity'),
 ('cost','Analizar costos, márgenes e impacto financiero',2,3,'Activity'),
 ('opinion','Emitir dictamen de viabilidad y comparación de costos',2,4,'Activity'),
 ('evaluate','Ponderar prestigio, precio y antigüedad',0,5,'Activity'),
 ('threshold','¿Es conveniente?',0,6,'Decision'),
 ('reject','Descartar propuesta y diferir selección',0,7,'Activity'),
 ('endreject','',0,8,'StateNode'),
 ('select','Designar proveedor prioritario por producto',0,9,'Activity'),
 ('propose','Proponer acuerdo comercial preferencial',0,10,'Activity'),
 ('accept','Acordar condiciones de suministro',1,10,'Activity'),
 ('record','Conservar fundamentos de la decisión y el acuerdo',0,11,'Activity'),
 ('endok','',0,12,'StateNode')],
 [('start','products',''),('products','ask',''),('ask','reply',''),('reply','consolidate',''),
  ('consolidate','cost',''),('cost','opinion',''),('opinion','evaluate',''),('evaluate','threshold',''),
  ('threshold','reject','No'),('reject','endreject',''),('threshold','select','Sí'),
  ('select','propose',''),('propose','accept',''),('accept','record',''),('record','endok','')])
diagrams[-1]['notes']='Proceso organizacional de N03, previsto para la iteración 3; no representa funcionalidad implementada en las entregas 1 y 2.'
# Separate the successful branch from the rejected branch, keeping both in Compras.
for n in diagrams[-1]['nodes']:
    if n['key']=='reject': n['w']=155; n['x']=120
    if n['key']=='endreject': n['x']=230
    if n['key'] in ('select','propose','record','endok'): n['x']=max(47,n['x']-15)
(OUT/'model-n03.json').write_text(json.dumps(diagrams[first:],ensure_ascii=False,indent=2),'utf-8')
print('N03: 2 design diagrams generated')
