"""Build code-aligned UML views without adding fictitious behavior to data entities.

This is the final modeling layer after build_ea_reconciliation.py. N03 is
implemented; only MarketingBLL remains an explicitly identified design proposal.
"""
import json
import re
from copy import deepcopy
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Documentacion/EnterpriseArchitect"
base = json.loads((OUT / "model.json").read_text("utf-8-sig"))
reconciled = json.loads((OUT / "model-reconciliation.json").read_text("utf-8-sig"))
old = {d["code"]: d for d in base + reconciled}
registry = {}


def split_args(value):
    # Generic type commas are not argument separators.
    return re.split(r",\s*(?![^<>]*>)", value.strip()) if value.strip() else []


def parameters(value):
    result = []
    for value in split_args(value):
        declaration, _, default = value.partition("=")
        kind, name = declaration.strip().rsplit(None, 1)
        result.append(dict(name=name, type=kind.strip(), default=default.strip()))
    return result


def method(name, result="void", params="", **kwargs):
    return dict(name=name, type=result, parameters=parameters(params), visibility="Public", **kwargs)


def load_class(name, path, extra_methods=(), include_fields=False):
    source = (ROOT / path).read_text("utf-8-sig")
    # Several N03 transport classes share a file. Extract only the requested body.
    declaration = re.search(r"\b(?:class|interface)\s+" + re.escape(name) + r"\b[^\{]*\{", source)
    assert declaration, (name, path)
    start, depth, end = declaration.end(), 1, declaration.end()
    while depth:
        depth += (source[end] == "{") - (source[end] == "}")
        end += 1
    source = source[start:end-1]
    # A nested grid/view class is not a member API of the enclosing UserControl.
    while nested := re.search(r'\b(?:public|internal|private|protected)\s+(?:(?:sealed|abstract|static)\s+)*class\s+\w+[^\{]*\{', source):
        depth, end = 1, nested.end()
        while depth:
            depth += (source[end] == '{') - (source[end] == '}')
            end += 1
        source = source[:nested.start()] + source[end:]
    attrs = []
    for match in re.finditer(r"public\s+(?:(?:virtual|override|abstract|static)\s+)*([\w<>?,.\[\] ]+?)\s+(\w+)\s*\{\s*get\b", source):
        kind, prop = match.groups()
        attrs.append(dict(name=prop, type=kind, visibility="Public"))
    if include_fields:
        for match in re.finditer(r"private\s+(?:(?:readonly|static|const)\s+)*([\w<>?,.\[\]]+)\s+(\w+)\s*[;=]", source):
            kind, field = match.groups()
            attrs.append(dict(name=field, type=kind, visibility="Private"))
    methods = []
    pattern = r"\b(public|internal|private|protected)\s+((?:(?:static|abstract|virtual|override|sealed|async)\s+)*)([\w<>?,.\[\] ]+?)\s+(\w+)(?:<[^>]+>)?\s*\(([^)]*)\)"
    for access, modifiers, result, operation, args in re.findall(pattern, source):
        if access != "public" and operation not in extra_methods:
            continue
        methods.append(dict(name=operation, type=result.strip(), parameters=parameters(args),
                            visibility={"internal": "Package"}.get(access, access.title()),
                            static="static" in modifiers, abstract="abstract" in modifiers))
    # Constructors matter for initialized collections/defaults, not invented domain methods.
    if path.startswith("BE/"):
        for args in re.findall(r"public\s+" + re.escape(name) + r"\s*\(([^)]*)\)", source):
            methods.append(method(name, "", args, stereotype="create"))
    if "/Interfaces/" in path:
        methods = [method(operation, result.strip(), args) for result, operation, args in
                   re.findall(r"^\s*([\w<>?,.\[\] ]+?)\s+(\w+)\(([^)]*)\);", source, re.M)]
    registry[name] = dict(key=name, name=name, kind="Interface" if "/Interfaces/" in path else "Class",
                          stereotype="interface" if "/Interfaces/" in path else path.split("/")[0], attributes=attrs, methods=methods,
                          abstract=bool(re.search(r"abstract class " + re.escape(name), (ROOT / path).read_text('utf-8-sig'))),
                          notes=path, source=path, proposed=False,
                          namespace=path.rsplit("/", 1)[0].replace("/", "."))


entities = ["Proveedor", "Producto", "CotizacionProveedor", "DetalleCotizacionProveedor", "OrdenCompra",
            "DetalleOrdenCompra", "Lote", "MovimientoStock", "Cliente", "Membresia", "Venta", "DetalleVenta",
            "Persona", "Usuario", "EventoBitacora"]
for name in entities:
    load_class(name, f"BE/{name}.cs")
for name in ["Componente", "Familia", "Patente"]:
    load_class(name, f"BE/Seguridad/{name}.cs", include_fields=True)
for name in ["AprovisionamientoBLL", "InventarioBLL", "VentaBLL", "ProveedorBLL", "ProductoBLL", "RegistroActividadBitacora"]:
    extras = {"AprovisionamientoBLL": ["ValidarCotizacion", "ValidarOrdenPreliminar", "CambiarEstado"],
              "InventarioBLL": ["ValidarLote"], "VentaBLL": ["ValidarCliente"]}.get(name, [])
    load_class(name, f"BLL/{name}.cs", extras, include_fields=True)
for name in ["IAprovisionamientoRepository", "IInventarioRepository", "IProductoRepository", "IProveedorRepository", "IVentaRepository", "IRegistroActividad"]:
    folder = "BE" if name == "IRegistroActividad" else "DAL"
    load_class(name, f"{folder}/Interfaces/{name}.cs")
for name in ["AprovisionamientoDAL", "InventarioDAL", "VentaDAL", "ProductoDAL", "ProveedorDAL"]:
    load_class(name, f"DAL/{name}.cs", ["InsertarLote", "InsertarMovimiento", "InsertarVenta", "InsertarDetalle"])
for name in ["UC_Aprovisionamiento", "UC_Ventas"]:
    load_class(name, f"UI/UserControls/{name}.cs", ["RegistrarCotizacion", "CrearOrden", "EnviarRevision", "AprobarOrden", "RechazarOrden", "EmitirOrden", "RegistrarRecepcion", "ConsultarInventario", "RegistrarCliente", "BuscarCliente", "ConfirmarVenta", "Notificar", "MostrarError"])

for name in ['SolicitudEvaluacion', 'DatosEvaluacion', 'OfertaEvaluable', 'ResultadoProveedor', 'EvaluacionProveedor', 'ResumenEvaluacion']:
    load_class(name, 'BE/EvaluacionProveedor.cs')
load_class('EvaluacionProveedorBLL', 'BLL/EvaluacionProveedorBLL.cs', include_fields=True)
load_class('IEvaluacionProveedorRepository', 'DAL/Interfaces/IEvaluacionProveedorRepository.cs')
load_class('EvaluacionProveedorDAL', 'DAL/EvaluacionProveedorDAL.cs', ['LeerDatos', 'Leer', 'ComprobarVigencia'])
load_class('ArchivoEvaluacion', 'Servicios/ArchivoEvaluacion.cs')
load_class('UC_EvaluacionProveedores', 'UI/UserControls/UC_EvaluacionProveedores.cs', ['Comparar', 'Confirmar', 'CrearOrden'])
load_class('AprovisionamientoDAL', 'DAL/AprovisionamientoDAL.cs', ['InsertarOrden'])
registry["MarketingBLL"] = dict(key="MarketingBLL", name="MarketingBLL", kind="Class", stereotype="propuesto N02",
    proposed=True, attributes=[], notes="Diseño de campañas; no existe motor de envío implementado.", methods=[
        method("SeleccionarDestinatarios", "List<Cliente>", "List<Cliente> clientes"),
        method("PrepararCampania", "string", "List<Cliente> destinatarios"),
        method("EnviarCampania", "void", "List<Cliente> destinatarios, string mensaje")])

diagrams = []


def signature_width(c):
    lines = [a["name"] + ": " + a["type"] for a in c.get("attributes", [])]
    lines += [m["name"] + "(" + ", ".join(p["type"] for p in m["parameters"]) + "): " + m["type"] for m in c.get("methods", [])]
    return max(360, min(1100, 65 + max(map(len, lines), default=20) * 7))


def class_view(code, names, columns=3, links=None):
    original = old.get(code, dict(title='N03 · Clases de lógica y persistencia de evaluación'))
    d = dict(code=code, title=original["title"], kind="Logical", nodes=[], links=deepcopy(links if links is not None else original["links"]),
             notes="Atributos y operaciones compartidos con G07-01. Firmas verificadas en el código. Las clases propuestas están identificadas explícitamente. Las BE conservan sus constructores y propiedades; la lógica reside en BLL.")
    width = max(signature_width(registry[n]) for n in names)
    xgap, y = 100, 40
    for start in range(0, len(names), columns):
        row = names[start:start + columns]
        height = max((70 if code.startswith('N03') else 90) + 17 * (len(registry[n].get("attributes", [])) + len(registry[n].get("methods", []))) for n in row)
        for col, name in enumerate(row):
            n = deepcopy(registry[name])
            n.update(x=40 + col * (width + xgap), y=y, w=width, h=height, fontSize=11)
            d["nodes"].append(n)
        y += height + (35 if code.startswith('N03') else 90)
    d["links"] = [l for l in d["links"] if l["source"] in names and l["target"] in names]
    diagrams.append(d)
    return d


def uses(d, a, b):
    if a != b and not any(l["source"] == a and l["target"] == b for l in d["links"]):
        d["links"].append(dict(source=a, target=b, name="", kind="Dependency"))


details = {
    "N01-04a": ["Proveedor", "CotizacionProveedor", "DetalleCotizacionProveedor", "OrdenCompra", "DetalleOrdenCompra", "Producto", "ProveedorBLL", "AprovisionamientoBLL", "ProductoBLL"],
    "N01-04b": ["Producto", "Lote", "MovimientoStock", "OrdenCompra", "Proveedor", "InventarioBLL"],
    "N02-04": ["Cliente", "Membresia", "Venta", "DetalleVenta", "Producto", "Lote", "VentaBLL", "MarketingBLL"],
    "N01-05": ["AprovisionamientoBLL", "IAprovisionamientoRepository", "AprovisionamientoDAL", "InventarioBLL", "IInventarioRepository", "InventarioDAL", "IProveedorRepository", "ProveedorDAL", "IProductoRepository", "ProductoDAL", "IRegistroActividad", "RegistroActividadBitacora"],
    "N02-05": ["VentaBLL", "IVentaRepository", "VentaDAL", "IProductoRepository", "ProductoDAL", "IInventarioRepository", "InventarioDAL", "IRegistroActividad", "RegistroActividadBitacora", "MarketingBLL"],
    "N03-04": ["EvaluacionProveedor", "SolicitudEvaluacion", "DatosEvaluacion", "OfertaEvaluable", "ResultadoProveedor", "ResumenEvaluacion"],
    "N03-05": ["EvaluacionProveedorBLL", "IEvaluacionProveedorRepository", "EvaluacionProveedorDAL", "ArchivoEvaluacion"]}
for code, names in details.items():
    n03_links = [dict(source=a, target=b, kind='Association', name='', sourceCardinality='0..*', targetCardinality=card)
        for a,b,card in [('EvaluacionProveedor','SolicitudEvaluacion','1'),('EvaluacionProveedor','DatosEvaluacion','1'),
            ('EvaluacionProveedor','ResultadoProveedor','0..*'),('DatosEvaluacion','Producto','1'),('DatosEvaluacion','OfertaEvaluable','0..*'),
            ('SolicitudEvaluacion','Producto','1'),('OfertaEvaluable','Proveedor','1'),('ResultadoProveedor','Proveedor','1'),
            ('EvaluacionProveedor','Proveedor','0..1'),('EvaluacionProveedor','OrdenCompra','0..1')]]
    d = class_view(code, names, columns=2 if code == 'N03-04' else 1 if code == 'N03-05' else 3,
        links=n03_links if code == 'N03-04' else [] if code == 'N03-05' else None)
    deps = {"N01-04a": [("ProveedorBLL", "Proveedor"), ("AprovisionamientoBLL", "OrdenCompra"), ("AprovisionamientoBLL", "CotizacionProveedor"), ("ProductoBLL", "Producto")],
            "N01-04b": [("InventarioBLL", "Lote"), ("InventarioBLL", "MovimientoStock")],
            "N02-04": [("VentaBLL", "Cliente"), ("VentaBLL", "Venta"), ("VentaBLL", "Membresia"), ("MarketingBLL", "Cliente")],
            "N03-04": [],
            "N03-05": [("EvaluacionProveedorBLL", "IEvaluacionProveedorRepository"), ("EvaluacionProveedorBLL", "ArchivoEvaluacion"), ("EvaluacionProveedorDAL", "ArchivoEvaluacion")],
            "N02-05": [("MarketingBLL", "VentaBLL")]}.get(code, [])
    for a, b in deps:
        uses(d, a, b)
    for implementation, contract in [("AprovisionamientoDAL", "IAprovisionamientoRepository"),
            ("InventarioDAL", "IInventarioRepository"), ("ProductoDAL", "IProductoRepository"),
            ("ProveedorDAL", "IProveedorRepository"), ("VentaDAL", "IVentaRepository"),
            ("RegistroActividadBitacora", "IRegistroActividad"), ("EvaluacionProveedorDAL", "IEvaluacionProveedorRepository")]:
        if implementation in names and contract in names:
            d["links"].append(dict(source=implementation, target=contract, name="", kind="Realization"))

# Associations are undirected unless navigation is explicitly modeled. Reuse the
# global multiplicities in both orientations and avoid reversed duplicates.
association_definitions = {frozenset((l["source"], l["target"])): l
    for l in old["G07-01"]["links"] if l["kind"] == "Association" and 'EvaluacionProveedor' not in (l['source'],l['target'])}


def normalize_links(links):
    result, seen = [], set()
    for edge in links:
        if edge["kind"] == "Association":
            key = (edge["kind"], frozenset((edge["source"], edge["target"])))
            canonical = association_definitions.get(key[1])
            if canonical:
                edge = deepcopy(canonical)
        else:
            key = (edge["kind"], edge["source"], edge["target"])
        if key not in seen:
            result.append(edge)
            seen.add(key)
    return result


for d in diagrams:
    d["links"] = normalize_links(d["links"])

# The global view contains the same canonical members, not just class names.
global_names = list(dict.fromkeys([n for names in details.values() for n in names] + ["Persona", "Usuario", "Componente", "Familia", "Patente", "EventoBitacora", "RegistroActividadBitacora"]))
global_links = [deepcopy(l) for l in old["G07-01"]["links"] if 'EvaluacionProveedor' not in (l['source'],l['target'])]
for d in diagrams:
    for edge in d["links"]:
        if not any(e["source"] == edge["source"] and e["target"] == edge["target"] and e["kind"] == edge["kind"] for e in global_links):
            global_links.append(deepcopy(edge))
global_links.extend(deepcopy(n03_links))
global_links.extend(dict(source=a,target=b,kind='Dependency',name='') for a,b in [
    ('EvaluacionProveedorBLL','EvaluacionProveedor'),('EvaluacionProveedorDAL','AprovisionamientoDAL')])
class_view("G07-01", global_names, columns=5, links=normalize_links(global_links))


# Last represented nested call before returning from each non-leaf invocation.
# Indices refer to the original call list, not to the expanded UML message list.
SEQUENCE_ENDS = {
    "N01-06a": {1:3, 4:6, 7:11, 10:11, 12:14, 13:14, 15:17, 16:17, 18:21, 19:20},
    "N01-06b": {1:7, 4:6, 8:9, 10:11},
    "N02-06a": {1:6},
    "N02-06b": {1:11, 2:10, 6:9},
    "N02-06c": {1:2},
    "N03-06": {1:8,2:8,5:6,7:8,9:15,10:15,12:15,13:15,16:23,17:22,18:22,19:21},
}

# Actor requests and observable outcomes surround the existing code realization.
ACTOR_GROUPS = {
    'N01-06a': [(1,3,'Dueño / Departamento de Compras','Registrar cotización','Cotización registrada'),(4,6,'Dueño / Departamento de Compras','Preparar orden','OrdenCompra'),(7,11,'Dueño / Departamento de Compras','Enviar a revisión','Estado de la orden'),(12,14,'Equipo de Contabilidad','Rechazar compra','Estado de la orden'),(15,17,'Equipo de Contabilidad','Aprobar compra','Estado de la orden'),(18,21,'Dueño / Departamento de Compras','Emitir orden aprobada','Estado de la orden')],
    'N01-06b': [(1,7,'Recepción / Depósito','Confirmar recepción','Resultado de recepción'),(8,9,'Recepción / Depósito','Consultar existencias','int'),(10,11,'Recepción / Depósito','Consultar origen','Trazabilidad del lote')],
    'N02-06a': [(1,7,'Personal de Ventas','Registrar cliente y adhesión','Resultado del alta y membresía')],
    'N02-06b': [(1,11,'Personal de Ventas','Confirmar venta','Venta confirmada o motivo de rechazo')],
    'N02-06c': [(1,2,'Personal de Ventas','Registrar consentimiento','Resultado del registro'),(3,5,'Responsable de Marketing','Solicitar campaña autorizada','Resultado de campaña')],
    'N03-06': [(1,8,'Dueño / Departamento de Compras','Comparar proveedores','Comparación guardada / error'),
        (9,15,'Dueño / Departamento de Compras','Confirmar proveedor','Decisión fundamentada / error'),
        (16,23,'Dueño / Departamento de Compras','Crear orden en Borrador','Número de orden / error')]
}


def sequence(code, participants, calls, frames=(), notes=""):
    d = dict(code=code, title=old[code]["title"], kind="Sequence", nodes=[], links=[], frames=list(frames),
             notes="Realización en clases. Las llamadas usan nombres exactos de operaciones. Cada respuesta discontinua vuelve al emisor de la llamada e indica su tipo de retorno; void representa fin de ejecución sin valor. Los retornos corresponden al flujo normal; las excepciones se describen en las notas. " + notes)
    for i, name in enumerate(participants):
        d["nodes"].append(dict(key=name, name=name, kind="Object", classifier=name, x=220 + i * 310, y=50, w=255, h=90,
                               stereotype="propuesto" if registry[name].get("proposed") else ""))
    stack, starts, returns = [], {}, {}
    for i, (a, b, name) in enumerate(calls, 1):
        methods = [m for m in registry[b]["methods"] if m["name"] == name]
        types = {m["type"] for m in methods}
        assert len(types) == 1, (code, b, name, types)
        end = SEQUENCE_ENDS[code].get(i, i)
        assert i <= end <= len(calls)
        if stack:
            assert a == stack[-1]["target"] and end <= stack[-1]["end"], (code, i, stack)
        call = dict(source=a, target=b, name=name, kind="Sequence", sequence=len(d["links"])+1,
                    callId=i, isReturn=False, returnType=types.pop(),
                    proposed=bool(registry[b].get("proposed")))
        starts[i] = call["sequence"]
        d["links"].append(call)
        stack.append(dict(call, end=end))
        while stack and stack[-1]["end"] == i:
            completed = stack.pop()
            reply = dict(source=completed["target"], target=completed["source"],
                         name=completed["returnType"], kind="Sequence", sequence=len(d["links"])+1,
                         replyTo=completed["callId"], isReturn=True,
                         returnType=completed["returnType"], proposed=completed["proposed"])
            returns[completed["callId"]] = reply["sequence"]
            d["links"].append(reply)
    assert not stack and len(d["links"]) == 2 * len(calls)
    for f in d["frames"]:
        first, last = f["first"], f["last"]
        f["first"], f["last"] = starts[first], max(returns[i] for i in range(first, last+1))
        if len(f["guards"]) > 1:
            # N01 rejection ends before approval/emission starts.
            f["size"] = 35 * (starts[15] - starts[first])
    original = d['links']
    before, after = {}, {}
    actors = list(dict.fromkeys(g[2] for g in ACTOR_GROUPS[code]))
    for n in d['nodes']:
        n['x'] += 310 * len(actors)
    for i, actor in enumerate(actors):
        d['nodes'].append(dict(key=actor,name=actor,kind='Actor',x=80+i*310,y=35,w=270,h=100))
    for first,last,actor,request,outcome in ACTOR_GROUPS[code]:
        target = calls[first-1][0]
        key = 'actor-'+str(first)
        before[starts[first]] = dict(source=actor,target=target,name=request,kind='Sequence',callId=key,isReturn=False,returnType=outcome,actorInteraction=True)
        closing=max(returns[j] for j in range(first,last+1))
        after[closing] = dict(source=target,target=actor,name=outcome,kind='Sequence',replyTo=key,isReturn=True,returnType=outcome,actorInteraction=True)
    expanded, positions = [], {}
    for link in original:
        oldseq=link['sequence']
        if oldseq in before: expanded.append(before[oldseq])
        positions[oldseq] = len(expanded)+1
        expanded.append(link)
        if oldseq in after: expanded.append(after[oldseq])
    for i,link in enumerate(expanded,1): link['sequence']=i
    for f in d['frames']:
        first,last=f['first'],f['last']
        f['first']=positions[first]-(1 if first in before else 0)
        f['last']=positions[last]+(1 if last in after else 0)
        if len(f['guards'])>1: f['size']=35*(positions[starts[15]]-1-f['first'])
    d['links']=expanded
    nesting=[]
    for message in expanded:
        if message['isReturn']:
            invocation=nesting.pop()
            assert (message['replyTo'],message['source'],message['target']) == (invocation['callId'],invocation['target'],invocation['source']), (code,message,invocation)
        else:
            assert not nesting or message['source']==nesting[-1]['target'],(code,message,nesting)
            nesting.append(message)
    assert not nesting
    d['notes']+=' Los actores organizacionales inician las solicitudes y reciben el resultado visible; sus mensajes expresan acciones de usuario, no métodos C#. Las líneas de vida internas conservan clases, operaciones y retornos tipados.'
    diagrams.append(d)


U, A, D, R = "UC_Aprovisionamiento", "AprovisionamientoBLL", "AprovisionamientoDAL", "RegistroActividadBitacora"
sequence("N01-06a", [U, A, D, R], [
    (U,A,"RegistrarCotizacion"),(A,A,"ValidarCotizacion"),(A,D,"InsertarCotizacion"),
    (U,A,"CrearOrdenPreliminar"),(A,A,"ValidarOrdenPreliminar"),(A,D,"InsertarOrden"),
    (U,A,"EnviarOrdenARevisionContable"),(A,D,"ObtenerOrden"),(A,D,"ContarProveedoresCotizadosPorProducto"),
    (A,A,"CambiarEstado"),(A,D,"ActualizarEstado"),
    (U,A,"RechazarOrdenCompra"),(A,A,"CambiarEstado"),(A,D,"ActualizarEstado"),
    (U,A,"AprobarOrdenCompra"),(A,A,"CambiarEstado"),(A,D,"ActualizarEstado"),
    (U,A,"EmitirOrdenAprobada"),(A,A,"CambiarEstado"),(A,D,"ActualizarEstado"),(A,R,"Registrar")],
    [dict(kind=0, first=12, last=21, guards=["Rechazar", "Aprobar y emitir"], size=110)],
    "Se omiten consultas repetidas y registros de bitácora intermedios. Antes de cambiar a PendienteAprobacion se exigen dos proveedores cotizados por producto; insuficiencia lanza InvalidOperationException. La UI ejecuta cada operación según el actor autorizado; Aprobar/Rechazar son excluyentes.")
I = "InventarioBLL"
sequence("N01-06b", [U,A,D,I,"InventarioDAL",R], [
    (U,A,"RecepcionarMercaderia"),(A,D,"ObtenerOrden"),(A,I,"ValidarLote"),
    (A,D,"RegistrarRecepcion"),(D,"InventarioDAL","InsertarLote"),(D,"InventarioDAL","InsertarMovimiento"),
    (A,R,"Registrar"),(U,I,"VerificarStock"),(I,"InventarioDAL","VerificarStock"),
    (U,I,"ObtenerTrazabilidad"),(I,"InventarioDAL","ObtenerTrazabilidad")],
    [dict(kind=4, first=5, last=6, guards=["Por lote recibido"], size=70)],
    "Los controles de pertenencia y cantidad se realizan dentro de RecepcionarMercaderia. RegistrarRecepcion usa una transacción y actualiza el estado parcial/completo; no llama a RegistrarIngresoLote. ConsultarInventario permite verificar stock y trazabilidad después de la recepción.")
U,V,VD = "UC_Ventas", "VentaBLL", "VentaDAL"
sequence("N02-06a", [U,V,VD,R], [
    (U,V,"RegistrarCliente"),(V,V,"ValidarCliente"),(V,VD,"BuscarClientePorEmail"),(V,VD,"GuardarCliente"),
    (V,VD,"CrearMembresia"),(V,R,"Registrar"),(U,U,"Notificar")],
    [dict(kind=1, first=5, last=5, guards=["crearMembresia"], size=50)],
    "El consentimiento de marketing se conserva en Cliente.AutorizaMarketing. NombreCompleto es propiedad calculada, no método. Datos inválidos o correo duplicado producen excepción y la UI llama a Notificar(error.Message, true). El orden de llamadas refleja el código, no una transacción de alta inexistente.")
sequence("N02-06b", [U,V,VD,"ProductoDAL","InventarioDAL",R], [
    (U,U,"ConfirmarVenta"),(U,V,"RegistrarVenta"),(V,VD,"ObtenerMembresiaActiva"),
    (V,"ProductoDAL","Obtener"),(V,"InventarioDAL","ObtenerLotesDisponibles"),
    (V,VD,"RegistrarVenta"),(VD,VD,"InsertarVenta"),(VD,VD,"InsertarDetalle"),
    (VD,"InventarioDAL","InsertarMovimiento"),(V,R,"Registrar"),(U,U,"Notificar")],
    [dict(kind=1, first=3, last=3, guards=["IdCliente.HasValue"], size=50),
     dict(kind=4, first=4, last=5, guards=["Por producto"], size=70),
     dict(kind=4, first=8, last=9, guards=["Por detalle FEFO"], size=70)],
    "RegistrarVenta de BLL calcula FEFO, descuento y totales dentro del método. VentaDAL descuenta stock mediante SQL en su transacción, no invoca DescontarStockLote. Fallos de stock cancelan la persistencia; los métodos de UI notifican el error.")
sequence("N02-06c", [U,V,VD,"MarketingBLL"], [
    (U,V,"RegistrarCliente"),(V,VD,"GuardarCliente"),
    ("MarketingBLL","MarketingBLL","SeleccionarDestinatarios"),
    ("MarketingBLL","MarketingBLL","PrepararCampania"),("MarketingBLL","MarketingBLL","EnviarCampania")],
    [dict(kind=1, first=3, last=5, guards=["Solo autorizados"], size=105)],
    "RegistrarCliente/GuardarCliente existen y conservan el consentimiento. MarketingBLL y sus tres operaciones son una propuesta no implementada. La adquisición de la lista de clientes y el canal de envío quedan pendientes de implementación; no se dibuja una llamada inexistente a VentaDAL para obtener segmentos.")
E="EvaluacionProveedorBLL"
U,D,F,P='UC_EvaluacionProveedores','EvaluacionProveedorDAL','ArchivoEvaluacion','AprovisionamientoDAL'
sequence("N03-06", [U,E,D,F,P], [
    (U,U,'Comparar'),(U,E,'Evaluar'),(E,E,'ValidarEvaluacion'),(E,D,'ObtenerDatos'),(E,E,'Calcular'),(E,F,'Firma'),(E,D,'GuardarEvaluacion'),(D,F,'Serializar'),
    (U,U,'Confirmar'),(U,E,'SeleccionarPrioritario'),(E,E,'Obtener'),(E,D,'SeleccionarPrioritario'),(D,D,'ComprobarVigencia'),(D,D,'LeerDatos'),(D,F,'Firma'),
    (U,U,'CrearOrden'),(U,E,'CrearOrdenBorrador'),(E,D,'CrearOrdenBorrador'),(D,D,'ComprobarVigencia'),(D,D,'LeerDatos'),(D,F,'Firma'),(D,P,'InsertarOrden'),(U,E,'Obtener')],
    [dict(kind=1, first=19, last=22, guards=['No hay orden vinculada; datos vigentes'], size=280)], "Implementación N03: comparar y guardar evidencia, confirmar una oferta elegible con fundamento, crear un único borrador. Se muestran las colaboraciones principales; se omiten consultas auxiliares de refresco e historial. ObtenerDatos y las escrituras usan transacciones serializables. ComprobarVigencia rechaza cambios de datos o de fecha antes de escribir. Reintentar CrearOrdenBorrador devuelve el ID existente; no emite, aprueba ni recepciona la orden. Las respuestas void de UI representan finalización; las excepciones se comunican al actor mediante Ejecutar/Informar.")

# Verify repeated class representations are generated from one definition.
for d in diagrams:
    if d["kind"] != "Logical":
        continue
    for n in d["nodes"]:
        assert n.get("attributes", []) == registry[n["name"]].get("attributes", [])
        assert n.get("methods", []) == registry[n["name"]].get("methods", [])
(OUT / "code-design.json").write_text(json.dumps(diagrams, ensure_ascii=False, indent=2), "utf-8")
(OUT / "class-catalog.json").write_text(json.dumps(registry, ensure_ascii=False, indent=2), "utf-8")
print(f"{len(diagrams)} updated views; {len(registry)} canonical classes; all sequence targets verified")
