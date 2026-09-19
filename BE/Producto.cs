namespace BE
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public string FamiliaOlfativa { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockMinimo { get; set; }
        public bool Activo { get; set; }

        public Producto()
        {
            Activo = true;
        }
    }
}
