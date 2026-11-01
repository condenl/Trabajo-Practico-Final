using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BE;
using BE.Seguridad;
using DAL;
using DAL.Interfaces;
using Servicios;

namespace BLL
{
    public class EvaluacionProveedorBLL
    {
        private readonly IEvaluacionProveedorRepository repositorio;
        private readonly Func<Usuario> obtenerUsuario;
        private readonly Func<DateTime> reloj;
        public EvaluacionProveedorBLL() : this(EvaluacionProveedorDAL.Instancia, () => Session.Instance.Usuario, () => DateTime.Now) { }
        public EvaluacionProveedorBLL(IEvaluacionProveedorRepository repositorio, Func<Usuario> obtenerUsuario, Func<DateTime> reloj)
        { this.repositorio = repositorio ?? throw new ArgumentNullException("repositorio"); this.obtenerUsuario = obtenerUsuario ?? throw new ArgumentNullException("obtenerUsuario"); this.reloj = reloj ?? throw new ArgumentNullException("reloj"); }

        private Usuario ExigirActor()
        {
            var actor = obtenerUsuario();
            if (actor == null || !actor.Activo || !actor.TienePermiso(TipoPermiso.Aprovisionamiento_Gestion))
                throw new UnauthorizedAccessException("La evaluación requiere una sesión de Compras o Dueño.");
            if (string.IsNullOrWhiteSpace(actor.Email) || actor.Email.Length > 200) throw new InvalidOperationException("La cuenta debe identificar al responsable de la evaluación.");
            return actor;
        }
        public EvaluacionProveedor Evaluar(SolicitudEvaluacion solicitud)
        {
            var actor = ExigirActor();
            ValidarEvaluacion(solicitud);
            var datos = repositorio.ObtenerDatos(solicitud.IdProducto, reloj().Date);
            var resultado = Calcular(solicitud, datos, reloj(), actor.Email);
            resultado.IdEvaluacion = repositorio.GuardarEvaluacion(resultado);
            return resultado;
        }
        public EvaluacionProveedor Obtener(int id) { ExigirActor(); return repositorio.ObtenerEvaluacion(id) ?? throw new InvalidOperationException("No existe la evaluación."); }
        public IList<ResumenEvaluacion> ConsultarHistorial(int idProducto) { ExigirActor(); return repositorio.ListarHistorial(idProducto); }
        public void SeleccionarPrioritario(int id, int idProveedor, string motivo)
        {
            var actor = ExigirActor();
            var e = Obtener(id);
            if (!e.Resultados.Any(r => r.IdProveedor == idProveedor && r.Elegible)) throw new InvalidOperationException("Seleccione una oferta elegible.");
            if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 10 || motivo.Length > 500)
                throw new ArgumentException("Indique un fundamento de entre 10 y 500 caracteres para conservar la decisión.");
            repositorio.SeleccionarPrioritario(id, idProveedor, motivo.Trim(), actor.Email);
        }
        public int CrearOrdenBorrador(int id) { return repositorio.CrearOrdenBorrador(id, ExigirActor().Email); }

        public static void ValidarEvaluacion(SolicitudEvaluacion s)
        {
            if (s == null || s.IdProducto <= 0 || s.Cantidad < 1 || s.Cantidad > 100000)
                throw new ArgumentException("Seleccione un producto y una cantidad de 1 a 100000.");
            if (s.PesoPrecio < 0 || s.PesoPrestigio < 0 || s.PesoAntiguedad < 0 || s.PesoPrecio + s.PesoPrestigio + s.PesoAntiguedad != 100)
                throw new ArgumentException("Los pesos deben ser no negativos y sumar 100 %.");
        }
        public static EvaluacionProveedor Calcular(SolicitudEvaluacion s, DatosEvaluacion datos, DateTime fecha, string responsable)
        {
            ValidarEvaluacion(s);
            if (datos == null || datos.Producto == null || !datos.Producto.Activo || datos.Producto.IdProducto != s.IdProducto)
                throw new InvalidOperationException("El producto no existe o está inactivo.");
            var e = new EvaluacionProveedor { Solicitud = s, Datos = datos, FechaEvaluacion = fecha, Responsable = responsable, FirmaDatos = ArchivoEvaluacion.Firma(datos) };
            decimal promedioVenta = datos.UnidadesVendidas90Dias > 0 ? datos.ImporteNetoVentas90Dias / datos.UnidadesVendidas90Dias : datos.Producto.PrecioVenta;
            e.PrecioVentaReferencia = Math.Min(datos.Producto.PrecioVenta, promedioVenta);
            decimal diaria = datos.UnidadesVendidas90Dias / 90m;
            e.CantidadSugerida = (int)Math.Min(100000m, Math.Max(0m, Math.Max(datos.Producto.StockMinimo, decimal.Ceiling(diaria * 30)) - datos.StockDisponible - datos.ComprasPendientes));
            var vigentes = datos.Ofertas.Where(o => o.Activo && o.IdCotizacion.HasValue && o.PrecioUnitario > 0 && o.FechaEmision <= datos.FechaCorte && o.FechaVigencia >= datos.FechaCorte).ToList();
            decimal minimo = vigentes.Count > 0 ? vigentes.Min(o => o.PrecioUnitario) : 0;
            foreach (var o in datos.Ofertas)
            {
                bool vigente = vigentes.Contains(o);
                var r = new ResultadoProveedor { IdProveedor = o.IdProveedor, Proveedor = o.Proveedor, IdCotizacion = o.IdCotizacion,
                    PrecioUnitario = o.PrecioUnitario, PlazoEntregaDias = o.PlazoEntregaDias, CostoTotal = o.PrecioUnitario * s.Cantidad,
                    PuntajePrecio = vigente ? minimo / o.PrecioUnitario * 100 : 0, PuntajePrestigio = o.IndicePrestigio,
                    PuntajeAntiguedad = Math.Min(o.AntiguedadMercado, 20) * 5m, UnidadesCompradas180Dias = o.UnidadesCompradas180Dias,
                    UnidadesRecibidas180Dias = o.UnidadesRecibidas180Dias };
                r.MargenEstimado = e.PrecioVentaReferencia > 0 ? (e.PrecioVentaReferencia - o.PrecioUnitario) / e.PrecioVentaReferencia * 100 : 0;
                r.BeneficioBrutoEstimado = (e.PrecioVentaReferencia - o.PrecioUnitario) * s.Cantidad;
                r.Elegible = vigente && vigentes.Count >= 2 && r.BeneficioBrutoEstimado > 0 && r.CostoTotal <= 9999999999999999.99m;
                r.Fundamentos = !o.Activo ? "Proveedor inactivo" : !vigente ? "Sin cotización vigente para este producto" : vigentes.Count < 2 ? "Se necesitan dos proveedores con cotizaciones vigentes" : r.BeneficioBrutoEstimado <= 0 ? "Sin margen bruto positivo" : "Oferta comparable y con margen bruto positivo";
                if (r.CostoTotal > 9999999999999999.99m) r.Fundamentos = "El importe supera el máximo admitido por una orden de compra";
                r.Advertencias = o.UnidadesCompradas180Dias == 0 ? "Sin antecedentes de compra en 180 días. " : "Recepción acumulada: " + o.UnidadesRecibidas180Dias + "/" + o.UnidadesCompradas180Dias + " unidades (180 días). ";
                if (diaria > 0 && datos.StockDisponible / diaria < o.PlazoEntregaDias) r.Advertencias += "El stock actual podría agotarse antes del plazo cotizado. ";
                if (e.CantidadSugerida == 0) r.Advertencias += "Stock y compras pendientes cubren el objetivo de reposición. ";
                r.PuntajePonderadoFinal = decimal.Round((r.PuntajePrecio * s.PesoPrecio + r.PuntajePrestigio * s.PesoPrestigio + r.PuntajeAntiguedad * s.PesoAntiguedad) / 100, 4);
                e.Resultados.Add(r);
            }
            e.Resultados = e.Resultados.OrderByDescending(r => r.Elegible).ThenByDescending(r => r.PuntajePonderadoFinal).ThenBy(r => r.PrecioUnitario).ThenBy(r => r.IdProveedor).ToList();
            e.IdProveedorRecomendado = e.Resultados.Where(r => r.Elegible).Select(r => (int?)r.IdProveedor).FirstOrDefault();
            return e;
        }
    }
}
