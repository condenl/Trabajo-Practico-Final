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

        public void AsignarPermisoARol(Componente rol, Componente permiso)
        {
            SeguridadDAL.Instancia.AgregarPermisoARol(rol.Id, permiso.Id);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "MODIFICACION_SEGURIDAD", $"Se agregó el permiso '{permiso.Nombre}' al rol '{rol.Nombre}'"));
        }

        public void QuitarPermisoDeRol(Componente rol, Componente permiso)
        {
            SeguridadDAL.Instancia.QuitarPermisoDeRol(rol.Id, permiso.Id);
            string responsable = Session.Instance.Usuario?.Email ?? "SISTEMA";
            GestorEventos.Instancia.Notificar(new EventoBitacora(responsable, "MODIFICACION_SEGURIDAD", $"Se quitó el permiso '{permiso.Nombre}' del rol '{rol.Nombre}'"));
        }
    }
}