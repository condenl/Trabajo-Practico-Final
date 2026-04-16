using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class UsuarioMPP
    {
        private static UsuarioMPP instancia;
        private static readonly object bloqueo = new object();

        private UsuarioMPP() { }

        public static UsuarioMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new UsuarioMPP();
                    return instancia;
                }
            }
        }

        public Usuario MapearUsuario(DataRow fila)
        {
            try
            {
                Usuario usuario = new Usuario();

                usuario.Id = Convert.ToInt32(fila["Id"]);
                usuario.Dni = Convert.ToInt32(fila["Dni"]);
                usuario.Nombre = fila["Nombre"].ToString();
                usuario.Apellido = fila["Apellido"].ToString();
                usuario.Email = fila["Email"].ToString();
                usuario.Password = fila["Password"].ToString();
                usuario.FechaAlta = Convert.ToDateTime(fila["FechaAlta"]);
                usuario.Activo = Convert.ToBoolean(fila["Activo"]);

                if (fila["RolId"] != DBNull.Value)
                    usuario.RolId = Convert.ToInt32(fila["RolId"]);

                if (fila.Table.Columns.Contains("RolNombre") && fila["RolNombre"] != DBNull.Value)
                {
                    usuario.RolAsignado = new Rol();
                    usuario.RolAsignado.Id = usuario.RolId;
                    usuario.RolAsignado.Nombre = fila["RolNombre"].ToString();
                }
                else
                {
                    usuario.RolAsignado = new Rol { Id = usuario.RolId, Nombre = "Desconocido" };
                }

                if (fila.Table.Columns.Contains("IntentosFallidos") && fila["IntentosFallidos"] != DBNull.Value)
                {
                    usuario.IntentosFallidos = Convert.ToInt32(fila["IntentosFallidos"]);
                }

                return usuario;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al mapear usuario.", ex);
            }
        }

        public List<Usuario> MapearLista(DataTable dt)
        {
            List<Usuario> lista = new List<Usuario>();
            if (dt != null)
            {
                foreach (DataRow fila in dt.Rows)
                    lista.Add(MapearUsuario(fila));
            }
            return lista;
        }
    }
}