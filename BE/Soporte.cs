using System;

namespace BE.Soporte
{
    public class TipoProblema
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class EstadoSolicitud
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class SolicitudSoporte
    {
        public int Id { get; set; }
        public string EmailSolicitante { get; set; }
        public TipoProblema TipoProblema { get; set; }
        public string Descripcion { get; set; }
        public EstadoSolicitud Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string ObservacionesAdmin { get; set; }

        public SolicitudSoporte()
        {
            TipoProblema = new TipoProblema();
            Estado = new EstadoSolicitud();
        }
    }
}