using System.Globalization;
using System.Xml.Serialization;
using BE;
using BE.Seguridad;
using BLL;
using PdfSharp.Pdf.IO;
using Servicios;
using UI.Reportes;

namespace Arquitectura.Tests;

[TestClass]
public class IntercambioOrdenesTests
{
    [TestMethod]
    public void Ejemplo_TresOrdenesImportablesConCatalogoActualSinGuardarCompras()
    {
        using var f = new Fixture();
        f.Proveedores.Datos[11] = new Proveedor { IdProveedor = 11, Cuit = "00000000001", Activo = false };
        f.Productos.Datos[21] = new Producto { IdProducto = 21, Codigo = "INACTIVO", Activo = false };
        var ejemplo = f.Bll.CrearEjemplo();
        var xml = OrdenesXml.Serializar(ejemplo);
        File.WriteAllText(f.Xml, xml, new System.Text.UTF8Encoding(false));
        var copia = OrdenesXml.Leer(f.Xml);
        Assert.HasCount(3, copia.Ordenes);
        Assert.AreEqual(3, f.Bll.ValidarImportacion(copia));
        Assert.IsTrue(copia.Ordenes.All(o => o.ProveedorCuit == "99000000001" &&
            o.Estado == EstadoOrdenCompra.Borrador && o.Detalles.Single().ProductoCodigo == "PERF-ÁMBAR"));
        Assert.AreEqual(3, copia.Ordenes.Select(o => o.IdOrdenOrigen).Distinct().Count());
        StringAssert.Contains(xml, "encoding=\"utf-8\"");
        StringAssert.Contains(xml, "&amp;");
        Assert.HasCount(1, f.Repo.Ordenes);
        Assert.IsEmpty(f.Registro.Actividades);
    }

    [TestMethod]
    public void Ejemplo_RequiereActorYCatalogosActivos()
    {
        using var f = new Fixture();
        f.Actor = null;
        Assert.Throws<UnauthorizedAccessException>(() => f.Bll.CrearEjemplo());
        f.Actor = new Usuario { Email = "compras@example.test" };
        f.Actor.Permisos.Add(new Patente { PermisoAsociado = TipoPermiso.Aprovisionamiento_Gestion });
        f.Proveedores.Datos[10].Activo = false;
        Assert.Throws<InvalidOperationException>(() => f.Bll.CrearEjemplo());
        f.Proveedores.Datos[10].Activo = true;
        f.Productos.Datos[20].Activo = false;
        Assert.Throws<InvalidOperationException>(() => f.Bll.CrearEjemplo());
    }

    [TestMethod]
    public void XmlSerializer_Roundtrip_ConservaMetadatosYDetalles()
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        OrdenesXml.Guardar(f.Xml, archivo);
        var copia = OrdenesXml.Leer(f.Xml);
        Assert.AreEqual(Serializar(archivo), Serializar(copia));
        StringAssert.Contains(File.ReadAllText(f.Xml), "<ArchivoOrdenesCompra version=\"1\">");
        Assert.AreEqual("Perfume ámbar & rosas", copia.Ordenes[0].Detalles[0].ProductoNombre);
        Assert.AreEqual(EstadoOrdenCompra.RecibidaParcial, copia.Ordenes[0].Estado);
    }

    [TestMethod]
    public void Xml_DecimalesIndependientesDeLaCultura()
    {
        using var f = new Fixture();
        var anterior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-AR");
            OrdenesXml.Guardar(f.Xml, f.Bll.ObtenerListado(new[] { 1 }));
            StringAssert.Contains(File.ReadAllText(f.Xml), "<PrecioUnitario>1234.56</PrecioUnitario>");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.AreEqual(1234.56m, OrdenesXml.Leer(f.Xml).Ordenes[0].Detalles[0].PrecioUnitario);
        }
        finally { CultureInfo.CurrentCulture = anterior; }
    }

    [DataRow("<otro />")]
    [DataRow("<ArchivoOrdenesCompra version=\"1\">")]
    [DataRow("<!DOCTYPE ArchivoOrdenesCompra [<!ENTITY xxe SYSTEM 'file:///C:/Windows/win.ini'>]><ArchivoOrdenesCompra version=\"1\">&xxe;</ArchivoOrdenesCompra>")]
    [TestMethod]
    public void Xml_RechazaFormatoAjenoMalformadoYEntidadesExternas(string xml)
    {
        using var f = new Fixture();
        File.WriteAllText(f.Xml, xml);
        Assert.Throws<InvalidDataException>(() => OrdenesXml.Leer(f.Xml));
    }

    [TestMethod]
    public void Xml_RechazaNodosDesconocidosYSegundaRaiz()
    {
        using var f = new Fixture();
        OrdenesXml.Guardar(f.Xml, f.Bll.ObtenerListado(new[] { 1 }));
        string xml = File.ReadAllText(f.Xml);
        File.WriteAllText(f.Xml, xml.Replace("</ArchivoOrdenesCompra>", "<Inyectado>1</Inyectado></ArchivoOrdenesCompra>"));
        Assert.Throws<InvalidDataException>(() => OrdenesXml.Leer(f.Xml));
        File.WriteAllText(f.Xml, xml + "<otraRaiz />");
        Assert.Throws<InvalidDataException>(() => OrdenesXml.Leer(f.Xml));
    }

    [TestMethod]
    public void Xml_RechazaVersionNoSoportadaYListasVacias()
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        archivo.Version = 2;
        Assert.Throws<InvalidDataException>(() => OrdenesXml.Guardar(f.Xml, archivo));
        archivo.Version = 1; archivo.Ordenes.Clear();
        Assert.Throws<InvalidDataException>(() => f.Bll.Importar(archivo));
        Assert.Throws<ArgumentException>(() => f.Bll.ObtenerListado(Array.Empty<int>()));
    }

    [TestMethod]
    public void Xml_RechazaArchivosDemasiadoGrandes()
    {
        using var f = new Fixture();
        using (var stream = File.Create(f.Xml)) stream.SetLength(OrdenesXml.MaximoBytes + 1);
        Assert.Throws<InvalidDataException>(() => OrdenesXml.Leer(f.Xml));
    }

    [TestMethod]
    public void Xml_TotalInvalido_NoSobrescribeArchivoExistente()
    {
        using var f = new Fixture();
        File.WriteAllText(f.Xml, "CONSERVAR");
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        archivo.Ordenes[0].Total++;
        Assert.Throws<InvalidDataException>(() => OrdenesXml.Guardar(f.Xml, archivo));
        Assert.AreEqual("CONSERVAR", File.ReadAllText(f.Xml));
    }

    [TestMethod]
    public void Xml_CaracterInvalido_RevierteArchivoTemporal()
    {
        using var f = new Fixture();
        File.WriteAllText(f.Xml, "CONSERVAR");
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        archivo.Ordenes[0].ProveedorNombre = "Invalido\u0001";
        Assert.Throws<InvalidOperationException>(() => OrdenesXml.Guardar(f.Xml, archivo));
        Assert.AreEqual("CONSERVAR", File.ReadAllText(f.Xml));
        Assert.HasCount(1, Directory.GetFiles(f.Carpeta));
    }

    [TestMethod]
    public void Importar_UsaClavesDeCatalogoYNoConcedeAprobacionNiStock()
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        var ids = f.Bll.Importar(archivo);
        var nueva = f.Repo.Ordenes[ids.Single()];
        Assert.AreNotEqual(1, nueva.IdOrdenCompra);
        Assert.AreEqual(10, nueva.IdProveedor);
        Assert.AreEqual(20, nueva.Detalles[0].IdProducto);
        Assert.AreEqual(EstadoOrdenCompra.Borrador, nueva.Estado);
        Assert.IsNull(nueva.IdUsuarioAprobadorContable);
        Assert.IsNull(nueva.FechaAprobacion);
        Assert.IsNull(nueva.MotivoRechazo);
        Assert.AreEqual(0, nueva.Detalles[0].CantidadRecibida);
        Assert.AreEqual("compras@example.test", nueva.CreadaPor);
        Assert.AreEqual(DateTime.Today, nueva.FechaEmision.Date);
        Assert.AreEqual(2469.12m, nueva.Total);
        Assert.AreEqual(1, archivo.Ordenes[0].Detalles[0].CantidadRecibida, "La deserialización de origen debe conservarse intacta.");
        Assert.HasCount(1, f.Registro.Actividades);
    }

    [TestMethod]
    public void Importar_UnDatoInvalido_NoInsertaElRestoDelLote()
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        var otra = f.Bll.ObtenerListado(new[] { 1 }).Ordenes[0];
        otra.IdOrdenOrigen = 2;
        otra.Detalles[0].ProductoCodigo = "NO-EXISTE";
        archivo.Ordenes.Add(otra);
        Assert.Throws<InvalidOperationException>(() => f.Bll.Importar(archivo));
        Assert.HasCount(1, f.Repo.Ordenes);
        Assert.IsEmpty(f.Registro.Actividades);
    }

    [TestMethod]
    public void Importar_RevalidaCatalogoDespuesDeLaVistaPrevia()
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        Assert.AreEqual(1, f.Bll.ValidarImportacion(archivo));
        f.Productos.Datos[20].Activo = false;
        Assert.Throws<InvalidOperationException>(() => f.Bll.Importar(archivo));
        Assert.HasCount(1, f.Repo.Ordenes);
    }

    [TestMethod]
    public void Intercambio_ExigeActorActivoConPermiso()
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        f.Actor = null;
        Assert.Throws<UnauthorizedAccessException>(() => f.Bll.ObtenerListado(new[] { 1 }));
        Assert.Throws<UnauthorizedAccessException>(() => f.Bll.Importar(archivo));
        f.Actor = new Usuario { Email = "contable@example.test" };
        f.Actor.Permisos.Add(new Patente { PermisoAsociado = TipoPermiso.Contabilidad_AprobarCompras });
        Assert.Throws<UnauthorizedAccessException>(() => f.Bll.Importar(archivo));
    }

    [DataRow(0, 1, 1234.56, false)]
    [DataRow(2, 3, 1234.56, false)]
    [DataRow(2, 1, -1.0, false)]
    [DataRow(2, 1, 1.123, false)]
    [TestMethod]
    public void Xml_ValidaCantidadesYPrecios(int cantidad, int recibida, double precio, bool _)
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        var detalle = archivo.Ordenes[0].Detalles[0];
        detalle.Cantidad = cantidad; detalle.CantidadRecibida = recibida; detalle.PrecioUnitario = (decimal)precio;
        Assert.Throws<InvalidDataException>(() => f.Bll.Importar(archivo));
    }

    [TestMethod]
    public void ReportePdf_GeneraArchivoValidoConPaginacion()
    {
        using var f = new Fixture();
        var archivo = f.Bll.ObtenerListado(new[] { 1 });
        archivo.Ordenes[0].Detalles[0].ProductoCodigo = new string('X', 50);
        archivo.Ordenes[0].Detalles[0].ProductoNombre = new string('A', 150);
        for (int i = 0; i < 90; i++) archivo.Ordenes[0].Detalles.Add(new DetalleOrdenArchivo {
            ProductoCodigo = "PERF-" + i, ProductoNombre = "Descripción extensa de perfume ámbar y rosas de prueba " + i,
            Cantidad = 2, PrecioUnitario = 100m });
        archivo.Ordenes[0].Total = archivo.Ordenes[0].Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        string ruta = Path.Combine(f.Carpeta, "reporte.pdf");
        ReporteOrdenesPdf.Guardar(ruta, archivo);
        using var pdf = PdfReader.Open(ruta, PdfDocumentOpenMode.Import);
        Assert.IsGreaterThanOrEqualTo(3, pdf.PageCount);
    }

    private static string Serializar(ArchivoOrdenesCompra archivo)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        new XmlSerializer(typeof(ArchivoOrdenesCompra)).Serialize(writer, archivo);
        return writer.ToString();
    }

    private sealed class Fixture : IDisposable
    {
        public readonly string Carpeta = Path.Combine(Path.GetTempPath(), "PerfumeriaXmlTests-" + Guid.NewGuid().ToString("N"));
        public string Xml => Path.Combine(Carpeta, "ordenes.xml");
        public readonly FakeAprovisionamientoRepository Repo = new();
        public readonly FakeProveedorRepository Proveedores = new();
        public readonly FakeProductoRepository Productos = new();
        public readonly FakeRegistroActividad Registro = new();
        public Usuario? Actor;
        public readonly IntercambioOrdenesBLL Bll;
        public Fixture()
        {
            Directory.CreateDirectory(Carpeta);
            Actor = new Usuario { Email = "compras@example.test" };
            Actor.Permisos.Add(new Patente { PermisoAsociado = TipoPermiso.Aprovisionamiento_Gestion });
            Proveedores.Datos[10] = new Proveedor { IdProveedor = 10, Cuit = "99000000001", RazonSocial = "Proveedor & Cía.", Activo = true };
            Productos.Datos[20] = new Producto { IdProducto = 20, Codigo = "PERF-ÁMBAR", Nombre = "Perfume ámbar & rosas", Activo = true };
            Repo.InsertarOrden(new OrdenCompra { IdProveedor = 10, CreadaPor = "origen@example.test", Estado = EstadoOrdenCompra.RecibidaParcial,
                FechaEmision = new DateTime(2026, 10, 1, 10, 20, 30), Total = 2469.12m, IdUsuarioAprobadorContable = 99,
                FechaAprobacion = new DateTime(2026, 10, 2), MotivoRechazo = "Texto histórico",
                Detalles = new List<DetalleOrdenCompra> { new() { IdProducto = 20, Cantidad = 2, CantidadRecibida = 1, PrecioUnitario = 1234.56m } } });
            Bll = new IntercambioOrdenesBLL(Repo, Proveedores, Productos, Registro, () => Actor!);
        }
        public void Dispose() { Directory.Delete(Carpeta, true); }
    }
}
