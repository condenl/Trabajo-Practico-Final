using System;

namespace BE
{
    public class Lote
    {
        public int IdLote { get; set; }
        public int IdProducto { get; set; }
        public int IdProveedor { get; set; }
        public int? IdOrdenCompra { get; set; }
        public string CodigoLote { get; set; }
        public string Origen { get; set; }
        public string CertificadoOrigen { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public int CantidadIngresada { get; set; }
        public int CantidadDisponible { get; set; }

        public Lote()
        {
            FechaRecepcion = DateTime.Now;
        }
    }
}
