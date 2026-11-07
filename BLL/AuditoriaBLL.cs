using System;
using System.Collections.Generic;
using System.Linq;
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
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            if (snapshot.AuditAction != "UPDATE" && snapshot.AuditAction != "DELETE")
                throw new InvalidOperationException("Esta versión no admite restauración.");
            var actual = new UsuarioBLL().ListarTodos().FirstOrDefault(u => u.Id == snapshot.Id);
            if (snapshot.AuditAction == "UPDATE")
            {
                if (actual == null) throw new InvalidOperationException("La cuenta ya no existe. Seleccione su versión anterior a la eliminación.");
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
                Usuario usuarioActual = actual;
                string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";

                if (usuarioActual != null)
                {
                    AuditoriaDAL.Instancia.GuardarSnapshot("UPDATE", usuarioActual, responsable);
                }
                // Historical credentials are already hashed; do not validate them as a new password.
                usuarioParaRestaurar.Dni = Encriptador.EncriptarAES(usuarioParaRestaurar.Dni);
                UsuarioDAL.Instancia.Modificar(usuarioParaRestaurar);
            }
            else if (snapshot.AuditAction == "DELETE")
            {
                if (actual != null) throw new InvalidOperationException("La cuenta ya existe. Actualice el historial antes de restaurar.");
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
