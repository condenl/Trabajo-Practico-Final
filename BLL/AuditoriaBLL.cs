using System;
using System.Collections.Generic;
using BE;
using DAL;
using Servicios;

namespace BLL
{
    public class AuditoriaBLL
    {
        public List<UsuarioAuditoria> ListarHistorial()
        {
            return AuditoriaDAL.Instancia.Listar();
        }

        public void HacerRollback(UsuarioAuditoria snapshot)
        {
            if (snapshot.AuditAction == "UPDATE")
            {
                Usuario usuarioParaRestaurar = new Usuario
                {
                    Id = snapshot.Id,
                    Dni = snapshot.Dni,
                    Nombre = snapshot.Nombre,
                    Apellido = snapshot.Apellido,
                    Email = snapshot.Email,
                    Password = snapshot.Password,
                    FechaAlta = snapshot.FechaAlta,
                    Activo = snapshot.Activo,
                    IntentosFallidos = snapshot.IntentosFallidos,
                    DVH = snapshot.DVH
                };
                UsuarioBLL gestorUsuario = new UsuarioBLL();
                Usuario usuarioActual = gestorUsuario.BuscarPorEmail(usuarioParaRestaurar.Email);
                string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";

                if (usuarioActual != null)
                {
                    AuditoriaDAL.Instancia.GuardarSnapshot("UPDATE", usuarioActual, responsable);
                }
                gestorUsuario.Guardar(usuarioParaRestaurar);
            }
            else if (snapshot.AuditAction == "DELETE")
            {
                AuditoriaDAL.Instancia.RestaurarEliminado(snapshot);
                UsuarioBLL gestorUsuario = new UsuarioBLL();
                gestorUsuario.RecalcularDV();
            }

            string respBitacora = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(
                respBitacora,
                "ROLLBACK_AUDITORIA",
                $"Se restauró al usuario {snapshot.Email} al estado exacto del {snapshot.AuditDate:dd/MM/yyyy HH:mm:ss}"
            ));
        }
    }
}