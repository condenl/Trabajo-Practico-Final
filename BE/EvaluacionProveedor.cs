using System;
using System.Collections.Generic;

namespace BE
{
    public class SolicitudEvaluacion
    {
        public int IdProducto { get; set; }
        public int Cantidad { get; set; } = 10;
        public decimal PesoPrecio { get; set; } = 50;
        public decimal PesoPrestigio { get; set; } = 30;
        public decimal PesoAntiguedad { get; set; } = 20;
    }

    public class DatosEvaluacion
    {
        public DateTime FechaCorte { get; set; }
        public Producto Producto { get; set; }
        public int StockDisponible { get; set; }
        public int ComprasPendientes { get; set; }
        public int UnidadesVendidas90Dias { get; set; }
        public decimal ImporteNetoVentas90Dias { get; set; }
        public List<OfertaEvaluable> Ofertas { get; set; } = new List<OfertaEvaluable>();
    }

    public class OfertaEvaluable
    {
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; }
        public bool Activo { get; set; }
        public int AntiguedadMercado { get; set; }
        public decimal IndicePrestigio { get; set; }
        public int? IdCotizacion { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaVigencia { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int PlazoEntregaDias { get; set; }
        public int UnidadesCompradas180Dias { get; set; }
        public int UnidadesRecibidas180Dias { get; set; }
        public decimal ImporteComprado180Dias { get; set; }
    }

    public class ResultadoProveedor
    {
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; }
        public int? IdCotizacion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int PlazoEntregaDias { get; set; }
        public decimal CostoTotal { get; set; }
        public decimal MargenEstimado { get; set; }
        public decimal BeneficioBrutoEstimado { get; set; }
        public decimal PuntajePrecio { get; set; }
        public decimal PuntajePrestigio { get; set; }
        public decimal PuntajeAntiguedad { get; set; }
        public decimal PuntajePonderadoFinal { get; set; }
        public bool Elegible { get; set; }
        public string Fundamentos { get; set; }
        public string Advertencias { get; set; }
        public int UnidadesCompradas180Dias { get; set; }
        public int UnidadesRecibidas180Dias { get; set; }
    }

    public class EvaluacionProveedor
    {
        public int IdEvaluacion { get; set; }
        public DateTime FechaEvaluacion { get; set; }
        public string Responsable { get; set; }
        public SolicitudEvaluacion Solicitud { get; set; }
        public DatosEvaluacion Datos { get; set; }
        public string FirmaDatos { get; set; }
        public int CantidadSugerida { get; set; }
        public decimal PrecioVentaReferencia { get; set; }
        public int? IdProveedorRecomendado { get; set; }
        public int? IdProveedorSeleccionado { get; set; }
        public string MotivoSeleccion { get; set; }
        public string SeleccionadaPor { get; set; }
        public DateTime? FechaSeleccion { get; set; }
        public int? IdOrdenCompra { get; set; }
        public List<ResultadoProveedor> Resultados { get; set; } = new List<ResultadoProveedor>();
    }

    public class ResumenEvaluacion
    {
        public int IdEvaluacion { get; set; }
        public DateTime Fecha { get; set; }
        public string Responsable { get; set; }
        public string ProveedorSeleccionado { get; set; }
        public int? IdOrdenCompra { get; set; }
    }
}
