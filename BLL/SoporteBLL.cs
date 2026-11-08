using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BE;
using BE.Soporte;
using BE.Seguridad;
using DAL;
using Servicios;

namespace BLL
{
    public class SoporteBLL
    {
        public void CrearSolicitud(SolicitudSoporte solicitud)
        {
            ValidarEntradas(solicitud);

            SoporteDAL.Instancia.Insertar(solicitud);
            GestorEventos.Instancia.Notificar(new EventoBitacora(
                solicitud.EmailSolicitante,
                "SOLICITUD_SOPORTE",
                $"El usuario generó una solicitud de soporte por: {solicitud.TipoProblema.Nombre}"
            ));
        }

        public void ModificarSolicitudPorAdmin(SolicitudSoporte solicitud)
        {
            ExigirGestion();
            SoporteDAL.Instancia.Actualizar(solicitud);
            string adminEmail = Session.Instance.Usuario?.Email ?? "ADMIN_DESCONOCIDO";

            GestorEventos.Instancia.Notificar(new EventoBitacora(
                adminEmail,
                "GESTION_SOPORTE",
                $"El administrador cambió la solicitud ID {solicitud.Id} del usuario {solicitud.EmailSolicitante} al estado {solicitud.Estado.Id}"
            ));
        }

        public List<SolicitudSoporte> ListarSolicitudes(int? idEstado = null)
        { ExigirGestion(); return SoporteDAL.Instancia.Listar(idEstado); }
        public List<TipoProblema> ObtenerTipos() => SoporteDAL.Instancia.ObtenerTipos();
        public List<EstadoSolicitud> ObtenerEstados() => SoporteDAL.Instancia.ObtenerEstados();

        private static void ExigirGestion()
        {
            if (Session.Instance.Usuario == null || !Session.Instance.Usuario.TienePermiso(TipoPermiso.Soporte_Gestion))
                throw new UnauthorizedAccessException("No tiene permiso para administrar solicitudes de soporte.");
        }

        private void ValidarEntradas(SolicitudSoporte s)
        {
            if (string.IsNullOrWhiteSpace(s.EmailSolicitante)) throw new Exception("El email es obligatorio.");
            if (string.IsNullOrWhiteSpace(s.Descripcion)) throw new Exception("Debe ingresar una descripción detallada del problema.");
            if (s.TipoProblema == null || s.TipoProblema.Id <= 0) throw new Exception("Debe seleccionar un tipo de problema válido.");

            string pattern = @"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(s.EmailSolicitante, pattern))
            {
                throw new Exception("El formato del correo electrónico es inválido.");
            }
        }
    }
}
