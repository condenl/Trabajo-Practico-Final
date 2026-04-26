using System;

namespace BE
{
    public class EventoBitacora
    {
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        public string UsuarioEmail { get; set; }

        public string Actividad { get; set; }

        public string InformacionAsociada { get; set; }

        public EventoBitacora()
        {
            FechaHora = DateTime.Now;
        }

        public EventoBitacora(string usuario, string actividad, string informacion)
        {
            UsuarioEmail = usuario;
            Actividad = actividad;
            InformacionAsociada = informacion;
            FechaHora = DateTime.Now;
        }
    }
}