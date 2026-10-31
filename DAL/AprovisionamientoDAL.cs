using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BE;
using DAL.Interfaces;
using MPP;

namespace DAL
{
    public class AprovisionamientoDAL : IAprovisionamientoRepository
    {
        private static AprovisionamientoDAL instancia;
        private static readonly object bloqueo = new object();

        private AprovisionamientoDAL() { }

        public static AprovisionamientoDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new AprovisionamientoDAL();
                    return instancia;
                }
            }
        }

        public int InsertarCotizacion(CotizacionProveedor cotizacion)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        int idCotizacion;
                        const string sqlCotizacion = @"
INSERT INTO CotizacionProveedor (IdProveedor, FechaEmision, FechaVigencia, Condiciones)
VALUES (@IdProveedor, @FechaEmision, @FechaVigencia, @Condiciones);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (SqlCommand comando = new SqlCommand(sqlCotizacion, conexion, transaccion))
                        {
                            comando.Parameters.AddWithValue("@IdProveedor", cotizacion.IdProveedor);
                            comando.Parameters.AddWithValue("@FechaEmision", cotizacion.FechaEmision);
                            comando.Parameters.AddWithValue("@FechaVigencia", cotizacion.FechaVigencia);
                            comando.Parameters.AddWithValue("@Condiciones", (object)cotizacion.Condiciones ?? DBNull.Value);
                            idCotizacion = Convert.ToInt32(comando.ExecuteScalar());
                        }

                        foreach (DetalleCotizacionProveedor detalle in cotizacion.Detalles)
                        {
                            const string sqlDetalle = @"
INSERT INTO DetalleCotizacionProveedor (IdCotizacion, IdProducto, PrecioUnitario, PlazoEntregaDias)
VALUES (@IdCotizacion, @IdProducto, @PrecioUnitario, @PlazoEntregaDias);";
                            using (SqlCommand comando = new SqlCommand(sqlDetalle, conexion, transaccion))
                            {
                                comando.Parameters.AddWithValue("@IdCotizacion", idCotizacion);
                                comando.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                comando.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                                comando.Parameters.AddWithValue("@PlazoEntregaDias", detalle.PlazoEntregaDias);
                                comando.ExecuteNonQuery();
                            }
                        }

                        transaccion.Commit();
                        return idCotizacion;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public int InsertarOrden(OrdenCompra orden)
        {
            return InsertarOrdenes(new List<OrdenCompra> { orden })[0];
        }

        public IList<int> InsertarOrdenes(IList<OrdenCompra> ordenes)
        {
            if (ordenes == null || ordenes.Count == 0) throw new ArgumentException("Debe indicar las órdenes a crear.");
            var ids = new List<int>();
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        foreach (OrdenCompra orden in ordenes)
                        {
                            if (orden == null || orden.Estado != EstadoOrdenCompra.Borrador || orden.Detalles == null || orden.Detalles.Count == 0 ||
                                orden.Detalles.Any(d => d == null || d.CantidadRecibida != 0))
                                throw new ArgumentException("La importación solo puede crear borradores sin recepciones.");
                            ids.Add(InsertarOrden(conexion, transaccion, orden));
                        }
                        transaccion.Commit();
                        return ids;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        private static int InsertarOrden(SqlConnection conexion, SqlTransaction transaccion, OrdenCompra orden)
        {
            // Recheck catalog availability under the same transaction, including concurrent deactivation.
            const string sqlOrden = @"
IF NOT EXISTS(SELECT 1 FROM Proveedor WITH (UPDLOCK,HOLDLOCK) WHERE IdProveedor=@IdProveedor AND Activo=1)
    THROW 50001, N'El proveedor no existe o dejó de estar activo. No se creó ninguna orden.', 1;
INSERT INTO OrdenCompra (IdProveedor, FechaEmision, Total, Estado, CreadaPor)
VALUES (@IdProveedor, @FechaEmision, @Total, @Estado, @CreadaPor);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            int idOrden;
            using (var comando = new SqlCommand(sqlOrden, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdProveedor", orden.IdProveedor);
                comando.Parameters.AddWithValue("@FechaEmision", orden.FechaEmision);
                comando.Parameters.AddWithValue("@Total", orden.Total);
                comando.Parameters.AddWithValue("@Estado", (int)orden.Estado);
                comando.Parameters.AddWithValue("@CreadaPor", orden.CreadaPor);
                idOrden = Convert.ToInt32(comando.ExecuteScalar());
            }
            const string sqlDetalle = @"
IF NOT EXISTS(SELECT 1 FROM Producto WITH (UPDLOCK,HOLDLOCK) WHERE IdProducto=@IdProducto AND Activo=1)
    THROW 50002, N'El producto no existe o dejó de estar activo. No se creó ninguna orden.', 1;
INSERT INTO DetalleOrdenCompra (IdOrdenCompra, IdProducto, Cantidad, CantidadRecibida, PrecioUnitario)
VALUES (@IdOrdenCompra, @IdProducto, @Cantidad, 0, @PrecioUnitario);";
            foreach (DetalleOrdenCompra detalle in orden.Detalles)
            {
                using (var comando = new SqlCommand(sqlDetalle, conexion, transaccion))
                {
                    comando.Parameters.AddWithValue("@IdOrdenCompra", idOrden);
                    comando.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                    comando.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                    comando.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                    comando.ExecuteNonQuery();
                }
            }
            return idOrden;
        }

        public OrdenCompra ObtenerOrden(int idOrdenCompra)
        {
            DataSet datos = LeerOrdenes(idOrdenCompra, null);
            if (datos.Tables[0].Rows.Count == 0) return null;
            return AprovisionamientoMPP.Instancia.MapearOrden(datos.Tables[0].Rows[0], datos.Tables[1].AsEnumerable());
        }

        public bool ActualizarEstado(int idOrdenCompra, EstadoOrdenCompra estadoEsperado, EstadoOrdenCompra nuevoEstado, int? idUsuarioContable, string motivoRechazo)
        {
            const string sql = @"
UPDATE OrdenCompra
SET Estado = @NuevoEstado,
    IdUsuarioAprobadorContable = CASE WHEN @IdUsuarioContable IS NULL THEN IdUsuarioAprobadorContable ELSE @IdUsuarioContable END,
    FechaAprobacion = CASE WHEN @NuevoEstado = 2 THEN GETDATE() ELSE FechaAprobacion END,
    MotivoRechazo = @MotivoRechazo
WHERE IdOrdenCompra = @IdOrdenCompra AND Estado = @EstadoEsperado;";

            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@IdOrdenCompra", idOrdenCompra);
                comando.Parameters.AddWithValue("@EstadoEsperado", (int)estadoEsperado);
                comando.Parameters.AddWithValue("@NuevoEstado", (int)nuevoEstado);
                comando.Parameters.AddWithValue("@IdUsuarioContable", (object)idUsuarioContable ?? DBNull.Value);
                comando.Parameters.AddWithValue("@MotivoRechazo", (object)motivoRechazo ?? DBNull.Value);
                conexion.Open();
                return comando.ExecuteNonQuery() == 1;
            }
        }

        public IDictionary<int, int> ContarProveedoresCotizadosPorProducto(IList<int> idsProducto, DateTime fechaReferencia)
        {
            Dictionary<int, int> resultado = new Dictionary<int, int>();
            if (idsProducto == null || idsProducto.Count == 0) return resultado;

            string[] nombresParametros = idsProducto.Select((id, indice) => "@Producto" + indice).ToArray();
            string sql = @"
SELECT d.IdProducto, COUNT(DISTINCT c.IdProveedor) AS CantidadProveedores
FROM CotizacionProveedor c
INNER JOIN DetalleCotizacionProveedor d ON d.IdCotizacion = c.IdCotizacion
WHERE c.FechaEmision <= @FechaReferencia
  AND c.FechaVigencia >= @FechaReferencia
  AND d.IdProducto IN (" + string.Join(",", nombresParametros) + @")
GROUP BY d.IdProducto;";

            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@FechaReferencia", fechaReferencia.Date);
                for (int indice = 0; indice < idsProducto.Count; indice++)
                    comando.Parameters.AddWithValue(nombresParametros[indice], idsProducto[indice]);

                conexion.Open();
                using (SqlDataReader lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        resultado[lector.GetInt32(0)] = lector.GetInt32(1);
                }
            }

            return resultado;
        }

        public IList<OrdenCompra> ListarOrdenes(int? idProveedor)
        {
            DataSet datos = LeerOrdenes(null, idProveedor);
            return AprovisionamientoMPP.Instancia.MapearOrdenes(datos.Tables[0], datos.Tables[1]);
        }

        public void RegistrarRecepcion(int idOrdenCompra, IList<Lote> lotes, EstadoOrdenCompra estadoFinal, string responsable)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        foreach (Lote lote in lotes)
                        {
                            const string sqlCantidad = @"
UPDATE DetalleOrdenCompra
SET CantidadRecibida = CantidadRecibida + @Cantidad
WHERE IdOrdenCompra = @IdOrdenCompra
  AND IdProducto = @IdProducto
  AND CantidadRecibida + @Cantidad <= Cantidad;";

                            using (SqlCommand comando = new SqlCommand(sqlCantidad, conexion, transaccion))
                            {
                                comando.Parameters.AddWithValue("@Cantidad", lote.CantidadIngresada);
                                comando.Parameters.AddWithValue("@IdOrdenCompra", idOrdenCompra);
                                comando.Parameters.AddWithValue("@IdProducto", lote.IdProducto);
                                if (comando.ExecuteNonQuery() != 1)
                                    throw new InvalidOperationException("La recepción supera la cantidad pendiente o el producto no pertenece a la orden.");
                            }

                            int idLote = InventarioDAL.InsertarLote(conexion, transaccion, lote);
                            InventarioDAL.InsertarMovimiento(
                                conexion,
                                transaccion,
                                idLote,
                                TipoMovimientoStock.IngresoCompra,
                                lote.CantidadIngresada,
                                responsable,
                                "Orden de compra " + idOrdenCompra);
                        }

                        using (SqlCommand comando = new SqlCommand(
                            "UPDATE OrdenCompra SET Estado = @Estado WHERE IdOrdenCompra = @IdOrdenCompra AND Estado IN (4, 5)",
                            conexion,
                            transaccion))
                        {
                            comando.Parameters.AddWithValue("@Estado", (int)estadoFinal);
                            comando.Parameters.AddWithValue("@IdOrdenCompra", idOrdenCompra);
                            if (comando.ExecuteNonQuery() != 1)
                                throw new InvalidOperationException("La orden ya no se encuentra habilitada para recepción.");
                        }

                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        private static DataSet LeerOrdenes(int? idOrdenCompra, int? idProveedor)
        {
            const string sql = @"
SELECT IdOrdenCompra, IdProveedor, FechaEmision, Total, Estado, IdUsuarioAprobadorContable, FechaAprobacion, MotivoRechazo, CreadaPor
FROM OrdenCompra
WHERE (@IdOrdenCompra IS NULL OR IdOrdenCompra = @IdOrdenCompra)
  AND (@IdProveedor IS NULL OR IdProveedor = @IdProveedor)
ORDER BY FechaEmision DESC, IdOrdenCompra DESC;

SELECT d.IdDetalle, d.IdOrdenCompra, d.IdProducto, d.Cantidad, d.CantidadRecibida, d.PrecioUnitario
FROM DetalleOrdenCompra d
INNER JOIN OrdenCompra o ON o.IdOrdenCompra = d.IdOrdenCompra
WHERE (@IdOrdenCompra IS NULL OR d.IdOrdenCompra = @IdOrdenCompra)
  AND (@IdProveedor IS NULL OR o.IdProveedor = @IdProveedor)
ORDER BY d.IdOrdenCompra, d.IdDetalle;";

            DataSet datos = new DataSet();
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
            {
                comando.Parameters.AddWithValue("@IdOrdenCompra", (object)idOrdenCompra ?? DBNull.Value);
                comando.Parameters.AddWithValue("@IdProveedor", (object)idProveedor ?? DBNull.Value);
                adaptador.Fill(datos);
            }
            return datos;
        }
    }
}
