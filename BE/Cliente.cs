using System;

namespace BE
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string PreferenciasOlfativas { get; set; }
        public bool AutorizaMarketing { get; set; }
        public DateTime FechaAlta { get; set; }
        public bool Activo { get; set; }

        public Cliente()
        {
            FechaAlta = DateTime.Now;
            Activo = true;
        }

        public string NombreCompleto
        {
            get { return (Apellido + ", " + Nombre).Trim(' ', ','); }
        }
    }
}
