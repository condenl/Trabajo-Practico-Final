using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using BE.Interfaces;
using MPP;

namespace DAL
{
    public class ProveedorDAL : IProveedorRepository
    {
        private static ProveedorDAL instancia;
        private static readonly object bloqueo = new object();

        private ProveedorDAL() { }

        public static ProveedorDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new ProveedorDAL();
                    return instancia;
                }
            }
        }

        public int Guardar(Proveedor proveedor)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand("sp_Proveedor_Guardar", conexion))
            {
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@IdProveedor", proveedor.IdProveedor);
                comando.Parameters.AddWithValue("@RazonSocial", proveedor.RazonSocial);
                comando.Parameters.AddWithValue("@Cuit", proveedor.Cuit);
                comando.Parameters.AddWithValue("@Contacto", (object)proveedor.Contacto ?? DBNull.Value);
                comando.Parameters.AddWithValue("@AntiguedadMercado", proveedor.AntiguedadMercado);
                comando.Parameters.AddWithValue("@IndicePrestigio", proveedor.IndicePrestigio);
                comando.Parameters.AddWithValue("@Activo", proveedor.Activo);
                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public Proveedor Obtener(int idProveedor)
        {
            DataTable tabla = Conexion.Instancia.Leer(
                "sp_Proveedor_Obtener",
                new[] { new SqlParameter("@IdProveedor", idProveedor) });
            return tabla.Rows.Count == 0 ? null : ProveedorMPP.Instancia.Mapear(tabla.Rows[0]);
        }

        public IList<Proveedor> Listar(bool soloActivos)
        {
            DataTable tabla = Conexion.Instancia.Leer(
                "sp_Proveedor_Listar",
                new[] { new SqlParameter("@SoloActivos", soloActivos) });
            return ProveedorMPP.Instancia.MapearLista(tabla);
        }
    }
}
