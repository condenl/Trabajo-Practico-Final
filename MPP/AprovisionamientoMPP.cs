using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BE;

namespace MPP
{
    public class AprovisionamientoMPP
    {
        private static AprovisionamientoMPP instancia;
        private static readonly object bloqueo = new object();

        private AprovisionamientoMPP() { }

        public static AprovisionamientoMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new AprovisionamientoMPP();
                    return instancia;
                }
            }
        }

        public OrdenCompra MapearOrden(DataRow row, IEnumerable<DataRow> filasDetalle)
        {
            if (row == null) return null;

            OrdenCompra orden = new OrdenCompra
            {
                IdOrdenCompra = Convert.ToInt32(row["IdOrdenCompra"]),
                IdProveedor = Convert.ToInt32(row["IdProveedor"]),
                FechaEmision = Convert.ToDateTime(row["FechaEmision"]),
                Total = Convert.ToDecimal(row["Total"]),
                Estado = (EstadoOrdenCompra)Convert.ToInt32(row["Estado"]),
                IdUsuarioAprobadorContable = row["IdUsuarioAprobadorContable"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["IdUsuarioAprobadorContable"]),
                FechaAprobacion = row["FechaAprobacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FechaAprobacion"]),
                MotivoRechazo = row["MotivoRechazo"] == DBNull.Value ? null : row["MotivoRechazo"].ToString(),
                CreadaPor = row["CreadaPor"].ToString()
            };

            foreach (DataRow detalle in filasDetalle)
            {
                orden.Detalles.Add(new DetalleOrdenCompra
                {
                    IdDetalle = Convert.ToInt32(detalle["IdDetalle"]),
                    IdOrdenCompra = Convert.ToInt32(detalle["IdOrdenCompra"]),
                    IdProducto = Convert.ToInt32(detalle["IdProducto"]),
                    Cantidad = Convert.ToInt32(detalle["Cantidad"]),
                    CantidadRecibida = Convert.ToInt32(detalle["CantidadRecibida"]),
                    PrecioUnitario = Convert.ToDecimal(detalle["PrecioUnitario"])
                });
            }

            return orden;
        }

        public IList<OrdenCompra> MapearOrdenes(DataTable tablaOrdenes, DataTable tablaDetalles)
        {
            List<OrdenCompra> ordenes = new List<OrdenCompra>();
            foreach (DataRow row in tablaOrdenes.Rows)
            {
                int id = Convert.ToInt32(row["IdOrdenCompra"]);
                IEnumerable<DataRow> detalles = tablaDetalles.AsEnumerable()
                    .Where(d => Convert.ToInt32(d["IdOrdenCompra"]) == id);
                ordenes.Add(MapearOrden(row, detalles));
            }
            return ordenes;
        }
    }
}
