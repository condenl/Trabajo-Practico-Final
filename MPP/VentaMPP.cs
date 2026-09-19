using System;
using System.Collections.Generic;
using System.Data;
using BE;

namespace MPP
{
    public class VentaMPP
    {
        private static VentaMPP instancia;
        private static readonly object bloqueo = new object();

        private VentaMPP() { }

        public static VentaMPP Instancia
        {
            get
            {
                lock (bloqueo)
                {
                    if (instancia == null) instancia = new VentaMPP();
                    return instancia;
                }
            }
        }

        public Cliente MapearCliente(DataRow row)
        {
            if (row == null) return null;
            return new Cliente
            {
                IdCliente = Convert.ToInt32(row["IdCliente"]),
                Nombre = row["Nombre"].ToString(),
                Apellido = row["Apellido"].ToString(),
                Email = row["Email"].ToString(),
                Telefono = row["Telefono"].ToString(),
                PreferenciasOlfativas = row["PreferenciasOlfativas"].ToString(),
                AutorizaMarketing = Convert.ToBoolean(row["AutorizaMarketing"]),
                FechaAlta = Convert.ToDateTime(row["FechaAlta"]),
                Activo = Convert.ToBoolean(row["Activo"])
            };
        }

        public Membresia MapearMembresia(DataRow row)
        {
            if (row == null) return null;
            return new Membresia
            {
                IdMembresia = Convert.ToInt32(row["IdMembresia"]),
                IdCliente = Convert.ToInt32(row["IdCliente"]),
                Numero = row["Numero"].ToString(),
                FechaAlta = Convert.ToDateTime(row["FechaAlta"]),
                PorcentajeDescuento = Convert.ToDecimal(row["PorcentajeDescuento"]),
                Activa = Convert.ToBoolean(row["Activa"])
            };
        }

        public IList<Venta> MapearVentas(DataTable tabla)
        {
            List<Venta> ventas = new List<Venta>();
            foreach (DataRow row in tabla.Rows)
            {
                ventas.Add(new Venta
                {
                    IdVenta = Convert.ToInt32(row["IdVenta"]),
                    IdCliente = row["IdCliente"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["IdCliente"]),
                    IdUsuarioVendedor = row["IdUsuarioVendedor"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["IdUsuarioVendedor"]),
                    Fecha = Convert.ToDateTime(row["Fecha"]),
                    Subtotal = Convert.ToDecimal(row["Subtotal"]),
                    Descuento = Convert.ToDecimal(row["Descuento"]),
                    Total = Convert.ToDecimal(row["Total"])
                });
            }
            return ventas;
        }
    }
}
