using System.Collections.Generic;
using BE;
using BE.Seguridad;
using DAL;

namespace BLL
{
    public class SeguridadBLL
    {
        public void CargarEstructuraPermisos(Usuario usuario)
        {
            if (usuario == null) return;
            usuario.Permisos = SeguridadDAL.Instancia.ObtenerPermisosDeUsuario(usuario.Id);
        }

        public List<Componente> ObtenerRolesDisponibles()
        {
            return SeguridadDAL.Instancia.ObtenerTodasLasFamilias();
        }

        public void AsignarRolAUsuario(Usuario usuario, Componente rol)
        {
            SeguridadDAL.Instancia.AsignarComponenteAUsuario(usuario.Id, rol.Id);
            if (Servicios.Session.Instance.Usuario != null && Servicios.Session.Instance.Usuario.Id == usuario.Id)
            {
                CargarEstructuraPermisos(Servicios.Session.Instance.Usuario);
            }
        }

        public void CrearRol(string nombre) => SeguridadDAL.Instancia.InsertarFamilia(nombre);
        public void ModificarRol(int id, string nombre) => SeguridadDAL.Instancia.ModificarFamilia(id, nombre);
        public void EliminarRol(int id) => SeguridadDAL.Instancia.EliminarFamilia(id);

        public List<Componente> ObtenerPatentesDisponibles() => SeguridadDAL.Instancia.ObtenerPatentes();
        public List<Componente> ObtenerPermisosDeRol(int idRol) => SeguridadDAL.Instancia.ObtenerPermisosDeRol(idRol);

        public void AsignarPermisoARol(Componente rol, Componente permiso)
            => SeguridadDAL.Instancia.AgregarPermisoARol(rol.Id, permiso.Id);

        public void QuitarPermisoDeRol(Componente rol, Componente permiso)
            => SeguridadDAL.Instancia.QuitarPermisoDeRol(rol.Id, permiso.Id);
    }
}