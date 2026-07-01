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
                // 1. ARMAMOS EL OBJETO A MANO (Adiós al bug silencioso del Clone)
                // Nos aseguramos de que el Id exacto viaje a SQL Server
                Usuario usuarioParaRestaurar = new Usuario
                {
                    Id = snapshot.Id,
                    Dni = snapshot.Dni, // Va en texto plano porque la BLL lo encriptará
                    Nombre = snapshot.Nombre,
                    Apellido = snapshot.Apellido,
                    Email = snapshot.Email,
                    Password = snapshot.Password,
                    FechaAlta = snapshot.FechaAlta,
                    Activo = snapshot.Activo,
                    IntentosFallidos = snapshot.IntentosFallidos,
                    DVH = snapshot.DVH
                };

                // 2. AUDITAR AL AUDITOR (Foto del futuro)
                // Antes de pisar el dato erróneo (el "aaaa"), le sacamos una foto para que quede 
                // registro en la grilla de que alguien deshizo este error.
                UsuarioBLL gestorUsuario = new UsuarioBLL();
                Usuario usuarioActual = gestorUsuario.BuscarPorEmail(usuarioParaRestaurar.Email);
                string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";

                if (usuarioActual != null)
                {
                    AuditoriaDAL.Instancia.GuardarSnapshot("UPDATE", usuarioActual, responsable);
                }

                // 3. MAGIA DE ARQUITECTURA: Reutilizamos tu BLL
                // Al llamar a Guardar, tu BLL va a encriptar el DNI automáticamente
                // y va a mandar a persistir el cambio ("aaa") a la BD.
                gestorUsuario.Guardar(usuarioParaRestaurar);
            }
            else if (snapshot.AuditAction == "DELETE")
            {
                // Para restaurar un eliminado pasamos el snapshot directo porque 
                // AuditoriaDAL.RestaurarEliminado ya tiene el EncriptarAES por dentro.
                AuditoriaDAL.Instancia.RestaurarEliminado(snapshot);

                // Recalculamos toda la integridad de la base
                UsuarioBLL gestorUsuario = new UsuarioBLL();
                gestorUsuario.RecalcularDV();
            }

            // 4. Registramos en la bitácora perezosa (el archivo log de sistema)
            string respBitacora = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(
                respBitacora,
                "ROLLBACK_AUDITORIA",
                $"Se restauró al usuario {snapshot.Email} al estado exacto del {snapshot.AuditDate:dd/MM/yyyy HH:mm:ss}"
            ));
        }
    }
}