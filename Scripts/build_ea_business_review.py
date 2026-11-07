"""Synchronize the business views of N02/N03 without rebuilding unrelated EA views."""
import json
from pathlib import Path

OUT = Path(__file__).resolve().parents[1] / 'Documentacion/EnterpriseArchitect'


def replace_views(path, replacements, source=None):
    # Preserve the original serialization of every unrelated diagram.
    source = path.read_text('utf-8-sig') if source is None else source
    decoder, cursor, spans = json.JSONDecoder(), source.index('[') + 1, []
    while True:
        while source[cursor].isspace() or source[cursor] == ',':
            cursor += 1
        if source[cursor] == ']':
            break
        item, end = decoder.raw_decode(source, cursor)
        if item['code'] in replacements:
            indent = ' ' * (cursor - source.rfind('\n', 0, cursor) - 1)
            replacement = json.dumps(replacements[item['code']], ensure_ascii=False, indent=4).replace('\n', '\n' + indent)
            spans.append((cursor, end, replacement))
        cursor = end
    assert len(spans) == len(replacements)
    for start, end, replacement in reversed(spans):
        source = source[:start] + replacement + source[end:]
    path.write_text(source, encoding='utf-8')


def build():
    views = []

    def view(code, title, kind, notes):
        d = dict(code=code, title=title, kind=kind, notes=notes, nodes=[], links=[])
        views.append(d)
        return d

    def node(d, key, name, x, y, w=270, h=65, kind='Activity', **kwargs):
        d['nodes'].append(dict(key=key, name=name, x=x, y=y, w=w, h=h, kind=kind, fontSize=12, **kwargs))

    def edge(d, a, b, name='', kind='ControlFlow', **kwargs):
        d['links'].append(dict(source=a, target=b, name=name, kind=kind, **kwargs))

    d = view('N02-02', 'N02 · Proceso de venta y fidelización', 'Activity',
             'Actividades organizacionales de venta y fidelización. Las alternativas se reúnen mediante nodos merge, no joins implícitos. '
             'Membresía vigente es el objeto crucial producido al recuperar una membresía existente o al crearla, y consumido al aplicar el beneficio. '
             'La autorización de marketing es independiente de la adhesión. Las campañas siguen siendo diseño propuesto y no forman parte de esta venta.')
    for i, (name, x, w) in enumerate([('Cliente',20,300), ('Personal de Ventas',335,410), ('Caja / Entrega',760,320)]):
        node(d, f'lane{i}', name, x, 25, w, 1540, 'ActivityPartition')
    node(d,'start','',150,70,30,30,'StateNode',subtype=100)
    node(d,'request','Elegir perfumes y cantidades',55,130,235)
    node(d,'stock','Revisar existencias utilizables y vencimientos',400,130)
    node(d,'enough','¿Hay stock?',500,235,75,55,'Decision')
    node(d,'nostock','Conocer el faltante y desistir de la compra',55,235,235)
    node(d,'endstock','',150,335,30,30,'StateNode',subtype=101)
    node(d,'identify','Identificar al cliente y consultar su membresía',400,345)
    node(d,'member','¿Membresía vigente?',500,455,75,55,'Decision')
    node(d,'join','¿Desea adherirse?',135,550,75,55,'Decision')
    node(d,'data','Brindar datos y autorización de marketing',55,650,235)
    node(d,'register','Registrar cliente y asignar membresía',365,650,205)
    node(d,'existing','Recuperar beneficio vigente',585,550,140)
    node(d,'membership','Membresía vigente',565,750,155,45,'Object')
    node(d,'benefit','Aplicar descuento de membresía',525,850,195)
    node(d,'general','Mantener precio general',365,850,140)
    node(d,'merge','',505,965,30,30,'Decision',notes='Merge de alternativas excluyentes: precio general o beneficio de membresía. No sincroniza ramas.')
    node(d,'total','Informar subtotal, descuento y total',800,1030,245)
    node(d,'pay','Confirmar compra y efectuar pago',55,1130,235)
    node(d,'collect','Confirmar cobro',800,1230,245)
    node(d,'record','Registrar venta y descontar stock por lote (FEFO)',400,1330)
    node(d,'deliver','Entregar los perfumes y comprobante',800,1430,245)
    node(d,'endok','',905,1520,30,30,'StateNode',subtype=101)
    node(d,'note','',55,1320,265,125,'Note',notes='La membresía puede otorgar descuento aunque el cliente no autorice marketing.\nLa venta a un cliente ocasional conserva el precio general.')
    for a,b,label in [('start','request',''),('request','stock',''),('stock','enough',''),('enough','nostock','[No]'),
        ('nostock','endstock',''),('enough','identify','[Sí]'),('identify','member',''),('member','existing','[Sí]'),
        ('member','join','[No]'),('join','data','[Sí]'),('data','register',''),('join','general','[No]'),
        ('benefit','merge',''),('general','merge',''),('merge','total',''),('total','pay',''),('pay','collect',''),
        ('collect','record',''),('record','deliver',''),('deliver','endok','')]:
        edge(d,a,b,label)
    edge(d,'existing','membership',kind='ObjectFlow')
    edge(d,'register','membership',kind='ObjectFlow')
    edge(d,'membership','benefit',kind='ObjectFlow')
    next(l for l in d['links'] if l['source']=='join' and l['target']=='general')['path']='330:-577;330:-882;'

    d = view('N03-01', 'N03 · Casos de uso de evaluación de proveedores', 'Use Case',
        'Descomposición del CU018 en objetivos concretos del Dueño / Departamento de Compras. '
        'Comparar cruza fuentes del negocio y valora ofertas; confirmar e iniciar compra comprueban la vigencia de la comparación. '
        'La compra requiere una selección fundamentada previa. Consultar historial es independiente. '
        'Registrar cotizaciones y aprobar órdenes pertenecen a N01; no se formalizan acuerdos comerciales en N03.')
    node(d,'scope','Evaluación y selección de proveedores',245,30,990,990,'Boundary')
    node(d,'owner','Dueño / Departamento de Compras',25,415,190,120,'Actor')
    cases=[
        ('compare','CU018.1\nComparar proveedores para un producto y cantidad',285,70,315,125),
        ('select','CU018.2\nConfirmar proveedor con fundamento',285,390,315,125),
        ('order','CU018.3\nIniciar una orden de compra en Borrador',285,650,315,125),
        ('history','CU018.4\nConsultar historial y fundamentos',285,865,315,105),
        ('sources','Cruzar stock, ventas y compras pendientes',835,70,330,125),
        ('value','Valorar precio, prestigio, antigüedad y margen',835,250,330,125),
        ('current','Verificar vigencia de la comparación',835,565,330,125),
    ]
    for key,name,x,y,w,h in cases:
        node(d,key,name,x,y,w,h,'UseCase',notes={
            'compare':'N03-A / CU018.1. Producto, cantidad y pesos; comparación fechada con ranking, exclusiones, margen, antecedentes y reposición sugerida. Se conserva incluso sin ofertas elegibles.',
            'select':'N03-B / CU018.2. Comparación con oferta elegible; elegir proveedor y conservar motivo de 10 a 500 caracteres, fecha y responsable. La decisión confirmada no se sobrescribe.',
            'order':'N03-C / CU018.3. Proveedor previamente confirmado. Crear un único borrador vinculado a la evaluación; reintentar devuelve la misma orden. Continúa por N01, sin aprobación ni ingreso de stock automático.',
            'history':'Consultar las comparaciones del producto y recuperar sus resultados y decisiones. No obliga a generar una nueva comparación.',
            'sources':'Inventario utilizable, ventas netas de los últimos 90 días, órdenes pendientes y antecedentes de compras/recepciones de 180 días.',
            'value':'Al menos dos cotizaciones vigentes de proveedores distintos. Puntaje ponderado y margen bruto estimado positivo. Explicar exclusiones y advertencias; el responsable puede elegir otra oferta elegible.',
            'current':'Antes de confirmar o crear un borrador, comprobar que no cambió la fecha ni la información considerada; de cambiar, solicitar una nueva comparación.'
        }[key])
    for target in ['compare','select','order','history']:
        edge(d,'owner',target,kind='Association')
    for a,b in [('compare','sources'),('compare','value'),('select','current'),('order','current')]:
        edge(d,a,b,kind='Dependency',stereotype='include')
    node(d,'note','',720,800,450,155,'Note',notes='Confirmar requiere una oferta elegible.\nCrear el borrador requiere una decisión confirmada y es opcional.\nLa aprobación contable y la recepción continúan por N01.')

    d=view('N03-02','N03 · Actividad de evaluación y selección de proveedores','Activity',
        'Proceso organizacional implementado: reunir propuestas, cruzar información del negocio, comparar, elegir con fundamento e iniciar opcionalmente un borrador. '
        'El responsable puede elegir cualquier oferta elegible. Sin dos cotizaciones vigentes o sin margen positivo se conserva la comparación sin recomendación. '
        'Si cambian los datos se repite la comparación; las tareas de aprobación y recepción pertenecen a N01.')
    node(d,'lane0','Dueño / Departamento de Compras',20,25,630,1520,'ActivityPartition')
    node(d,'lane1','Proveedores',665,25,325,1520,'ActivityPartition')
    node(d,'start','',255,70,30,30,'StateNode',subtype=100)
    node(d,'need','Definir producto, cantidad y criterios de evaluación',100,130,340)
    node(d,'quotes','Aportar precios, plazos, vigencia y trayectoria',700,130,260)
    node(d,'offers','Propuestas comerciales',725,240,210,45,'Object')
    node(d,'sources','Reunir stock utilizable, ventas y compras pendientes',100,370,340)
    node(d,'compare','Comparar ofertas y conservar ranking fechado con fundamentos',100,455,340,75)
    node(d,'ranking','Comparación de proveedores',100,550,340,45,'Object')
    node(d,'review','Revisar alternativas, antecedentes y reposición sugerida',100,625,340)
    node(d,'eligible','¿Oferta elegible?',230,725,80,55,'Decision')
    node(d,'defer','Diferir selección y solicitar nuevas cotizaciones',455,725,170,80)
    node(d,'enddefer','',525,850,30,30,'StateNode',subtype=101)
    node(d,'select','Elegir oferta y fundamentar la decisión',100,825,340)
    node(d,'current','¿Datos vigentes?',230,925,80,55,'Decision')
    node(d,'refresh','Actualizar antecedentes y repetir comparación',455,925,170,80)
    node(d,'decision','Conservar elección, motivo y responsable',100,1025,340)
    node(d,'buy','¿Iniciar compra?',230,1120,80,55,'Decision')
    node(d,'enddecision','',525,1220,30,30,'StateNode',subtype=101)
    node(d,'draft','Revalidar datos y confeccionar borrador vinculado a la evaluación',100,1220,340,65,notes='Si cambió la información, se vuelve a comparar sin crear una orden parcial. Reintentar devuelve el borrador ya vinculado.')
    node(d,'order','Orden de compra [Borrador]',100,1310,340,45,'Object')
    node(d,'continue','Continuar circuito de compras y aprobación (N01)',100,1390,340)
    node(d,'endok','',255,1490,30,30,'StateNode',subtype=101)
    node(d,'note','',700,350,260,275,'Note',notes='Comparar precio, prestigio y antigüedad; estimar margen y plazo.\nCruzar ventas de 90 días y compras de 180 días.\nOfertas elegibles: al menos dos proveedores con cotización vigente y margen bruto positivo.\nLa aprobación contable continúa en N01.')
    for a,b,label in [('start','need',''),('need','quotes',''),('sources','compare',''),
        ('review','eligible',''),('eligible','defer','[No]'),('defer','enddefer',''),('eligible','select','[Sí]'),
        ('select','current',''),('current','refresh','[No]'),('refresh','sources',''),('current','decision','[Sí]'),
        ('decision','buy',''),('buy','enddecision','[No]'),('buy','draft','[Sí]') ,('continue','endok','')]:
        edge(d,a,b,label)
    for a,b in [('quotes','offers'),('offers','sources'),('compare','ranking'),('ranking','review'),('draft','order'),('order','continue')]:
        edge(d,a,b,kind='ObjectFlow')
    next(l for l in d['links'] if l['source']=='refresh')['path']='640:-965;640:-340;'
    # Re-entry is an alternative to initial collection, so an explicit merge is required.
    node(d,'merge','',255,330,30,20,'Decision',notes='Merge de la primera recopilación y la actualización de antecedentes; no es una sincronización.')
    next(l for l in d['links'] if l['source']=='offers')['target']='merge'
    # Keep object and control flows separate: the object is consumed before the merge.
    node(d,'collect','Reunir propuestas y antecedentes comerciales',100,240,340)
    next(l for l in d['links'] if l['source']=='offers')['target']='collect'
    edge(d,'collect','merge')
    next(l for l in d['links'] if l['source']=='refresh')['target']='merge'
    edge(d,'merge','sources')

    for filename,codes in [('model.json',{'N02-02'}),('model-n03.json',{'N03-01','N03-02'})]:
        path=OUT/filename
        prior=json.loads(path.read_text('utf-8-sig'))
        replacements={v['code']:v for v in views if v['code'] in codes}
        assert codes.issubset({v['code'] for v in prior})
        replace_views(path,replacements)
    (OUT/'business-review.json').write_text(json.dumps(views,ensure_ascii=False,indent=2),'utf-8')
    print('Business views updated: '+', '.join(v['code'] for v in views))


if __name__=='__main__':
    build()
