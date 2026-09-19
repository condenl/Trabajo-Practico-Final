using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class ProveedorMPP
    {
        private static ProveedorMPP instancia;
        private static readonly object bloqueo = new object();

        private ProveedorMPP() { }

        public static ProveedorMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new ProveedorMPP();
                    return instancia;
                }
            }
        }

        public Proveedor Mapear(DataRow row)
        {
            if (row == null) return null;

            return new Proveedor
            {
                IdProveedor = Convert.ToInt32(row["IdProveedor"]),
                RazonSocial = row["RazonSocial"].ToString(),
                Cuit = row["Cuit"].ToString(),
                Contacto = row["Contacto"].ToString(),
                AntiguedadMercado = Convert.ToInt32(row["AntiguedadMercado"]),
                IndicePrestigio = Convert.ToDecimal(row["IndicePrestigio"]),
                Activo = Convert.ToBoolean(row["Activo"])
            };
        }

        public IList<Proveedor> MapearLista(DataTable tabla)
        {
            List<Proveedor> proveedores = new List<Proveedor>();
            foreach (DataRow row in tabla.Rows) proveedores.Add(Mapear(row));
            return proveedores;
        }
    }
}
