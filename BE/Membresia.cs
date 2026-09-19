using System;

namespace BE
{
    public class Membresia
    {
        public int IdMembresia { get; set; }
        public int IdCliente { get; set; }
        public string Numero { get; set; }
        public DateTime FechaAlta { get; set; }
        public decimal PorcentajeDescuento { get; set; }
        public bool Activa { get; set; }

        public Membresia()
        {
            FechaAlta = DateTime.Now;
            Activa = true;
        }
    }
}
