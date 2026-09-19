using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using BE.Interfaces;
using MPP;

namespace DAL
{
    public class VentaDAL : IVentaRepository
    {
        private static VentaDAL instancia;
        private static readonly object bloqueo = new object();

        private VentaDAL() { }

        public static VentaDAL Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new VentaDAL();
                    return instancia;
                }
            }
        }

        public Cliente BuscarClientePorEmail(string email)
        {
            const string sql = "SELECT TOP 1 * FROM Cliente WHERE Email = @Email AND Activo = 1";
            DataTable tabla = EjecutarConsulta(sql, new SqlParameter("@Email", email));
            return tabla.Rows.Count == 0 ? null : VentaMPP.Instancia.MapearCliente(tabla.Rows[0]);
        }

        public int GuardarCliente(Cliente cliente)
        {
            const string sql = @"
INSERT INTO Cliente (Nombre, Apellido, Email, Telefono, PreferenciasOlfativas, AutorizaMarketing, FechaAlta, Activo)
VALUES (@Nombre, @Apellido, @Email, @Telefono, @PreferenciasOlfativas, @AutorizaMarketing, @FechaAlta, @Activo);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@Nombre", cliente.Nombre.Trim());
                comando.Parameters.AddWithValue("@Apellido", cliente.Apellido.Trim());
                comando.Parameters.AddWithValue("@Email", cliente.Email);
                comando.Parameters.AddWithValue("@Telefono", (object)cliente.Telefono ?? DBNull.Value);
                comando.Parameters.AddWithValue("@PreferenciasOlfativas", (object)cliente.PreferenciasOlfativas ?? DBNull.Value);
                comando.Parameters.AddWithValue("@AutorizaMarketing", cliente.AutorizaMarketing);
                comando.Parameters.AddWithValue("@FechaAlta", cliente.FechaAlta);
                comando.Parameters.AddWithValue("@Activo", cliente.Activo);
                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public Membresia ObtenerMembresiaActiva(int idCliente)
        {
            const string sql = "SELECT TOP 1 * FROM Membresia WHERE IdCliente = @IdCliente AND Activa = 1 ORDER BY FechaAlta DESC";
            DataTable tabla = EjecutarConsulta(sql, new SqlParameter("@IdCliente", idCliente));
            return tabla.Rows.Count == 0 ? null : VentaMPP.Instancia.MapearMembresia(tabla.Rows[0]);
        }

        public int CrearMembresia(Membresia membresia)
        {
            const string sql = @"
INSERT INTO Membresia (IdCliente, Numero, FechaAlta, PorcentajeDescuento, Activa)
VALUES (@IdCliente, @Numero, @FechaAlta, @PorcentajeDescuento, @Activa);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@IdCliente", membresia.IdCliente);
                comando.Parameters.AddWithValue("@Numero", membresia.Numero);
                comando.Parameters.AddWithValue("@FechaAlta", membresia.FechaAlta);
                comando.Parameters.AddWithValue("@PorcentajeDescuento", membresia.PorcentajeDescuento);
                comando.Parameters.AddWithValue("@Activa", membresia.Activa);
                conexion.Open();
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public int RegistrarVenta(Venta venta, string responsable)
        {
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            {
                conexion.Open();
                using (SqlTransaction transaccion = conexion.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int idVenta = InsertarVenta(conexion, transaccion, venta);
                        foreach (DetalleVenta detalle in venta.Detalles)
                        {
                            using (SqlCommand descontar = new SqlCommand(
                                "UPDATE Lote SET CantidadDisponible = CantidadDisponible - @Cantidad WHERE IdLote = @IdLote AND IdProducto = @IdProducto AND CantidadDisponible >= @Cantidad",
                                conexion,
                                transaccion))
                            {
                                descontar.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                                descontar.Parameters.AddWithValue("@IdLote", detalle.IdLote);
                                descontar.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                                if (descontar.ExecuteNonQuery() == 0)
                                    throw new InvalidOperationException("El stock cambió durante la venta. Revise las cantidades e intente nuevamente.");
                            }

                            InsertarDetalle(conexion, transaccion, idVenta, detalle);
                            InventarioDAL.InsertarMovimiento(
                                conexion,
                                transaccion,
                                detalle.IdLote,
                                TipoMovimientoStock.SalidaVenta,
                                -detalle.Cantidad,
                                responsable,
                                "Venta " + idVenta);
                        }

                        transaccion.Commit();
                        return idVenta;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public IList<Venta> ListarVentasCliente(int idCliente)
        {
            const string sql = "SELECT * FROM Venta WHERE IdCliente = @IdCliente ORDER BY Fecha DESC, IdVenta DESC";
            return VentaMPP.Instancia.MapearVentas(EjecutarConsulta(sql, new SqlParameter("@IdCliente", idCliente)));
        }

        private static int InsertarVenta(SqlConnection conexion, SqlTransaction transaccion, Venta venta)
        {
            const string sql = @"
INSERT INTO Venta (IdCliente, IdUsuarioVendedor, Fecha, Subtotal, Descuento, Total)
VALUES (@IdCliente, @IdUsuarioVendedor, @Fecha, @Subtotal, @Descuento, @Total);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdCliente", (object)venta.IdCliente ?? DBNull.Value);
                comando.Parameters.AddWithValue("@IdUsuarioVendedor", (object)venta.IdUsuarioVendedor ?? DBNull.Value);
                comando.Parameters.AddWithValue("@Fecha", venta.Fecha);
                comando.Parameters.AddWithValue("@Subtotal", venta.Subtotal);
                comando.Parameters.AddWithValue("@Descuento", venta.Descuento);
                comando.Parameters.AddWithValue("@Total", venta.Total);
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        private static void InsertarDetalle(SqlConnection conexion, SqlTransaction transaccion, int idVenta, DetalleVenta detalle)
        {
            const string sql = @"
INSERT INTO DetalleVenta (IdVenta, IdProducto, IdLote, Cantidad, PrecioUnitario, DescuentoUnitario)
VALUES (@IdVenta, @IdProducto, @IdLote, @Cantidad, @PrecioUnitario, @DescuentoUnitario);";
            using (SqlCommand comando = new SqlCommand(sql, conexion, transaccion))
            {
                comando.Parameters.AddWithValue("@IdVenta", idVenta);
                comando.Parameters.AddWithValue("@IdProducto", detalle.IdProducto);
                comando.Parameters.AddWithValue("@IdLote", detalle.IdLote);
                comando.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                comando.Parameters.AddWithValue("@PrecioUnitario", detalle.PrecioUnitario);
                comando.Parameters.AddWithValue("@DescuentoUnitario", detalle.DescuentoUnitario);
                comando.ExecuteNonQuery();
            }
        }

        private static DataTable EjecutarConsulta(string sql, params SqlParameter[] parametros)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = Conexion.Instancia.ObtenerConexion())
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
            {
                if (parametros != null && parametros.Length > 0) comando.Parameters.AddRange(parametros);
                adaptador.Fill(tabla);
            }
            return tabla;
        }
    }
}
