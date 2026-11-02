using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL.Interfaces;
using MPP;

namespace DAL
{
    public class InventarioDAL : IInventarioRepository
    {
        private static InventarioDAL instancia;
        private static readonly object bloqueo = new object();

        private InventarioDAL() { }

        public static InventarioDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new InventarioDAL();
                    return instancia;
                }
            }
        }

        public int VerificarStock(int idProducto)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand("SELECT ISNULL(SUM(CantidadDisponible), 0) FROM Lote WHERE IdProducto = @IdProducto", conexion))
            {
                comando.Parameters.AddWithValue("@IdProducto", idProducto);
                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public int RegistrarIngresoLote(Lote lote, string responsable, string referencia)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int idLote = InsertarLote(conexion, transaccion, lote);
                        InsertarMovimiento(conexion, transaccion, idLote, TipoMovimientoStock.AjustePositivo, lote.CantidadIngresada, responsable, referencia);
                        transaccion.Commit();
                        return idLote;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool DescontarStockLote(int idLote, int cantidad, string responsable, string referencia)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int afectadas;
                        using (SqlCommand comando = new SqlCommand(
                            "UPDATE Lote SET CantidadDisponible = CantidadDisponible - @Cantidad WHERE IdLote = @IdLote AND CantidadDisponible >= @Cantidad", conexion, transaccion))
                        {
                            comando.Parameters.AddWithValue("@IdLote", idLote);
                            comando.Parameters.AddWithValue("@Cantidad", cantidad);
                            afectadas = comando.ExecuteNonQuery();
                        }

                        if (afectadas == 0)
                        {
                            transaccion.Rollback();
                            return false;
                        }

                        InsertarMovimiento(conexion, transaccion, idLote, TipoMovimientoStock.SalidaVenta, -cantidad, responsable, referencia);
                        transaccion.Commit();
                        return true;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public IList<Lote> ObtenerLotesDisponibles(int idProducto)
        {
            return LeerLotes(
                "SELECT * FROM Lote WHERE IdProducto = @IdProducto AND CantidadDisponible > 0 AND (FechaVencimiento IS NULL OR FechaVencimiento > CAST(GETDATE() AS DATE)) ORDER BY FechaVencimiento, FechaRecepcion",
                idProducto);
        }

        public IList<Lote> ObtenerTrazabilidad(int idProducto)
        {
            return LeerLotes(
                "SELECT * FROM Lote WHERE IdProducto = @IdProducto ORDER BY FechaRecepcion DESC, IdLote DESC",
                idProducto);
        }

        internal static int InsertarLote(SqlConnection conexion, SqlTransaction transaccion, Lote lote)
        {
            // La lectura bloquea también la clave aún inexistente hasta terminar la recepción.
            // Así una recepción concurrente no puede insertar el mismo origen entre la validación y el alta.
            const string sqlExistente = @"
SELECT p.Nombre, pr.RazonSocial
FROM Lote l WITH (UPDLOCK, HOLDLOCK)
INNER JOIN Producto p ON p.IdProducto = l.IdProducto
INNER JOIN Proveedor pr ON pr.IdProveedor = l.IdProveedor
WHERE l.IdProducto = @IdProducto AND l.IdProveedor = @IdProveedor AND l.CodigoLote = @CodigoLote;";
            using (SqlCommand comando = new SqlCommand(sqlExistente, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdProducto", lote.IdProducto);
                comando.Parameters.AddWithValue("@IdProveedor", lote.IdProveedor);
                comando.Parameters.AddWithValue("@CodigoLote", lote.CodigoLote);
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    if (lector.Read())
                        throw new InvalidOperationException(string.Format(
                            "El lote '{0}' del producto '{1}' y proveedor '{2}' ya está registrado. No se registró el ingreso. Revise la trazabilidad y el código del lote recibido para evitar duplicar mercadería.",
                            lote.CodigoLote, lector.GetString(0), lector.GetString(1)));
                }
            }

            const string sql = @"
INSERT INTO Lote
    (IdProducto, IdProveedor, IdOrdenCompra, CodigoLote, Origen, CertificadoOrigen, FechaVencimiento, FechaRecepcion, CantidadIngresada, CantidadDisponible)
VALUES
    (@IdProducto, @IdProveedor, @IdOrdenCompra, @CodigoLote, @Origen, @CertificadoOrigen, @FechaVencimiento, @FechaRecepcion, @CantidadIngresada, @CantidadDisponible);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdProducto", lote.IdProducto);
                comando.Parameters.AddWithValue("@IdProveedor", lote.IdProveedor);
                comando.Parameters.AddWithValue("@IdOrdenCompra", (object)lote.IdOrdenCompra ?? DBNull.Value);
                comando.Parameters.AddWithValue("@CodigoLote", lote.CodigoLote);
                comando.Parameters.AddWithValue("@Origen", lote.Origen);
                comando.Parameters.AddWithValue("@CertificadoOrigen", lote.CertificadoOrigen ?? string.Empty);
                comando.Parameters.AddWithValue("@FechaVencimiento", (object)lote.FechaVencimiento ?? DBNull.Value);
                comando.Parameters.AddWithValue("@FechaRecepcion", lote.FechaRecepcion);
                comando.Parameters.AddWithValue("@CantidadIngresada", lote.CantidadIngresada);
                comando.Parameters.AddWithValue("@CantidadDisponible", lote.CantidadDisponible);
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        internal static void InsertarMovimiento(SqlConnection conexion, SqlTransaction transaccion, int idLote, TipoMovimientoStock tipo, int cantidad, string responsable, string referencia)
        {
            const string sql = @"
INSERT INTO MovimientoStock (IdLote, Tipo, Cantidad, Fecha, Referencia, Responsable)
VALUES (@IdLote, @Tipo, @Cantidad, GETDATE(), @Referencia, @Responsable);";

            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdLote", idLote);
                comando.Parameters.AddWithValue("@Tipo", (int)tipo);
                comando.Parameters.AddWithValue("@Cantidad", cantidad);
                comando.Parameters.AddWithValue("@Referencia", (object)referencia ?? DBNull.Value);
                comando.Parameters.AddWithValue("@Responsable", responsable);
                comando.ExecuteNonQuery();
            }
        }

        private static IList<Lote> LeerLotes(string sql, int idProducto)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
            {
                comando.Parameters.AddWithValue("@IdProducto", idProducto);
                adaptador.Fill(tabla);
            }
            return InventarioMPP.Instancia.MapearLotes(tabla);
        }
    }
}
