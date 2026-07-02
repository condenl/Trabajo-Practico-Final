using System;
using System.Collections.Generic;
using BE;
using BE.Seguridad;
using DAL;
using Servicios;

namespace BLL
{
    public class SeguridadBLL
    {
        public void CargarEstructuraPermisos(Usuario usuario)
        {
            if (usuario == null) return;
            usuario.Permisos = SeguridadDAL.Instancia.ObtenerPermisosDeUsuario(usuario.Id);
        }

        public List<Componente> ObtenerRolesDisponibles() => SeguridadDAL.Instancia.ObtenerTodasLasFamilias();

        public Familia ObtenerFamiliaCompleta(int idFamilia)
        {
            return SeguridadDAL.Instancia.ObtenerFamiliaCompleta(idFamilia);
        }

        public void AsignarRolAUsuario(Usuario usuario, Componente rol)
        {
            SeguridadDAL.Instancia.AsignarComponenteAUsuario(usuario.Id, rol.Id);

            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "ASIGNACION_ROL", $"Se asignó el rol '{rol.Nombre}' al usuario '{usuario.Email}'."));

            if (Session.Instance.Usuario != null && Session.Instance.Usuario.Id == usuario.Id)
            {
                CargarEstructuraPermisos(Session.Instance.Usuario);
            }
        }

        public void CrearRol(string nombre)
        {
            SeguridadDAL.Instancia.InsertarFamilia(nombre);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "ALTA_ROL", $"Se creó el rol: {nombre}"));
        }

        public void ModificarRol(int id, string nombre)
        {
            SeguridadDAL.Instancia.ModificarFamilia(id, nombre);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "MODIFICACION_ROL", $"Se modificó el rol ID {id}. Nuevo nombre: {nombre}"));
        }

        public void EliminarRol(int id)
        {
            SeguridadDAL.Instancia.EliminarFamilia(id);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "BAJA_ROL", $"Se eliminó el rol ID {id}"));
        }

        public List<Componente> ObtenerPatentesDisponibles() => SeguridadDAL.Instancia.ObtenerPatentes();
        public List<Componente> ObtenerPermisosDeRol(int idRol) => SeguridadDAL.Instancia.ObtenerPermisosDeRol(idRol);

        public void AsignarPermisoARol(Componente rolPadre, Componente componenteAAgregar)
        {
            if (rolPadre.Id == componenteAAgregar.Id)
                throw new Exception("Operación denegada: Un rol no puede contenerse a sí mismo.");

            if (componenteAAgregar is Familia)
            {
                if (ExisteReferenciaCircular(rolPadre.Id, componenteAAgregar.Id))
                    throw new Exception("Operación denegada: Esto generaría una referencia circular infinita (Ej: A contiene a B, y B intenta contener a A).");
            }

            SeguridadDAL.Instancia.AgregarPermisoARol(rolPadre.Id, componenteAAgregar.Id);

            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            string tipoAgregado = componenteAAgregar is Familia ? "Rol" : "Permiso";

            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "MODIFICACION_SEGURIDAD",
                $"Se agregó el {tipoAgregado} '{componenteAAgregar.Nombre}' al Rol '{rolPadre.Nombre}'"));
        }

        public void QuitarPermisoDeRol(Componente rol, Componente permiso)
        {
            SeguridadDAL.Instancia.QuitarPermisoDeRol(rol.Id, permiso.Id);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "MODIFICACION_SEGURIDAD", $"Se quitó el permiso '{permiso.Nombre}' del rol '{rol.Nombre}'"));
        }

        private bool ExisteReferenciaCircular(int idPadreDestino, int idHijoPotencial)
        {
            var hijosDelPotencial = ObtenerPermisosDeRol(idHijoPotencial);

            foreach (var hijo in hijosDelPotencial)
            {
                if (hijo.Id == idPadreDestino) return true;
                if (hijo is Familia)
                {
                    if (ExisteReferenciaCircular(idPadreDestino, hijo.Id)) return true;
                }
            }
            return false;
        }
    }
}