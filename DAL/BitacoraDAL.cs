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
    public class BitacoraDAL
    {
        private static BitacoraDAL instancia;
        private static readonly object bloqueo = new object();

        private BitacoraDAL() { }

        public static BitacoraDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null)
                    {
                        instancia = new BitacoraDAL();
                    }
                    return instancia;
                }
            }
        }

        public int Insertar(EventoBitacora evento)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaHora", evento.FechaHora),
                new SqlParameter("@Usuario", evento.UsuarioEmail),
                new SqlParameter("@Actividad", evento.Actividad),
                new SqlParameter("@Informacion", evento.InformacionAsociada)
            };

            return Conexion.Instancia.RUD("sp_Bitacora_Insertar", parametros);
        }

        public List<EventoBitacora> LeerFiltrado(DateTime? desde, DateTime? hasta, string usuario, string actividad)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaDesde", (object)desde ?? DBNull.Value),
                new SqlParameter("@FechaHasta", (object)hasta ?? DBNull.Value),
                new SqlParameter("@Usuario", string.IsNullOrEmpty(usuario) ? DBNull.Value : (object)usuario),
                new SqlParameter("@Actividad", string.IsNullOrEmpty(actividad) ? DBNull.Value : (object)actividad)
            };

            DataTable dt = Conexion.Instancia.Leer("sp_Bitacora_Consultar", parametros);
            return BitacoraMPP.Instancia.MapearLista(dt);
        }
    }
}