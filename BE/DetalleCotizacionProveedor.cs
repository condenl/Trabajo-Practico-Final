namespace BE
{
    public class DetalleCotizacionProveedor
    {
        public int IdDetalleCotizacion { get; set; }
        public int IdCotizacion { get; set; }
        public int IdProducto { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int PlazoEntregaDias { get; set; }
    }
}
