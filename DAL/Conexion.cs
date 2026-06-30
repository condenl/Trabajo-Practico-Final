using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DAL
{
    public class Conexion
    {
        private static Conexion instancia;
        private static readonly object bloqueo = new object();
        private string connectionString;
        private static SqlConnection conexion;

        private Conexion()
        {
            connectionString = ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString;
        }

        public static Conexion Instancia
        {
            get
            {
                if (instancia == null)
                {
                    lock (bloqueo)
                    {
                        if (instancia == null)
                        {
                            instancia = new Conexion();
                        }
                    }
                }
                return instancia;
            }
        }

        public SqlConnection ObtenerConexion()
        {
            return new SqlConnection(connectionString);
        }

        public int RUD(string commandText, SqlParameter[] parametros)
        {
            return RUD(commandText, parametros, CommandType.StoredProcedure);
        }

        public int RUD(string commandText, SqlParameter[] parametros, CommandType commandType)
        {
            using (SqlConnection cn = this.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(commandText, cn))
                {
                    cmd.CommandType = commandType;

                    if (parametros != null)
                    {
                        cmd.Parameters.AddRange(parametros);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable Leer(string storedProcedure, SqlParameter[] parametros)
        {
            DataTable dt = new DataTable();
            using (SqlConnection cn = this.ObtenerConexion())
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(storedProcedure, cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parametros != null)
                    {
                        cmd.Parameters.AddRange(parametros);
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                return dt;
            }
        }

        public void RestaurarBaseDeDatos(string backupPath)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using (SqlConnection cn = new SqlConnection(builder.ConnectionString))
            {
                cn.Open();
                string query = $@"
                    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{dbName}] FROM DISK = @BackupPath WITH REPLACE;
                    ALTER DATABASE [{dbName}] SET MULTI_USER;";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.Add(new SqlParameter("@BackupPath", backupPath));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}