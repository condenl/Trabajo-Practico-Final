namespace BE
{
    public class DetalleOrdenCompra
    {
        public int IdDetalle { get; set; }
        public int IdOrdenCompra { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public int CantidadRecibida { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal
        {
            get { return Cantidad * PrecioUnitario; }
        }
    }
}
