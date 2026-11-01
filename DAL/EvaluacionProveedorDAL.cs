using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BE;
using DAL.Interfaces;
using Servicios;

namespace DAL
{
    public sealed class EvaluacionProveedorDAL : IEvaluacionProveedorRepository
    {
        private static readonly EvaluacionProveedorDAL instancia = new EvaluacionProveedorDAL();
        private EvaluacionProveedorDAL() { }
        public static EvaluacionProveedorDAL Instancia { get { return instancia; } }

        public DatosEvaluacion ObtenerDatos(int idProducto, DateTime fechaCorte)
        {
            using (var c = Conexion.Instancia.ObtenerConexion())
            {
                c.Open();
                using (var t = c.BeginTransaction(IsolationLevel.Serializable))
                { var datos = LeerDatos(c, t, idProducto, fechaCorte); t.Commit(); return datos; }
            }
        }
        private static DatosEvaluacion LeerDatos(SqlConnection c, SqlTransaction t, int producto, DateTime fecha)
        {
            const string sql = @"
SELECT * FROM dbo.Producto WHERE IdProducto=@Producto;
SELECT
 (SELECT ISNULL(SUM(CantidadDisponible),0) FROM dbo.Lote WHERE IdProducto=@Producto AND (FechaVencimiento IS NULL OR FechaVencimiento>@Fecha)) Stock,
 (SELECT ISNULL(SUM(d.Cantidad-d.CantidadRecibida),0) FROM dbo.DetalleOrdenCompra d JOIN dbo.OrdenCompra o ON o.IdOrdenCompra=d.IdOrdenCompra WHERE d.IdProducto=@Producto AND o.Estado IN(4,5) AND o.FechaEmision<@Hasta) Pendiente,
 (SELECT ISNULL(SUM(d.Cantidad),0) FROM dbo.DetalleVenta d JOIN dbo.Venta v ON v.IdVenta=d.IdVenta WHERE d.IdProducto=@Producto AND v.Fecha>=@Inicio90 AND v.Fecha<@Hasta) Vendidas,
 (SELECT ISNULL(SUM(d.Cantidad*(d.PrecioUnitario-d.DescuentoUnitario)),0) FROM dbo.DetalleVenta d JOIN dbo.Venta v ON v.IdVenta=d.IdVenta WHERE d.IdProducto=@Producto AND v.Fecha>=@Inicio90 AND v.Fecha<@Hasta) VentasNetas;
SELECT p.IdProveedor,p.RazonSocial,p.Activo,p.AntiguedadMercado,p.IndicePrestigio,
 q.IdCotizacion,q.FechaEmision,q.FechaVigencia,q.PrecioUnitario,q.PlazoEntregaDias,
 ISNULL(h.Compradas,0) Compradas,ISNULL(h.Recibidas,0) Recibidas,ISNULL(h.Importe,0) Importe
FROM dbo.Proveedor p
OUTER APPLY (SELECT TOP(1) cp.IdCotizacion,cp.FechaEmision,cp.FechaVigencia,d.PrecioUnitario,d.PlazoEntregaDias
 FROM dbo.CotizacionProveedor cp JOIN dbo.DetalleCotizacionProveedor d ON d.IdCotizacion=cp.IdCotizacion
 WHERE cp.IdProveedor=p.IdProveedor AND d.IdProducto=@Producto
 ORDER BY CASE WHEN cp.FechaEmision<=@Fecha AND cp.FechaVigencia>=@Fecha THEN 0 ELSE 1 END,cp.FechaEmision DESC,cp.IdCotizacion DESC) q
OUTER APPLY (SELECT SUM(d.Cantidad) Compradas,SUM(d.CantidadRecibida) Recibidas,SUM(d.Cantidad*d.PrecioUnitario) Importe
 FROM dbo.OrdenCompra o JOIN dbo.DetalleOrdenCompra d ON d.IdOrdenCompra=o.IdOrdenCompra
 WHERE o.IdProveedor=p.IdProveedor AND d.IdProducto=@Producto AND o.Estado IN(4,5,6) AND o.FechaEmision>=@Inicio180 AND o.FechaEmision<@Hasta) h
ORDER BY p.IdProveedor;";
            var ds = new DataSet();
            using (var cmd = new SqlCommand(sql, c, t))
            {
                cmd.Parameters.AddWithValue("@Producto", producto);
                cmd.Parameters.AddWithValue("@Fecha", fecha.Date);
                cmd.Parameters.AddWithValue("@Hasta", fecha.Date.AddDays(1));
                cmd.Parameters.AddWithValue("@Inicio90", fecha.Date.AddDays(-89));
                cmd.Parameters.AddWithValue("@Inicio180", fecha.Date.AddDays(-179));
                using (var adapter = new SqlDataAdapter(cmd)) adapter.Fill(ds);
            }
            var result = new DatosEvaluacion { FechaCorte = fecha.Date };
            if (ds.Tables[0].Rows.Count == 0) return result;
            var p = ds.Tables[0].Rows[0];
            result.Producto = new Producto { IdProducto = producto, Codigo = (string)p["Codigo"], Nombre = (string)p["Nombre"],
                Marca = p["Marca"] as string, FamiliaOlfativa = p["FamiliaOlfativa"] as string, Activo = (bool)p["Activo"],
                PrecioVenta = (decimal)p["PrecioVenta"], StockMinimo = (int)p["StockMinimo"] };
            var a = ds.Tables[1].Rows[0];
            result.StockDisponible = Convert.ToInt32(a["Stock"]); result.ComprasPendientes = Convert.ToInt32(a["Pendiente"]);
            result.UnidadesVendidas90Dias = Convert.ToInt32(a["Vendidas"]); result.ImporteNetoVentas90Dias = Convert.ToDecimal(a["VentasNetas"]);
            foreach (DataRow r in ds.Tables[2].Rows)
                result.Ofertas.Add(new OfertaEvaluable { IdProveedor = (int)r["IdProveedor"], Proveedor = (string)r["RazonSocial"], Activo = (bool)r["Activo"],
                    AntiguedadMercado = (int)r["AntiguedadMercado"], IndicePrestigio = (decimal)r["IndicePrestigio"], IdCotizacion = r["IdCotizacion"] as int?,
                    FechaEmision = r["FechaEmision"] as DateTime?, FechaVigencia = r["FechaVigencia"] as DateTime?,
                    PrecioUnitario = r["PrecioUnitario"] == DBNull.Value ? 0 : (decimal)r["PrecioUnitario"],
                    PlazoEntregaDias = r["PlazoEntregaDias"] == DBNull.Value ? 0 : (int)r["PlazoEntregaDias"],
                    UnidadesCompradas180Dias = Convert.ToInt32(r["Compradas"]), UnidadesRecibidas180Dias = Convert.ToInt32(r["Recibidas"]), ImporteComprado180Dias = Convert.ToDecimal(r["Importe"]) });
            return result;
        }
        public int GuardarEvaluacion(EvaluacionProveedor e)
        {
            using (var c = Conexion.Instancia.ObtenerConexion())
            using (var cmd = new SqlCommand(@"INSERT dbo.EvaluacionProveedor(IdProducto,FechaEvaluacion,Responsable,SnapshotXml)
 VALUES(@p,@f,@r,@xml); SELECT CAST(SCOPE_IDENTITY() AS INT);", c))
            {
                cmd.Parameters.AddWithValue("@p", e.Solicitud.IdProducto); cmd.Parameters.AddWithValue("@f", e.FechaEvaluacion);
                cmd.Parameters.AddWithValue("@r", e.Responsable); cmd.Parameters.AddWithValue("@xml", ArchivoEvaluacion.Serializar(e));
                c.Open(); return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
        private static EvaluacionProveedor Leer(SqlConnection c, SqlTransaction t, int id, bool bloquear)
        {
            using (var cmd = new SqlCommand("SELECT * FROM dbo.EvaluacionProveedor " + (bloquear ? "WITH(UPDLOCK,HOLDLOCK) " : "") + "WHERE IdEvaluacion=@id", c, t))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) throw new InvalidOperationException("No existe la evaluación.");
                    var e = ArchivoEvaluacion.Leer((string)r["SnapshotXml"]); e.IdEvaluacion = id;
                    e.IdProveedorSeleccionado = r["IdProveedorSeleccionado"] as int?; e.MotivoSeleccion = r["MotivoSeleccion"] as string;
                    e.IdOrdenCompra = r["IdOrdenCompra"] as int?;
                    e.SeleccionadaPor = r["SeleccionadaPor"] as string; e.FechaSeleccion = r["FechaSeleccion"] as DateTime?;
                    return e;
                }
            }
        }
        public EvaluacionProveedor ObtenerEvaluacion(int id)
        { using (var c = Conexion.Instancia.ObtenerConexion()) { c.Open(); return Leer(c, null, id, false); } }
        public IList<ResumenEvaluacion> ListarHistorial(int producto)
        {
            var result = new List<ResumenEvaluacion>();
            using (var c = Conexion.Instancia.ObtenerConexion())
            using (var cmd = new SqlCommand(@"SELECT TOP(100) e.IdEvaluacion,e.FechaEvaluacion,e.Responsable,p.RazonSocial,e.IdOrdenCompra
 FROM dbo.EvaluacionProveedor e LEFT JOIN dbo.Proveedor p ON p.IdProveedor=e.IdProveedorSeleccionado WHERE e.IdProducto=@p ORDER BY e.IdEvaluacion DESC", c))
            {
                cmd.Parameters.AddWithValue("@p", producto); c.Open();
                using (var r = cmd.ExecuteReader()) while (r.Read()) result.Add(new ResumenEvaluacion { IdEvaluacion = r.GetInt32(0), Fecha = r.GetDateTime(1), Responsable = r.GetString(2), ProveedorSeleccionado = r.IsDBNull(3) ? "Sin decisión" : r.GetString(3), IdOrdenCompra = r.IsDBNull(4) ? (int?)null : r.GetInt32(4) });
            }
            return result;
        }
        private static void ComprobarVigencia(SqlConnection c, SqlTransaction t, EvaluacionProveedor e)
        {
            if (e.Datos.FechaCorte.Date != DateTime.Today || ArchivoEvaluacion.Firma(LeerDatos(c, t, e.Solicitud.IdProducto, DateTime.Today)) != e.FirmaDatos)
                throw new InvalidOperationException("Los datos cambiaron o la evaluación es de otro día. Vuelva a comparar antes de confirmar o crear una orden.");
        }
        public void SeleccionarPrioritario(int id, int proveedor, string motivo, string responsable)
        {
            using (var c = Conexion.Instancia.ObtenerConexion())
            {
                c.Open(); using (var t = c.BeginTransaction(IsolationLevel.Serializable))
                {
                    var e = Leer(c, t, id, true);
                    if (e.IdProveedorSeleccionado.HasValue) throw new InvalidOperationException("La evaluación ya tiene una decisión. Cree otra comparación para cambiarla.");
                    if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 10 || motivo.Length > 500 || !e.Resultados.Any(r => r.IdProveedor == proveedor && r.Elegible))
                        throw new InvalidOperationException("Proveedor o fundamento de selección inválido.");
                    ComprobarVigencia(c, t, e);
                    using (var cmd = new SqlCommand(@"UPDATE dbo.EvaluacionProveedor SET IdProveedorSeleccionado=@p,MotivoSeleccion=@m,SeleccionadaPor=@r,FechaSeleccion=GETDATE() WHERE IdEvaluacion=@id", c, t))
                    { cmd.Parameters.AddWithValue("@p", proveedor); cmd.Parameters.AddWithValue("@m", motivo); cmd.Parameters.AddWithValue("@r", responsable); cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    t.Commit();
                }
            }
        }
        public int CrearOrdenBorrador(int id, string responsable)
        {
            using (var c = Conexion.Instancia.ObtenerConexion())
            {
                c.Open(); using (var t = c.BeginTransaction(IsolationLevel.Serializable))
                {
                    var e = Leer(c, t, id, true);
                    if (e.IdOrdenCompra.HasValue) { t.Commit(); return e.IdOrdenCompra.Value; }
                    if (!e.IdProveedorSeleccionado.HasValue) throw new InvalidOperationException("Primero confirme un proveedor.");
                    ComprobarVigencia(c, t, e);
                    var r = e.Resultados.Single(x => x.IdProveedor == e.IdProveedorSeleccionado.Value && x.Elegible);
                    var orden = new OrdenCompra { IdProveedor = r.IdProveedor, CreadaPor = responsable, Total = r.CostoTotal,
                        Detalles = new List<DetalleOrdenCompra> { new DetalleOrdenCompra { IdProducto = e.Solicitud.IdProducto, Cantidad = e.Solicitud.Cantidad, PrecioUnitario = r.PrecioUnitario } } };
                    int ordenId = AprovisionamientoDAL.InsertarOrden(c, t, orden);
                    using (var cmd = new SqlCommand("UPDATE dbo.EvaluacionProveedor SET IdOrdenCompra=@o WHERE IdEvaluacion=@id", c, t))
                    { cmd.Parameters.AddWithValue("@o", ordenId); cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    t.Commit(); return ordenId;
                }
            }
        }
    }
}
