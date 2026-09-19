using System;
using System.Collections.Generic;

namespace BE
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int? IdCliente { get; set; }
        public int? IdUsuarioVendedor { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public List<DetalleVenta> Detalles { get; set; }

        public Venta()
        {
            Fecha = DateTime.Now;
            Detalles = new List<DetalleVenta>();
        }
    }
}
