using BE;
using BE.Interfaces;
using Servicios;

namespace BLL
{
    public sealed class RegistroActividadBitacora : IRegistroActividad
    {
        public void Registrar(string actividad, string detalle)
        {
            string responsable = Session.Instance.Usuario != null
                ? Session.Instance.Usuario.Email
                : "SISTEMA";

            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, actividad, detalle));
        }
    }
}
