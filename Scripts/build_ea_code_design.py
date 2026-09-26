"""Build code-aligned UML views without adding fictitious behavior to data entities.

This is the final modeling layer after build_ea_reconciliation.py. N03 and
MarketingBLL are explicit design proposals, never represented as existing C#.
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
    attrs = []
    for match in re.finditer(r"public\s+(?:(?:virtual|override|abstract|static)\s+)*([\w<>?,.\[\] ]+?)\s+(\w+)\s*\{\s*get\b", source):
        kind, prop = match.groups()
        attrs.append(dict(name=prop, type=kind, visibility="Public"))
    if include_fields:
        for match in re.finditer(r"private\s+(?:(?:readonly|static|const)\s+)*([\w<>?,.\[\]]+)\s+(\w+)\s*[;=]", source):
            kind, field = match.groups()
            attrs.append(dict(name=field, type=kind, visibility="Private"))
    methods = []
    pattern = r"\b(public|internal|private|protected)\s+((?:(?:static|abstract|virtual|override|sealed|async)\s+)*)([\w<>?,.\[\] ]+?)\s+(\w+)\s*\(([^)]*)\)"
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
    if path.startswith("BE/Interfaces/"):
        methods = [method(operation, result.strip(), args) for result, operation, args in
                   re.findall(r"^\s*([\w<>?,.\[\] ]+?)\s+(\w+)\(([^)]*)\);", source, re.M)]
    registry[name] = dict(key=name, name=name, kind="Interface" if path.startswith("BE/Interfaces/") else "Class",
                          stereotype=path.split("/")[0], attributes=attrs, methods=methods,
                          abstract=bool(re.search(r"abstract class " + re.escape(name), source)),
                          notes=path, source=path, proposed=False)


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
    load_class(name, f"BE/Interfaces/{name}.cs")
for name in ["AprovisionamientoDAL", "InventarioDAL", "VentaDAL", "ProductoDAL", "ProveedorDAL"]:
    load_class(name, f"DAL/{name}.cs", ["InsertarLote", "InsertarMovimiento", "InsertarVenta", "InsertarDetalle"])
for name in ["UC_Aprovisionamiento", "UC_Ventas"]:
    load_class(name, f"UI/UserControls/{name}.cs", ["RegistrarCotizacion", "CrearOrden", "EnviarRevision", "AprobarOrden", "RechazarOrden", "EmitirOrden", "RegistrarRecepcion", "ConsultarInventario", "RegistrarCliente", "BuscarCliente", "ConfirmarVenta", "Notificar", "MostrarError"])

evaluation = deepcopy(next(n for n in old["N03-04"]["nodes"] if n["name"] == "EvaluacionProveedor"))
evaluation.update(proposed=True, notes="Diseño N03: clase y operación propuestas, todavía sin implementación C#.",
                  methods=[method("CalcularPuntajePonderado", "decimal")])
registry["EvaluacionProveedor"] = evaluation
registry["EvaluacionProveedorBLL"] = dict(key="EvaluacionProveedorBLL", name="EvaluacionProveedorBLL", kind="Class",
    stereotype="propuesto N03", proposed=True, attributes=[], notes="Diseño N03; no existe todavía en BLL.", methods=[
        method("Evaluar", "EvaluacionProveedor", "EvaluacionProveedor evaluacion"),
        method("ValidarEvaluacion", "void", "EvaluacionProveedor evaluacion"),
        method("SeleccionarPrioritario", "EvaluacionProveedor", "List<EvaluacionProveedor> evaluaciones"),
        method("GuardarEvaluacion", "int", "EvaluacionProveedor evaluacion")])
registry["MarketingBLL"] = dict(key="MarketingBLL", name="MarketingBLL", kind="Class", stereotype="propuesto N02",
    proposed=True, attributes=[], notes="Diseño de campañas; no existe motor de envío implementado.", methods=[
        method("SeleccionarDestinatarios", "List<Cliente>", "List<Cliente> clientes"),
        method("PrepararCampania", "string", "List<Cliente> destinatarios"),
        method("EnviarCampania", "void", "List<Cliente> destinatarios, string mensaje")])

diagrams = []


def signature_width(c):
    lines = [a["name"] + ": " + a["type"] for a in c.get("attributes", [])]
    lines += [m["name"] + "(" + ", ".join(p["name"] + ": " + p["type"] for p in m["parameters"]) + "): " + m["type"] for m in c.get("methods", [])]
    return max(340, min(1250, 60 + max(map(len, lines), default=20) * 6))


def class_view(code, names, columns=3, links=None):
    original = old[code]
    d = dict(code=code, title=original["title"], kind="Logical", nodes=[], links=deepcopy(links if links is not None else original["links"]),
             notes="Atributos y operaciones compartidos con G07-01. Firmas verificadas en el código. Las clases propuestas están identificadas explícitamente. Las BE conservan sus constructores y propiedades; la lógica reside en BLL.")
    width = max(signature_width(registry[n]) for n in names)
    xgap, y = 170, 40
    for start in range(0, len(names), columns):
        row = names[start:start + columns]
        height = max(110 + 22 * (len(registry[n].get("attributes", [])) + len(registry[n].get("methods", []))) for n in row)
        for col, name in enumerate(row):
            n = deepcopy(registry[name])
            n.update(x=40 + col * (width + xgap), y=y, w=width, h=height, fontSize=11)
            d["nodes"].append(n)
        y += height + 150
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
    "N01-05": ["AprovisionamientoBLL", "IAprovisionamientoRepository", "InventarioBLL", "IInventarioRepository", "IProveedorRepository", "IProductoRepository", "IRegistroActividad"],
    "N02-05": ["VentaBLL", "IVentaRepository", "IProductoRepository", "IInventarioRepository", "IRegistroActividad", "MarketingBLL"],
    "N03-04": ["Proveedor", "Producto", "EvaluacionProveedor", "EvaluacionProveedorBLL", "AprovisionamientoBLL", "InventarioBLL"]}
for code, names in details.items():
    d = class_view(code, names, columns=2 if code.endswith("-05") else 3)
    deps = {"N01-04a": [("ProveedorBLL", "Proveedor"), ("AprovisionamientoBLL", "OrdenCompra"), ("AprovisionamientoBLL", "CotizacionProveedor"), ("ProductoBLL", "Producto")],
            "N01-04b": [("InventarioBLL", "Lote"), ("InventarioBLL", "MovimientoStock")],
            "N02-04": [("VentaBLL", "Cliente"), ("VentaBLL", "Venta"), ("VentaBLL", "Membresia"), ("MarketingBLL", "Cliente")],
            "N03-04": [("EvaluacionProveedorBLL", "EvaluacionProveedor"), ("EvaluacionProveedorBLL", "AprovisionamientoBLL"), ("EvaluacionProveedorBLL", "InventarioBLL")],
            "N02-05": [("MarketingBLL", "VentaBLL")]}.get(code, [])
    for a, b in deps:
        uses(d, a, b)

# The global view contains the same canonical members, not just class names.
global_names = list(dict.fromkeys([n for names in details.values() for n in names] + ["Persona", "Usuario", "Componente", "Familia", "Patente", "EventoBitacora", "RegistroActividadBitacora"]))
global_links = deepcopy(old["G07-01"]["links"])
for d in diagrams:
    for edge in d["links"]:
        if not any(e["source"] == edge["source"] and e["target"] == edge["target"] and e["kind"] == edge["kind"] for e in global_links):
            global_links.append(deepcopy(edge))
class_view("G07-01", global_names, columns=5, links=global_links)


def sequence(code, participants, calls, frames=(), notes=""):
    d = dict(code=code, title=old[code]["title"], kind="Sequence", nodes=[], links=[], frames=list(frames),
             notes="Realización en clases. Los mensajes son nombres exactos de operaciones; las firmas completas están en los diagramas de clases. " + notes)
    for i, name in enumerate(participants):
        d["nodes"].append(dict(key=name, name=name, kind="Object", classifier=name, x=220 + i * 310, y=50, w=255, h=90,
                               stereotype="propuesto" if registry[name].get("proposed") else ""))
    for i, (a, b, name) in enumerate(calls):
        assert any(m["name"] == name for m in registry[b]["methods"]), (code, b, name)
        d["links"].append(dict(source=a, target=b, name=name, kind="Sequence", sequence=i + 1,
                               proposed=bool(registry[b].get("proposed"))))
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
sequence("N03-06", [E,"ProveedorBLL","ProductoBLL",A,I,"EvaluacionProveedor"], [
    (E,E,"Evaluar"),(E,"ProveedorBLL","Obtener"),(E,"ProductoBLL","Obtener"),
    (E,A,"ConsultarHistorialCompras"),(E,I,"ObtenerTrazabilidad"),(E,E,"ValidarEvaluacion"),
    (E,"EvaluacionProveedor","CalcularPuntajePonderado"),(E,E,"SeleccionarPrioritario"),(E,E,"GuardarEvaluacion")],
    [], "Propuesta N03: EvaluacionProveedorBLL y EvaluacionProveedor no existen aún en C#. Obtener, ConsultarHistorialCompras y ObtenerTrazabilidad sí existen en las BLL consultadas. La conveniencia y los fundamentos se validan antes de priorizar; GuardarEvaluacion requiere persistencia futura.")

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
