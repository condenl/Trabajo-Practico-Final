using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class ProductoMPP
    {
        private static ProductoMPP instancia;
        private static readonly object bloqueo = new object();

        private ProductoMPP() { }

        public static ProductoMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new ProductoMPP();
                    return instancia;
                }
            }
        }

        public Producto Mapear(DataRow row)
        {
            if (row == null) return null;

            return new Producto
            {
                IdProducto = Convert.ToInt32(row["IdProducto"]),
                Codigo = row["Codigo"].ToString(),
                Nombre = row["Nombre"].ToString(),
                Marca = row["Marca"].ToString(),
                FamiliaOlfativa = row["FamiliaOlfativa"].ToString(),
                PrecioVenta = Convert.ToDecimal(row["PrecioVenta"]),
                StockMinimo = Convert.ToInt32(row["StockMinimo"]),
                Activo = Convert.ToBoolean(row["Activo"])
            };
        }

        public IList<Producto> MapearLista(DataTable tabla)
        {
            List<Producto> productos = new List<Producto>();
            foreach (DataRow row in tabla.Rows) productos.Add(Mapear(row));
            return productos;
        }
    }
}
