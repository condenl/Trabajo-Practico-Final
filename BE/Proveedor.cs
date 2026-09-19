namespace BE
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string Contacto { get; set; }
        public int AntiguedadMercado { get; set; }
        public decimal IndicePrestigio { get; set; }
        public bool Activo { get; set; }

        public Proveedor()
        {
            Activo = true;
        }
    }
}
