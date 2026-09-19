using System;
using System.Collections.Generic;

namespace BE
{
    public class CotizacionProveedor
    {
        public int IdCotizacion { get; set; }
        public int IdProveedor { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVigencia { get; set; }
        public string Condiciones { get; set; }
        public List<DetalleCotizacionProveedor> Detalles { get; set; }

        public CotizacionProveedor()
        {
            FechaEmision = DateTime.Now;
            Detalles = new List<DetalleCotizacionProveedor>();
        }
    }
}
