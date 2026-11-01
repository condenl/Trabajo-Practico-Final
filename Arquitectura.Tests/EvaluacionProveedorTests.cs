using BE;
using BE.Seguridad;
using BLL;
using DAL.Interfaces;
using Servicios;

namespace Arquitectura.Tests;

[TestClass]
public class EvaluacionProveedorTests
{
    private static readonly DateTime Hoy = new(2026, 11, 1);
    private static DatosEvaluacion Datos() => new() {
        FechaCorte = Hoy, Producto = new Producto { IdProducto = 1, Activo = true, PrecioVenta = 300, StockMinimo = 5 },
        StockDisponible = 5, ComprasPendientes = 10, UnidadesVendidas90Dias = 90, ImporteNetoVentas90Dias = 27000,
        Ofertas = new List<OfertaEvaluable> {
            new() { IdProveedor = 1, Proveedor = "Trayectoria", Activo = true, IdCotizacion = 1, FechaEmision = Hoy.AddDays(-1), FechaVigencia = Hoy,
                PrecioUnitario = 100, IndicePrestigio = 90, AntiguedadMercado = 20, PlazoEntregaDias = 10 },
            new() { IdProveedor = 2, Proveedor = "Económico", Activo = true, IdCotizacion = 2, FechaEmision = Hoy, FechaVigencia = Hoy.AddDays(10),
                PrecioUnitario = 95, IndicePrestigio = 20, AntiguedadMercado = 1, PlazoEntregaDias = 2 }
        }
    };
    private static EvaluacionProveedor Calcular(DatosEvaluacion d) => EvaluacionProveedorBLL.Calcular(new SolicitudEvaluacion { IdProducto = 1, Cantidad = 10 }, d, Hoy, "compras@test.example");

    [TestMethod] public void CruzaPrecioPrestigioAntiguedad_YNoEligeSoloElMasBarato()
    {
        var e = Calcular(Datos());
        Assert.AreEqual(1, e.IdProveedorRecomendado);
        Assert.AreEqual(94.5m, e.Resultados[0].PuntajePonderadoFinal);
        Assert.AreEqual(1000m, e.Resultados[0].CostoTotal);
        Assert.AreEqual(2000m, e.Resultados[0].BeneficioBrutoEstimado);
    }
    [TestMethod] public void PlanificacionDescuentaStockYComprasPendientesDeLaDemanda()
    { var e = Calcular(Datos()); Assert.AreEqual(15, e.CantidadSugerida); StringAssert.Contains(e.Resultados[0].Advertencias, "agotarse"); }
    [TestMethod] public void SinVentasUsaMinimoDeStockYSinInventarDemanda()
    { var d = Datos(); d.UnidadesVendidas90Dias = 0; d.ImporteNetoVentas90Dias = 0; Assert.AreEqual(0, Calcular(d).CantidadSugerida); }
    [TestMethod] public void UsaPrecioRealNetoDeDescuentosSiEsMenorQueCatalogo()
    { var d = Datos(); d.ImporteNetoVentas90Dias = 9000; var e = Calcular(d); Assert.AreEqual(100m, e.PrecioVentaReferencia); Assert.IsFalse(e.Resultados.Single(r => r.IdProveedor == 1).Elegible); Assert.AreEqual(2, e.IdProveedorRecomendado); }
    [DataRow("vencida")][DataRow("futura")][DataRow("inactivo")][DataRow("sinprecio")]
    [TestMethod] public void NoRecomiendaSinDosOfertasVigentesDeProveedoresActivos(string caso)
    {
        var d = Datos(); var o = d.Ofertas[1];
        if (caso == "vencida") o.FechaVigencia = Hoy.AddDays(-1);
        if (caso == "futura") o.FechaEmision = Hoy.AddDays(1);
        if (caso == "inactivo") o.Activo = false;
        if (caso == "sinprecio") o.PrecioUnitario = 0;
        Assert.IsNull(Calcular(d).IdProveedorRecomendado);
    }
    [TestMethod] public void SinMargenNoGeneraRecomendacion()
    { var d = Datos(); d.Producto.PrecioVenta = 50; Assert.IsNull(Calcular(d).IdProveedorRecomendado); }
    [TestMethod] public void AntiguedadSeSaturaEnVeinteAniosYEmpateEsDeterminista()
    {
        var d = Datos(); d.Ofertas[0].AntiguedadMercado = 200; var a = d.Ofertas[1]; a.IndicePrestigio = 90; a.AntiguedadMercado = 20; a.PrecioUnitario = 100;
        var e = Calcular(d); Assert.AreEqual(100m, e.Resultados[0].PuntajeAntiguedad); Assert.AreEqual(1, e.IdProveedorRecomendado);
    }
    [TestMethod] public void CambiarPesosPuedeCambiarLaDecision()
    { var e = EvaluacionProveedorBLL.Calcular(new SolicitudEvaluacion { IdProducto = 1, PesoPrecio = 100, PesoPrestigio = 0, PesoAntiguedad = 0 }, Datos(), Hoy, "test"); Assert.AreEqual(2, e.IdProveedorRecomendado); }
    [TestMethod] public void RechazaPesosInvalidosYCantidadCero()
    { Assert.Throws<ArgumentException>(() => EvaluacionProveedorBLL.ValidarEvaluacion(new SolicitudEvaluacion { IdProducto = 1, PesoPrecio = 80 })); Assert.Throws<ArgumentException>(() => EvaluacionProveedorBLL.ValidarEvaluacion(new SolicitudEvaluacion { IdProducto = 1, Cantidad = 0 })); }
    [TestMethod] public void ProductoInactivoSeRechaza()
    { var d = Datos(); d.Producto.Activo = false; Assert.Throws<InvalidOperationException>(() => Calcular(d)); }
    [TestMethod] public void SnapshotConservaEvidenciaYDetectaCambiosDeDatos()
    {
        var d = Datos(); var e = Calcular(d); var leido = ArchivoEvaluacion.Leer(ArchivoEvaluacion.Serializar(e));
        Assert.AreEqual(e.FirmaDatos, ArchivoEvaluacion.Firma(leido.Datos)); Assert.AreEqual(e.Resultados[0].PuntajePonderadoFinal, leido.Resultados[0].PuntajePonderadoFinal);
        d.Ofertas[0].PrecioUnitario++; Assert.AreNotEqual(e.FirmaDatos, ArchivoEvaluacion.Firma(d));
    }
    [TestMethod] public void BllExigeActorYFundamentoAntesDeGuardarDecision()
    {
        Usuario? actor = null; var repo = new Repo(); var bll = new EvaluacionProveedorBLL(repo, () => actor!, () => Hoy);
        Assert.Throws<UnauthorizedAccessException>(() => bll.Evaluar(new SolicitudEvaluacion { IdProducto = 1 }));
        actor = new Usuario { Email = "compras@test.example" }; actor.Permisos.Add(new Patente { PermisoAsociado = TipoPermiso.Aprovisionamiento_Gestion });
        var e = bll.Evaluar(new SolicitudEvaluacion { IdProducto = 1 }); Assert.AreEqual(7, e.IdEvaluacion);
        Assert.Throws<ArgumentException>(() => bll.SeleccionarPrioritario(7, 1, "breve")); Assert.IsFalse(repo.Seleccionada);
        bll.SeleccionarPrioritario(7, 2, "Se prioriza el precio por presupuesto disponible."); Assert.IsTrue(repo.Seleccionada);
    }
    private class Repo : IEvaluacionProveedorRepository
    {
        public EvaluacionProveedor Evaluacion = null!; public bool Seleccionada;
        public DatosEvaluacion ObtenerDatos(int id, DateTime fecha) => Datos();
        public int GuardarEvaluacion(EvaluacionProveedor e) { Evaluacion = e; return 7; }
        public EvaluacionProveedor ObtenerEvaluacion(int id) => Evaluacion;
        public IList<ResumenEvaluacion> ListarHistorial(int id) => new List<ResumenEvaluacion>();
        public void SeleccionarPrioritario(int id, int proveedor, string motivo, string actor) { Seleccionada = true; }
        public int CrearOrdenBorrador(int id, string actor) => 99;
    }
}
