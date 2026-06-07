using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;

namespace DAL
{
    public class DVVDAL
    {
        private static DVVDAL instancia;
        private static readonly object bloqueo = new object();

        private DVVDAL() { }

        public static DVVDAL Instancia
        { 
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new DVVDAL();
                    return instancia;
                }
            }
        }

        public string Obtener(string nombreEntidad)
        {
            SqlParameter[] p = { new SqlParameter("@NombreEntidad", nombreEntidad) };
            // Asumimos un select directo o SP
            DataTable dt = Conexion.Instancia.Leer("sp_DVV_Consultar", p);
            if (dt.Rows.Count > 0)
                return dt.Rows[0]["Valor"].ToString();
            return null;
        }

        public void Guardar(string nombreEntidad, string valor)
        {
            SqlParameter[] p = {
                new SqlParameter("@NombreEntidad", nombreEntidad),
                new SqlParameter("@Valor", valor)
            };
            string sql = @"
                IF EXISTS (SELECT 1 FROM DVV WHERE NombreEntidad = @NombreEntidad)
                    UPDATE DVV SET Valor = @Valor WHERE NombreEntidad = @NombreEntidad
                ELSE
                    INSERT INTO DVV (NombreEntidad, Valor) VALUES (@NombreEntidad, @Valor)";
            
            Conexion.Instancia.RUD(sql, p, CommandType.Text);
        }
    }
}
