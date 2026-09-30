"""Reconcile organizational sequences and global design views with the project folder.

N03 and marketing coordination are design, not claims of implemented automation.
The original N01/N02 detail diagrams are retained.
"""
import json
from copy import deepcopy
from build_ea_model import OUT, ROOT, diagrams, diagram, node, link, sequence, tables

start = len(diagrams)
sequence('N01-06a','N01 · Secuencia: preparar y revisar compra',
 ['Dueño / Compras','Proveedores','Contabilidad'],[
 (0,1,'Solicitar cotizaciones comparables'),
 (1,0,'Informar precio, plazo, condiciones y vigencia'),
 (0,0,'Reunir dos proveedores vigentes por producto'),
 (0,1,'[Ofertas insuficientes] Solicitar nuevas ofertas; diferir orden'),
 (0,0,'[Ofertas suficientes] Comparar y preparar orden preliminar'),
 (0,2,'[Ofertas suficientes] Remitir orden y comparativa'),
 (2,2,'Revisar condiciones y presupuesto'),
 (2,0,'[Rechazada] Informar motivo; no emitir compra'),
 (2,0,'[Aprobada] Autorizar compra'),
 (0,1,'[Aprobada] Emitir orden formal autorizada')],
 'Diseño organizacional. Ofertas insuficientes interrumpe la preparación; Aprobada/Rechazada son excluyentes. Solicitud y respuesta se repiten por proveedor. N01 implementa la aprobación antes de emitir.')
sequence('N01-06b','N01 · Secuencia: recepción y trazabilidad',
 ['Proveedor','Recepción / Depósito','Dueño / Compras','Contabilidad'],[
 (0,1,'Entregar perfumes, remito y certificados'),
 (1,2,'Solicitar orden emitida y cantidades pendientes'),
 (2,1,'Informar detalle autorizado y saldo pendiente'),
 (1,1,'Cotejar producto, cantidad, origen, lote y vencimiento'),
 (1,0,'[No conforme] Reclamar diferencias; no ingresar lote inválido'),
 (1,3,'[No conforme] Remitir constancia de diferencias'),
 (1,1,'[Conforme] Identificar lotes y registrar ingreso'),
 (1,2,'[Conforme] Informar cantidades y trazabilidad recibidas'),
 (1,3,'[Conforme] Remitir constancia de recepción y diferencias'),
 (2,2,'[Faltantes] Conservar recepción parcial y saldo pendiente'),
 (2,2,'[Completa] Cerrar recepción y archivar compra')],
 'Diseño de recepción. Conforme/No conforme son alternativas; Faltantes/Completa solo aplican a recepción conforme. El intercambio con Contabilidad es una responsabilidad organizacional y no presupone una notificación automática.')
sequence('N02-06a','N02 · Secuencia: adhesión a membresía',
 ['Cliente','Personal de Ventas','Responsable de Clientes'],[
 (1,0,'Ofrecer membresía y explicar beneficios'),
 (0,1,'[No acepta] Continuar como cliente ocasional'),
 (0,1,'[Acepta] Brindar identificación y datos de contacto'),
 (1,0,'[Acepta] Consultar preferencias y permiso de marketing'),
 (0,1,'[Acepta] Informar preferencias y consentimiento elegido'),
 (1,2,'[Acepta] Remitir solicitud de adhesión y datos'),
 (2,2,'Validar datos y ausencia de membresía activa duplicada'),
 (2,1,'[Datos inválidos] Solicitar corrección; no dar de alta'),
 (1,0,'[Datos inválidos] Pedir datos corregidos'),
 (2,2,'[Datos válidos] Asignar número de socio y beneficio'),
 (2,1,'[Datos válidos] Confirmar condición de miembro y descuento'),
 (1,0,'[Datos válidos] Comunicar adhesión y beneficio vigente')],
 'Responsable de Clientes es una responsabilidad organizacional que puede cumplir Ventas. No aceptar membresía no impide comprar. Marketing es opcional e independiente de la membresía. Datos inválidos vuelve a la validación; solo datos válidos generan alta.')
sequence('N02-06b','N02 · Secuencia: venta con beneficio',
 ['Cliente','Personal de Ventas','Responsable de Clientes','Caja / Entrega'],[
 (0,1,'Solicitar perfumes y asesoramiento'),
 (1,0,'Informar alternativas, precio y condiciones'),
 (1,1,'Verificar existencias y vigencia de lotes'),
 (1,0,'[Sin stock] Informar faltante; no concretar venta'),
 (1,0,'[Con stock] Consultar identificación o adhesión'),
 (0,1,'[Con stock] Identificarse o continuar como ocasional'),
 (1,2,'[Cliente identificado] Consultar membresía y beneficio'),
 (2,1,'[Cliente identificado] Confirmar beneficio activo o tarifa general'),
 (1,3,'[Con stock] Remitir detalle e importe final'),
 (3,0,'Informar total y solicitar pago'),
 (0,3,'Confirmar compra y efectuar pago'),
 (3,0,'Entregar perfumes por FEFO y comprobante'),
 (1,1,'Conservar venta, salida por lote e historial')],
 'Sin stock interrumpe. Cliente ocasional o sin membresía activa no obtiene descuento. Adhesión se detalla en N02-06a. Pago y entrega son actividades del negocio, no integración bancaria.')
sequence('N02-06c','N02 · Secuencia: coordinación de marketing',
 ['Cliente','Responsable de Clientes','Marketing'],[
 (1,0,'Consultar preferencias y autorización de contacto'),
 (0,1,'Aceptar o rechazar contacto comercial'),
 (1,1,'Conservar decisión de consentimiento'),
 (1,2,'[Autoriza] Aportar segmento y preferencias autorizadas'),
 (2,2,'[Autoriza] Preparar campaña acorde al segmento'),
 (2,0,'[Autoriza] Comunicar propuesta comercial'),
 (1,1,'[No autoriza] Excluir de comunicaciones comerciales')],
 'Diseño organizacional ya previsto por fidelización. La iteración 2 registra consentimiento y preferencias; no se afirma que exista un motor automático de campañas. Autoriza/No autoriza son excluyentes.')
sequence('N03-06','N03 · Secuencia: evaluación y selección de proveedores',
 ['Dueño / Compras','Proveedores','Contabilidad','Depósito'],[
 (0,1,'Solicitar condiciones para productos equivalentes'),
 (1,0,'Presentar precio, trayectoria, prestigio y certificados'),
 (0,2,'Solicitar antecedentes económicos y compras'),
 (2,0,'Informar costos, márgenes y cumplimiento económico'),
 (0,3,'Solicitar antecedentes de entregas y trazabilidad'),
 (3,0,'Informar calidad, diferencias, origen y rotación'),
 (0,0,'Validar comparabilidad y evidencia de las propuestas'),
 (0,1,'[Información incompleta] Pedir aclaraciones; diferir decisión'),
 (0,0,'[Completa] Ponderar precio, prestigio y antigüedad'),
 (0,2,'[Completa] Presentar recomendación y fundamentos'),
 (2,0,'Validar o rechazar conveniencia económica'),
 (0,0,'[No conveniente] Descartar propuesta y reevaluar alternativas'),
 (0,1,'[Conveniente] Priorizar proveedor y proponer acuerdo'),
 (1,0,'[Conveniente] Confirmar condiciones de suministro'),
 (0,0,'[Acuerdo] Conservar evaluación, prioridad y fundamentos')],
 'Diseño de N03, iteración 3. No implementado. El historial de compras y entregas aporta evidencia a los tres criterios, no agrega un cuarto peso implícito. Información incompleta difiere; rechazo vuelve a evaluar; acuerdo formaliza la selección.')
sequences=deepcopy(diagrams[start:])

# Global overview: names and relations, with implementation detail in the process views.
d=diagram('G07-01','G07 · Clases globales de perfumería',notes='Vista global de relaciones. Las vistas BE/BLL por proceso contienen atributos y operaciones. EvaluacionProveedor es diseño de N03. Seguridad y bitácora conservan las clases reales; no se inventa una FK desde EventoBitacora a Usuario.')
positions={
 'Proveedor':(30,30),'CotizacionProveedor':(360,30),'DetalleCotizacionProveedor':(690,30),
 'OrdenCompra':(30,190),'DetalleOrdenCompra':(360,190),'Producto':(690,190),
 'Usuario':(30,350),'Lote':(360,350),'EvaluacionProveedor':(690,350),
 'Componente':(30,510),'MovimientoStock':(360,510),'DetalleVenta':(690,510),
 'Familia':(30,670),'Patente':(30,800),'Venta':(360,670),'Cliente':(690,670),
 'Persona':(30,950),'EventoBitacora':(360,950),'Membresia':(690,850)}
for name,(x,y) in positions.items():
    node(d,name,name,x,y,275,72,stereotype='diseño N03' if name=='EvaluacionProveedor' else '',fontSize=13,
         abstract=name in ('Persona','Componente'))
for a,b,sa,sb in [
 ('Proveedor','CotizacionProveedor','1','0..*'),('CotizacionProveedor','DetalleCotizacionProveedor','1','1..*'),
 ('DetalleCotizacionProveedor','Producto','0..*','1'),('Proveedor','OrdenCompra','1','0..*'),
 ('OrdenCompra','DetalleOrdenCompra','1','1..*'),('DetalleOrdenCompra','Producto','0..*','1'),
 ('Producto','Lote','1','0..*'),('OrdenCompra','Lote','0..1','0..*'),('Proveedor','Lote','1','0..*'),
 ('Lote','MovimientoStock','1','0..*'),('Producto','EvaluacionProveedor','1','0..*'),
 ('Proveedor','EvaluacionProveedor','1','0..*'),('Venta','DetalleVenta','1','1..*'),
 ('DetalleVenta','Producto','0..*','1'),('DetalleVenta','Lote','0..*','1'),
 ('Cliente','Venta','0..1','0..*'),('Cliente','Membresia','1','0..*'),
 ('Usuario','Componente','0..*','0..*'),('Familia','Componente','0..*','0..*')]:
    link(d,a,b,sourceCardinality=sa,targetCardinality=sb)
for a,b in [('Usuario','Persona'),('Familia','Componente'),('Patente','Componente')]: link(d,a,b,kind='Generalization')
link(d,'Usuario','OrdenCompra','aprueba','Association',sourceCardinality='0..1',targetCardinality='0..*')
link(d,'Usuario','Venta','vende','Association',sourceCardinality='0..1',targetCardinality='0..*')
node(d,'note','',340,1100,625,110,'Note',notes='Membresías: historial 0..*, máximo una activa por cliente.\nEventoBitacora registra UsuarioEmail; no una referencia por Id.\nLas asociaciones BE por Id no implican propiedades de navegación.\nEvaluacionProveedor: diseño de la iteración 3.',fontSize=12)

# N03 design class detail complements, rather than replaces, the real N01/N02 code.
evalattrs=[('IdEvaluacion','int'),('IdProveedor','int'),('IdProducto','int'),('FechaEvaluacion','DateTime'),
 ('PuntajePrestigio','decimal'),('PuntajePrecio','decimal'),('PuntajeAntiguedad','decimal'),
 ('PesoPrestigio','decimal'),('PesoPrecio','decimal'),('PesoAntiguedad','decimal'),
 ('PuntajePonderadoFinal','decimal'),('Fundamentos','string'),('EsPrioritario','bool')]
d=diagram('N03-04','N03 · Clases de evaluación de proveedores',notes='Diseño de iteración 3. Conserva la evaluación del documento y explicita fecha, pesos y fundamentos para reproducir la decisión. No generado desde clases implementadas.')
node(d,'Proveedor','Proveedor',30,50,260,150,attributes=[dict(name=n,type=t) for n,t in [('IdProveedor','int'),('RazonSocial','string'),('AntiguedadMercado','int'),('IndicePrestigio','decimal')]],fontSize=12)
node(d,'Producto','Producto',720,50,240,150,attributes=[dict(name=n,type=t) for n,t in [('IdProducto','int'),('Codigo','string'),('Nombre','string')]],fontSize=12)
node(d,'EvaluacionProveedor','EvaluacionProveedor',330,290,360,410,stereotype='diseño N03',attributes=[dict(name=n,type=t) for n,t in evalattrs],fontSize=12)
for n in ('Proveedor','Producto'): link(d,n,'EvaluacionProveedor',sourceCardinality='1',targetCardinality='0..*')
node(d,'note','',30,760,930,135,'Note',notes='Pesos no negativos y suma igual a 1; puntajes normalizados con igual escala.\nPuntaje final = prestigio × peso + precio × peso + antigüedad × peso.\nUna sola evaluación prioritaria vigente por producto; se preserva el historial.\nLa evidencia de compras y entregas fundamenta los puntajes y la validación contable.',fontSize=12)

# Physical overview uses actual FK names; Bitacora has no FK to Usuarios.
globaltables=deepcopy(tables)
globaltables.update({
 'Usuarios':dict(attrs=[dict(name='Id',type='INT',stereotype='PK')],fks=[]),
 'Seguridad_Componente':dict(attrs=[dict(name='Id',type='INT',stereotype='PK')],fks=[]),
 'Seguridad_Usuario_Componente':dict(attrs=[dict(name=n,type='INT',stereotype='PK,FK') for n in ('IdUsuario','IdComponente')],fks=[('IdUsuario','Usuarios','Id'),('IdComponente','Seguridad_Componente','Id')]),
 'Seguridad_Familia_Hijo':dict(attrs=[dict(name=n,type='INT',stereotype='PK,FK') for n in ('IdPadre','IdHijo')],fks=[('IdPadre','Seguridad_Componente','Id'),('IdHijo','Seguridad_Componente','Id')]),
 'Bitacora':dict(attrs=[dict(name='Id',type='INT',stereotype='PK'),dict(name='Usuario',type='VARCHAR(150)')],fks=[]),
 'EvaluacionProveedor':dict(attrs=[dict(name=n,type='INT',stereotype='PK' if n=='IdEvaluacion' else 'FK') for n in ('IdEvaluacion','IdProveedor','IdProducto')],fks=[('IdProveedor','Proveedor','IdProveedor'),('IdProducto','Producto','IdProducto')])})
positions={
 'Proveedor':(20,20),'CotizacionProveedor':(430,20),'DetalleCotizacionProveedor':(840,20),
 'OrdenCompra':(20,230),'DetalleOrdenCompra':(430,230),'Producto':(840,230),
 'Usuarios':(20,470),'Lote':(430,470),'EvaluacionProveedor':(840,470),
 'Venta':(20,730),'DetalleVenta':(430,730),'MovimientoStock':(840,730),
 'Cliente':(20,970),'Membresia':(430,970),'Bitacora':(840,970),
 'Seguridad_Usuario_Componente':(20,1190),'Seguridad_Componente':(430,1190),'Seguridad_Familia_Hijo':(840,1190)}
d=diagram('G08-01','G08 · DER global de perfumería',notes='Vista global de claves y relaciones. Los atributos completos de negocio se encuentran en los DER por proceso. Las tablas de seguridad se contrastaron con metadatos de la base local. EvaluacionProveedor es diseño futuro, no tabla implementada.')
for name,(x,y) in positions.items():
    t=globaltables[name]
    attrs=[a for a in t['attrs'] if a.get('stereotype') or name=='Bitacora']
    node(d,name,name,x,y,340,90+len(attrs)*24,stereotype='diseño N03' if name=='EvaluacionProveedor' else 'table',attributes=attrs,fontSize=12)
    for col,target,_ in t['fks']:
        a=next(a for a in t['attrs'] if a['name']==col)
        link(d,name,target,col,stereotype='FK',sourceCardinality='0..*',targetCardinality='0..1' if a.get('notes')=='NULL' else '1')
node(d,'note','',20,1460,1160,120,'Note',notes='Vista de integración: atributos no clave en las vistas por proceso.\nMembresia: historial, con índice único filtrado para una activa por cliente.\nBitacora conserva el usuario como texto, sin FK a Usuarios.\nEvaluacionProveedor representa el diseño de N03, no una tabla instalada.',fontSize=13)

d=diagram('N03-07','N03 · DER de evaluación de proveedores',notes='Diseño de iteración 3. Relaciona propuestas evaluadas con producto y proveedor. No es DDL ejecutado.')
for name,x in [('Proveedor',20),('Producto',710)]:
    node(d,name,name,x,30,280,125,stereotype='table',attributes=globaltables[name]['attrs'][:3],fontSize=12)
attrs=[]
for n,t in evalattrs:
    sqltype={'int':'INT','DateTime':'DATETIME','decimal':'DECIMAL(18,4)','string':'NVARCHAR(MAX)','bool':'BIT'}[t]
    attrs.append(dict(name=n,type=sqltype,stereotype='PK' if n=='IdEvaluacion' else ('FK' if n in ('IdProveedor','IdProducto') else '')))
node(d,'EvaluacionProveedor','EvaluacionProveedor',315,270,380,440,stereotype='diseño N03',attributes=attrs,fontSize=12)
for n in ('Proveedor','Producto'): link(d,'EvaluacionProveedor',n,'Id'+n,stereotype='FK',sourceCardinality='0..*',targetCardinality='1')
node(d,'note','',20,780,975,110,'Note',notes='Restricciones de diseño: pesos >= 0 y suma = 1; puntajes entre 0 y 100.\nPrioridad única por producto cuando EsPrioritario = 1.\nConservar evaluaciones anteriores y los fundamentos de cada decisión.',fontSize=12)

new=[d for d in diagrams[start:] if d['code'] not in {'N01-06a','N01-06b','N02-06a','N02-06b'}]
(OUT/'model-reconciliation.json').write_text(json.dumps(new,ensure_ascii=False,indent=2),'utf-8')
(OUT/'sequences-reconciled.json').write_text(json.dumps(sequences[:4],ensure_ascii=False,indent=2),'utf-8')
print(f'{len(new)} new diagrams and 4 sequence updates')
