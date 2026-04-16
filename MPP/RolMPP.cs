using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class RolMPP
    {
        private static RolMPP instancia;
        private static readonly object bloqueo = new object();

        private RolMPP() { }

        public static RolMPP Instancia
        {
            get
            {
                if (instancia == null)
                {
                    lock (bloqueo)
                    {
                        if (instancia == null) instancia = new RolMPP();
                    }
                }
                return instancia;
            }
        }

        public Rol MapearRol(DataRow fila)
        {
            Rol rol = new Rol();
            rol.Id = Convert.ToInt32(fila["Id"]);
            rol.Nombre = fila["Nombre"].ToString();

            if (fila["Descripcion"] != DBNull.Value)
                rol.Descripcion = fila["Descripcion"].ToString();

            rol.Activo = Convert.ToBoolean(fila["Activo"]);

            return rol;
        }

        public List<Rol> MapearLista(DataTable dt)
        {
            List<Rol> lista = new List<Rol>();
            if (dt != null)
            {
                foreach (DataRow fila in dt.Rows)
                {
                    lista.Add(MapearRol(fila));
                }
            }
            return lista;
        }
    }
}