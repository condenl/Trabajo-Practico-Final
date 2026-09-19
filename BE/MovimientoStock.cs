using System;

namespace BE
{
    public class MovimientoStock
    {
        public int IdMovimiento { get; set; }
        public int IdLote { get; set; }
        public TipoMovimientoStock Tipo { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string Referencia { get; set; }
        public string Responsable { get; set; }

        public MovimientoStock()
        {
            Fecha = DateTime.Now;
        }
    }
}
