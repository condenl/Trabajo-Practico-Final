using BE;
using MPP;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class RolDAL
    {
        private static RolDAL instancia;
        private static readonly object bloqueo = new object();

        private RolDAL() { }

        public static RolDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null)
                    {
                        instancia = new RolDAL();
                    }
                    return instancia;
                }
            }
        }

        public List<Rol> Listar()
        {
            DataTable dt = Conexion.Instancia.Leer("sp_ListarRoles", null);
            return RolMPP.Instancia.MapearLista(dt);
        }

        public int Insertar(Rol rol)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", rol.Nombre),
                new SqlParameter("@Descripcion", rol.Descripcion)
            };

            return Conexion.Instancia.RUD("SP_InsertarRol", parametros);
        }

        public int Modificar(Rol rol)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Id", rol.Id),
                new SqlParameter("@Nombre", rol.Nombre),
                new SqlParameter("@Descripcion", rol.Descripcion)
            };

            return Conexion.Instancia.RUD("SP_ModificarRol", parametros);
        }

        public int Eliminar(int id)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Id", id)
            };
            return Conexion.Instancia.RUD("SP_EliminarRol", parametros);
        }
    }
}