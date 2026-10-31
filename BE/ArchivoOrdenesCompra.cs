using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BE
{
    // Transport contract: catalog business keys make files portable between databases.
    [XmlRoot("ArchivoOrdenesCompra")]
    public class ArchivoOrdenesCompra
    {
        [XmlAttribute("version")]
        public int Version { get; set; }
        public DateTime FechaExportacionUtc { get; set; }
        [XmlArray("Ordenes"), XmlArrayItem("Orden")]
        public List<OrdenCompraArchivo> Ordenes { get; set; } = new List<OrdenCompraArchivo>();
    }

    public class OrdenCompraArchivo
    {
        public int IdOrdenOrigen { get; set; }
        public string ProveedorCuit { get; set; }
        public string ProveedorNombre { get; set; }
        public DateTime FechaEmision { get; set; }
        public EstadoOrdenCompra Estado { get; set; }
        public decimal Total { get; set; }
        public string CreadaPor { get; set; }
        public int? IdUsuarioAprobadorContable { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string MotivoRechazo { get; set; }
        [XmlArray("Detalles"), XmlArrayItem("Detalle")]
        public List<DetalleOrdenArchivo> Detalles { get; set; } = new List<DetalleOrdenArchivo>();
    }

    public class DetalleOrdenArchivo
    {
        public string ProductoCodigo { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public int CantidadRecibida { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
