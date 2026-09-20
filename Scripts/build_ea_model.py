"""Build the N01/N02 model specification from the business rules and current C#/SQL.

Run before import_ea_model.ps1. Only generates the specification; EA owns rendering.
"""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Documentacion' / 'EnterpriseArchitect'
OUT.mkdir(exist_ok=True)
diagrams = []

def diagram(code, title, kind='Logical', notes=''):
    d = dict(code=code, title=title, kind=kind, notes=notes, nodes=[], links=[])
    diagrams.append(d)
    return d

def node(d, key, name, x, y, w=220, h=90, kind='Class', **kw):
    n = dict(key=key, name=name, x=x, y=y, w=w, h=h, kind=kind, **kw)
    d['nodes'].append(n)
    return n

def link(d, source, target, name='', kind='Association', **kw):
    d['links'].append(dict(source=source, target=target, name=name, kind=kind, **kw))

cu_text = (ROOT / 'Documentacion/casos de uso/Casos_de_Uso_N01_N02.md').read_text('utf-8')
cu_notes = {m[0]: re.sub(r'<br>', '\n', m[1]) for m in re.findall(r'## (CU-N\d+-\d+)[^\n]*\n(.*?)(?=\n## |\Z)', cu_text, re.S)}

for process, roles, cases, relations in [
    ('N01', ['Dueño / Compras', 'Equipo de Contabilidad', 'Proveedor', 'Recepción / Depósito'],
     [('CU-N01-01', 'Preparar y enviar orden a revisión'), ('CU-N01-02', 'Revisar y autorizar compra'), ('CU-N01-03', 'Recepcionar mercadería trazable')],
     [(0,0),(1,1),(0,1),(2,0),(2,2),(3,2)]),
    ('N02', ['Cliente', 'Personal de Ventas'],
     [('CU-N02-01','Adherir cliente al programa de membresía'),('CU-N02-02','Concretar venta con beneficio vigente')],
     [(0,0),(1,0),(0,1),(1,1)])]:
    d=diagram(process+'-01', process+' · Casos de uso principales','Use Case', 'Actores organizacionales. Las especificaciones completas se conservan en las notas de cada caso de uso y en Google Docs.')
    for i,r in enumerate(roles): node(d,'r'+str(i),r,30,i*170+50,160,100,'Actor')
    for i,(code,label) in enumerate(cases): node(d,'c'+str(i),code+'\n'+label,320,i*210+65,290,130,'UseCase',notes=cu_notes[code])
    for r,c in relations: link(d,'r'+str(r),'c'+str(c))

# Activity/process diagrams: lane ownership is organizational, never a software component.
def activity(code,title,roles,steps,edges):
    d=diagram(code,title,'Activity')
    bottom=max(s[3] for s in steps)*125+175
    for i,r in enumerate(roles): node(d,'lane'+str(i),r,20+i*270,20,260,bottom,'ActivityPartition')
    for key,label,lane,row,kind in steps:
        w,h=(35,35) if kind in ('StateNode',) else ((95,65) if kind=='Decision' else (215,70))
        n=node(d,key,label,42+lane*270+(215-w)//2,85+row*125,w,h,kind)
        if key=='start': n['subtype']=100
        if key.startswith('end'): n['subtype']=101
    for a,b,label in edges: link(d,a,b,label,'ControlFlow')
    return d

activity('N01-02','N01 · Actividad de aprovisionamiento',
 ['Dueño / Compras','Proveedores','Contabilidad / Recepción'],[
 ('start','',0,0,'StateNode'),('need','Relevar stock y definir faltantes',0,1,'Activity'),
 ('ask','Solicitar propuestas comparables',0,2,'Activity'),('quote','Entregar precios, vigencia y condiciones',1,2,'Activity'),
 ('compare','Reunir al menos 2 proveedores vigentes por producto',0,3,'Activity'),('draft','Confeccionar orden preliminar',0,4,'Activity'),
 ('review','Contabilidad: revisar comparativa y presupuesto',2,4,'Activity'),('approve','¿Aprueba?',2,5,'Decision'),
 ('reject','Informar motivo de rechazo',1,6,'Activity'),('endreject','',1,7,'StateNode'),
 ('emit','Emitir orden autorizada',0,6,'Activity'),('send','Despachar lotes con remito y origen',1,8,'Activity'),
 ('receive','Recepción: cotejar cantidad, lote y origen',2,8,'Activity'),('valid','¿Conforme?',2,9,'Decision'),
 ('claim','Reclamar diferencias; no ingresar lote inválido',1,10,'Activity'),('endclaim','',1,11,'StateNode'),
 ('stock','Ingresar lotes y actualizar historial de compras',2,10,'Activity'),('endok','',2,11,'StateNode')],
 [('start','need',''),('need','ask',''),('ask','quote',''),('quote','compare',''),('compare','draft',''),('draft','review',''),('review','approve',''),('approve','reject','No'),('reject','endreject',''),('approve','emit','Sí'),('emit','send',''),('send','receive',''),('receive','valid',''),('valid','claim','No'),('claim','endclaim',''),('valid','stock','Sí'),('stock','endok','')])

activity('N02-02','N02 · Proceso de venta y fidelización',
 ['Cliente','Personal de Ventas','Caja / Entrega'],[
 ('start','',0,0,'StateNode'),('request','Seleccionar perfumes y cantidades',0,1,'Activity'),
 ('stock','Revisar existencias y vencimientos',1,1,'Activity'),('enough','¿Hay stock?',1,2,'Decision'),
 ('nostock','Informar faltante y no concretar venta',0,3,'Activity'),('endstock','',0,4,'StateNode'),
 ('member','Consultar pertenencia al programa',1,3,'Activity'),('join','¿Desea adherir?',0,5,'Decision'),
 ('data','Brindar datos y consentimiento de contacto',0,6,'Activity'),('register','Completar ficha y asignar membresía',1,6,'Activity'),
 ('benefit','Verificar beneficio vigente; sin membresía, tarifa general',1,7,'Activity'),
 ('total','Calcular total y confirmar compra',2,8,'Activity'),('pay','Efectuar pago',0,8,'Activity'),
 ('deliver','Entregar lotes con vencimiento más próximo (FEFO)',2,9,'Activity'),('record','Conservar venta, movimientos e historial del cliente',1,10,'Activity'),('endok','',1,11,'StateNode')],
 [('start','request',''),('request','stock',''),('stock','enough',''),('enough','nostock','No'),('nostock','endstock',''),('enough','member','Sí'),('member','join','No es miembro'),('member','benefit','Ya es miembro'),('join','data','Sí'),('join','benefit','No'),('data','register',''),('register','benefit',''),('benefit','total',''),('total','pay',''),('pay','deliver',''),('deliver','record',''),('record','endok','')])

def concepts(code,title,names,edges):
    d=diagram(code,title,notes='Modelo conceptual del negocio: conceptos y cardinalidades, sin clases de software ni base de datos.')
    for i,name in enumerate(names): node(d,name,name,40+(i%3)*310,40+(i//3)*220,210,75,stereotype='concepto')
    for a,b,label,sa,sb in edges: link(d,a,b,label,sourceCardinality=sa,targetCardinality=sb)

concepts('N01-03','N01 · Modelo conceptual',
 ['Proveedor','Cotización','Producto','Orden de compra','Detalle de compra','Lote','Movimiento de stock'],[
 ('Proveedor','Cotización','presenta','1','0..*'),('Proveedor','Orden de compra','recibe','1','0..*'),
 ('Cotización','Producto','ofrece','0..*','1..*'),('Orden de compra','Detalle de compra','contiene','1','1..*'),
 ('Detalle de compra','Producto','solicita','0..*','1'),('Orden de compra','Lote','origina','0..1','0..*'),
 ('Producto','Lote','identifica','1','0..*'),('Proveedor','Lote','suministra','1','0..*'),('Lote','Movimiento de stock','registra','1','0..*')])
concepts('N02-03','N02 · Modelo conceptual',
 ['Cliente','Membresía','Venta','Detalle de venta','Producto','Lote'],[
 ('Cliente','Membresía','posee (máx. 1 activa)','1','0..*'),('Cliente','Venta','realiza','0..1','0..*'),
 ('Venta','Detalle de venta','contiene','1','1..*'),('Detalle de venta','Producto','corresponde a','0..*','1'),
 ('Detalle de venta','Lote','consume','0..*','1'),('Producto','Lote','se almacena en','1','0..*')])

# Import exact public entity properties, not the outdated preliminary Mermaid classes.
def class_node(d,name,x,y,folder='BE'):
    path=ROOT/folder/(name+'.cs')
    src=path.read_text('utf-8-sig')
    attrs=[dict(name=n,type=t) for t,n in re.findall(r'public\s+([\w<>?,.]+)\s+(\w+)\s*\{\s*get\s*;',src)]
    methods=[]
    if folder=='BLL':
        methods=[dict(name=n,type=t) for t,n in re.findall(r'public\s+([\w<>?,.]+)\s+(\w+)\s*\(',src)]
    n=node(d,name,name,x,y,310,max(95,60+20*(len(attrs)+len(methods))),attributes=attrs,methods=methods,stereotype=folder,notes=str(path.relative_to(ROOT)))
    return n

for code,title,names,edges in [
 ('N01-04a','N01 · Clases BE de compras',['Proveedor','CotizacionProveedor','DetalleCotizacionProveedor','OrdenCompra','DetalleOrdenCompra','Producto'],[('Proveedor','CotizacionProveedor'),('CotizacionProveedor','DetalleCotizacionProveedor'),('DetalleCotizacionProveedor','Producto'),('Proveedor','OrdenCompra'),('OrdenCompra','DetalleOrdenCompra'),('DetalleOrdenCompra','Producto')]),
 ('N01-04b','N01 · Clases BE de inventario',['Producto','Lote','MovimientoStock','OrdenCompra','Proveedor'],[('Producto','Lote'),('Proveedor','Lote'),('OrdenCompra','Lote'),('Lote','MovimientoStock')]),
 ('N02-04','N02 · Clases BE de ventas',['Cliente','Membresia','Venta','DetalleVenta','Producto','Lote'],[('Cliente','Membresia'),('Cliente','Venta'),('Venta','DetalleVenta'),('Producto','DetalleVenta'),('Lote','DetalleVenta')])]:
    d=diagram(code,title,notes='Propiedades públicas extraídas de los archivos BE. Las asociaciones representan referencias por identificador; no agregan navegación inexistente al código.')
    for i,n in enumerate(names): class_node(d,n,40+(i%3)*420,40+(i//3)*470)
    for a,b in edges: link(d,a,b)

for process,names in [('N01',['AprovisionamientoBLL','InventarioBLL']),('N02',['VentaBLL'])]:
    d=diagram(process+'-05',process+' · Clases de lógica de negocio (BLL)',notes='Operaciones públicas verificadas en BLL. Los contratos de persistencia se muestran como dependencias, no como participantes de los procesos organizacionales.')
    interfaces=[]
    for i,name in enumerate(names):
        class_node(d,name,40,50+i*370,'BLL')
        src=(ROOT/'BLL'/(name+'.cs')).read_text('utf-8-sig')
        for contract in re.findall(r'private readonly (I\w+) ',src):
            if contract not in interfaces: interfaces.append(contract)
            link(d,name,contract,'usa','Dependency')
    for i,name in enumerate(interfaces): node(d,name,name,620,40+i*145,280,75,'Interface')

def sequence(code,title,roles,messages,notes):
    d=diagram(code,title,'Sequence',notes)
    for i,name in enumerate(roles): node(d,str(i),name,40+i*290,50,175,90,'Object',stereotype='rol de negocio')
    for i,(a,b,label) in enumerate(messages): link(d,str(a),str(b),label,'Sequence',sequence=i+1)

sequence('N01-06a','N01 · Secuencia: preparar y revisar compra',
 ['Dueño / Compras','Proveedores','Contabilidad'],[
 (0,1,'Solicitar cotizaciones por producto'),(1,0,'Entregar precios, plazos y vigencia'),
 (0,0,'Comparar al menos dos proveedores vigentes'),(0,0,'Confeccionar orden preliminar'),
 (0,2,'Presentar orden y comparativa'),(2,2,'Revisar condiciones y presupuesto'),
 (2,0,'[Aprobada] Autorizar compra'),(0,1,'[Aprobada] Emitir orden formal'),
 (2,0,'[Rechazada] Informar motivo; no emitir')],
 'CU-N01-01 y CU-N01-02. Las guardas Aprobada/Rechazada son alternativas excluyentes; no ocurren ambas en una ejecución. Solicitud y respuesta se repiten por proveedor.')
sequence('N01-06b','N01 · Secuencia: recepción y trazabilidad',
 ['Proveedor','Recepción / Depósito','Dueño / Compras'],[
 (0,1,'Entregar mercadería, remito y certificado'),(1,2,'Cotejar con orden emitida'),(2,1,'Confirmar productos y cantidades pendientes'),
 (1,1,'Verificar lote, origen y vencimiento'),(1,0,'[No conforme] Reclamar; no ingresar'),
 (1,1,'[Conforme] Identificar lotes y registrar ingreso'),(1,2,'Informar cantidades efectivamente recibidas'),
 (2,2,'[Faltantes] Conservar compra recibida parcialmente'),(2,2,'[Completa] Cerrar recepción y archivar compra')],
 'CU-N01-03. Conforme/No conforme y Faltantes/Completa son ramas excluyentes. El ingreso conserva proveedor, orden, lote y movimiento de stock.')
sequence('N02-06a','N02 · Secuencia: adhesión a membresía',
 ['Cliente','Personal de Ventas'],[
 (1,0,'Ofrecer membresía y explicar beneficios'),(0,1,'[Acepta] Entregar identificación y contacto'),
 (1,0,'Solicitar consentimiento de marketing'),(0,1,'Comunicar preferencias y autorización elegida'),
 (1,1,'Verificar datos y ausencia de duplicados'),(1,1,'Asignar número de socio y descuento de 0 a 30 %'),
 (1,0,'Confirmar adhesión y beneficio'),(1,0,'[Datos inválidos] Solicitar corrección')],
 'CU-N02-01. El consentimiento de marketing es opcional y no condiciona la membresía. Si no acepta, la venta continúa sin alta. Datos inválidos es una alternativa al alta exitosa.')
sequence('N02-06b','N02 · Secuencia: venta con beneficio',
 ['Cliente','Personal de Ventas','Caja / Entrega'],[
 (0,1,'Solicitar perfumes y cantidades'),(1,1,'Verificar disponibilidad y vigencia de lotes'),
 (1,0,'[Sin stock] Informar faltante; no vender'),(0,1,'[Si se identifica] Presentar membresía'),
 (1,1,'Determinar beneficio vigente o tarifa general'),(1,2,'Entregar detalle e importe final'),
 (2,0,'Solicitar pago'),(0,2,'Confirmar compra y efectuar pago'),
 (2,0,'Entregar perfumes y comprobante'),(1,1,'Conservar venta y salida por lote FEFO')],
 'CU-N02-02. Sin stock interrumpe la venta. Se permite cliente ocasional sin descuento. Pago y entrega son actividades organizacionales; no se modela una pasarela de pagos implementada.')

# Exact physical SQL columns and FK relations, grouped into legible views.
tables={}
for path in [ROOT/'SQL/Iteracion2_Aprovisionamiento.sql',ROOT/'SQL/Iteracion2_Ventas_Fidelizacion.sql']:
    src=path.read_text('utf-8-sig')
    for name,body in re.findall(r'CREATE TABLE dbo\.(\w+)\s*\((.*?)\n    \);',src,re.S):
        attrs=[]
        fks=re.findall(r'FOREIGN KEY \((\w+)\) REFERENCES dbo\.(\w+)\((\w+)\)',body)
        for line in body.splitlines():
            m=re.match(r'\s+(\w+)\s+((?:INT|NVARCHAR|VARCHAR|DECIMAL|BIT|DATE|DATETIME)(?:\([^)]*\))?)\s+(.*)',line)
            if m:
                n,t,tail=m.groups()
                attrs.append(dict(name=n,type=t,stereotype='PK' if 'PRIMARY KEY' in tail else ('FK' if any(f[0]==n for f in fks) else ''),notes='NOT NULL' if 'NOT NULL' in tail else 'NULL'))
        tables[name]=dict(attrs=attrs,fks=fks,notes=body)
tables['Usuarios']=dict(attrs=[dict(name='Id',type='INT',stereotype='PK')],fks=[],notes='Referencia a la tabla existente de seguridad. Se muestra solo la clave utilizada por este proceso.')
for code,title,names in [
 ('N01-07a','N01 · Datos: cotizaciones y compras',['Proveedor','CotizacionProveedor','DetalleCotizacionProveedor','OrdenCompra','DetalleOrdenCompra','Producto','Usuarios']),
 ('N01-07b','N01 · Datos: inventario y trazabilidad',['Producto','Lote','MovimientoStock','Proveedor','OrdenCompra']),
 ('N02-07','N02 · Datos: ventas y membresías',['Cliente','Membresia','Venta','DetalleVenta','Producto','Lote','Usuarios'])]:
    d=diagram(code,title,notes='Modelo físico extraído de los scripts SQL de la iteración 2 (SQL Server major 16). PK/FK y nulabilidad se conservan en los atributos. Las tablas compartidas se reutilizan conceptualmente entre vistas.')
    for i,n in enumerate(names):
        t=tables[n]
        node(d,n,n,40+(i%3)*425,40+(i//3)*430,315,65+len(t['attrs'])*21,attributes=t['attrs'],stereotype='table',notes=t['notes'])
        for col,target,targetcol in t['fks']:
            if target in names: link(d,n,target,col,sourceCardinality='0..*',targetCardinality='0..1' if next(a for a in t['attrs'] if a['name']==col)['notes']=='NULL' else '1')

(OUT/'model.json').write_text(json.dumps(diagrams,ensure_ascii=False,indent=2),'utf-8')
print(f'{len(diagrams)} diagrams; {sum(len(d["nodes"]) for d in diagrams)} elements')
