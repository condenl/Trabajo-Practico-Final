using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class InventarioMPP
    {
        private static InventarioMPP instancia;
        private static readonly object bloqueo = new object();

        private InventarioMPP() { }

        public static InventarioMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new InventarioMPP();
                    return instancia;
                }
            }
        }

        public Lote MapearLote(DataRow row)
        {
            if (row == null) return null;

            return new Lote
            {
                IdLote = Convert.ToInt32(row["IdLote"]),
                IdProducto = Convert.ToInt32(row["IdProducto"]),
                IdProveedor = Convert.ToInt32(row["IdProveedor"]),
                IdOrdenCompra = row["IdOrdenCompra"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["IdOrdenCompra"]),
                CodigoLote = row["CodigoLote"].ToString(),
                Origen = row["Origen"].ToString(),
                CertificadoOrigen = row["CertificadoOrigen"].ToString(),
                FechaVencimiento = row["FechaVencimiento"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FechaVencimiento"]),
                FechaRecepcion = Convert.ToDateTime(row["FechaRecepcion"]),
                CantidadIngresada = Convert.ToInt32(row["CantidadIngresada"]),
                CantidadDisponible = Convert.ToInt32(row["CantidadDisponible"])
            };
        }

        public IList<Lote> MapearLotes(DataTable tabla)
        {
            List<Lote> lotes = new List<Lote>();
            foreach (DataRow row in tabla.Rows) lotes.Add(MapearLote(row));
            return lotes;
        }
    }
}
