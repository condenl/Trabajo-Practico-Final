using System;
using System.Collections.Generic;

namespace BE
{
    public class OrdenCompra
    {
        public int IdOrdenCompra { get; set; }
        public int IdProveedor { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
        public EstadoOrdenCompra Estado { get; set; }
        public int? IdUsuarioAprobadorContable { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public string MotivoRechazo { get; set; }
        public string CreadaPor { get; set; }
        public List<DetalleOrdenCompra> Detalles { get; set; }

        public OrdenCompra()
        {
            FechaEmision = DateTime.Now;
            Estado = EstadoOrdenCompra.Borrador;
            Detalles = new List<DetalleOrdenCompra>();
        }
    }
}
