using System;

namespace BE
{
    public class Alerta
    {
        public string Mensaje { get; set; }
        public DateTime FechaHora { get; set; }
        public string Nivel { get; set; }
        public string UsuarioGenerador { get; set; }

        public Alerta()
        {
            FechaHora = DateTime.Now;
            Nivel = "INFO";
        }

        public Alerta(string mensaje)
        {
            Mensaje = mensaje;
            Nivel = "INFO";
            UsuarioGenerador = "Sistema";
            FechaHora = DateTime.Now;
        }

        public Alerta(string mensaje, string nivel, string usuario)
        {
            Mensaje = mensaje;
            Nivel = nivel;
            UsuarioGenerador = usuario;
            FechaHora = DateTime.Now;
        }
    }
}