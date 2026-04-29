using System;
using System.Collections.Generic;
using System.Data;
using BE;
using Servicios;

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
                string dniCifrado = fila["Dni"].ToString();
                usuario.Dni = Encriptador.DesencriptarAES(dniCifrado);

                usuario.Nombre = fila["Nombre"].ToString();
                usuario.Apellido = fila["Apellido"].ToString();
                usuario.Email = fila["Email"].ToString();
                usuario.Password = fila["Password"].ToString();
                usuario.FechaAlta = Convert.ToDateTime(fila["FechaAlta"]);
                usuario.Activo = Convert.ToBoolean(fila["Activo"]);

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