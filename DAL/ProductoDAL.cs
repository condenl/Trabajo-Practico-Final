using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL.Interfaces;
using MPP;

namespace DAL
{
    public class ProductoDAL : IProductoRepository
    {
        private static ProductoDAL instancia;
        private static readonly object bloqueo = new object();

        private ProductoDAL() { }

        public static ProductoDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new ProductoDAL();
                    return instancia;
                }
            }
        }

        public int Guardar(Producto producto)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand("sp_Producto_Guardar", conexion))
            {
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                comando.Parameters.AddWithValue("@Codigo", producto.Codigo);
                comando.Parameters.AddWithValue("@Nombre", producto.Nombre);
                comando.Parameters.AddWithValue("@Marca", (object)producto.Marca ?? DBNull.Value);
                comando.Parameters.AddWithValue("@FamiliaOlfativa", (object)producto.FamiliaOlfativa ?? DBNull.Value);
                comando.Parameters.AddWithValue("@PrecioVenta", producto.PrecioVenta);
                comando.Parameters.AddWithValue("@StockMinimo", producto.StockMinimo);
                comando.Parameters.AddWithValue("@Activo", producto.Activo);
                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public Producto Obtener(int idProducto)
        {
            DataTable tabla = Conexion.Instancia.Leer(
                "sp_Producto_Obtener",
                new[] { new SqlParameter("@IdProducto", idProducto) });
            return tabla.Rows.Count == 0 ? null : ProductoMPP.Instancia.Mapear(tabla.Rows[0]);
        }

        public IList<Producto> Listar(bool soloActivos)
        {
            DataTable tabla = Conexion.Instancia.Leer(
                "sp_Producto_Listar",
                new[] { new SqlParameter("@SoloActivos", soloActivos) });
            return ProductoMPP.Instancia.MapearLista(tabla);
        }
    }
}
