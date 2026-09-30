using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BE.Interfaces;
using DAL.Interfaces;
using DAL;
using Servicios;
using BE.Seguridad;

namespace BLL
{
    public class VentaBLL
    {
        private readonly IVentaRepository ventaRepository;
        private readonly IProductoRepository productoRepository;
        private readonly IInventarioRepository inventarioRepository;
        private readonly IRegistroActividad registroActividad;

        public VentaBLL()
            : this(VentaDAL.Instancia, ProductoDAL.Instancia, InventarioDAL.Instancia, new RegistroActividadBitacora())
        {
        }

        public VentaBLL(
            IVentaRepository ventaRepository,
            IProductoRepository productoRepository,
            IInventarioRepository inventarioRepository,
            IRegistroActividad registroActividad)
        {
            this.ventaRepository = ventaRepository ?? throw new ArgumentNullException("ventaRepository");
            this.productoRepository = productoRepository ?? throw new ArgumentNullException("productoRepository");
            this.inventarioRepository = inventarioRepository ?? throw new ArgumentNullException("inventarioRepository");
            this.registroActividad = registroActividad ?? throw new ArgumentNullException("registroActividad");
        }

        public int RegistrarCliente(Cliente cliente, bool crearMembresia, decimal porcentajeDescuento = 10m)
        {
            ExigirPermisoVenta();
            ValidarCliente(cliente);
            if (ventaRepository.BuscarClientePorEmail(cliente.Email.Trim()) != null)
                throw new InvalidOperationException("Ya existe un cliente registrado con ese correo electrónico.");

            cliente.Email = cliente.Email.Trim().ToLowerInvariant();
            int idCliente = ventaRepository.GuardarCliente(cliente);
            cliente.IdCliente = idCliente;

            if (crearMembresia)
            {
                if (porcentajeDescuento < 0m || porcentajeDescuento > 30m)
                    throw new ArgumentException("El descuento de membresía debe estar entre 0 y 30 por ciento.");

                Membresia membresia = new Membresia
                {
                    IdCliente = idCliente,
                    Numero = "SOC-" + idCliente.ToString("D6"),
                    PorcentajeDescuento = porcentajeDescuento
                };
                membresia.IdMembresia = ventaRepository.CrearMembresia(membresia);
            }

            registroActividad.Registrar("CLIENTE_REGISTRADO", "Cliente " + idCliente + " registrado para fidelización.");
            return idCliente;
        }

        public Cliente BuscarClientePorEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return ventaRepository.BuscarClientePorEmail(email.Trim().ToLowerInvariant());
        }

        public int RegistrarVenta(Venta venta)
        {
            ExigirPermisoVenta();
            if (venta == null) throw new ArgumentNullException("venta");
            if (venta.Detalles == null || venta.Detalles.Count == 0)
                throw new ArgumentException("La venta debe contener al menos un producto.");

            List<DetalleVenta> asignaciones = new List<DetalleVenta>();
            decimal porcentajeDescuento = 0m;
            if (venta.IdCliente.HasValue)
            {
                Membresia membresia = ventaRepository.ObtenerMembresiaActiva(venta.IdCliente.Value);
                if (membresia != null) porcentajeDescuento = membresia.PorcentajeDescuento;
            }

            foreach (IGrouping<int, DetalleVenta> grupo in venta.Detalles.GroupBy(d => d.IdProducto))
            {
                int cantidadSolicitada = grupo.Sum(d => d.Cantidad);
                if (grupo.Key <= 0 || cantidadSolicitada <= 0)
                    throw new ArgumentException("Cada detalle debe indicar un producto y una cantidad válida.");

                Producto producto = productoRepository.Obtener(grupo.Key);
                if (producto == null || !producto.Activo)
                    throw new InvalidOperationException("El producto " + grupo.Key + " no existe o está inactivo.");

                IList<Lote> lotes = inventarioRepository.ObtenerLotesDisponibles(grupo.Key);
                if (lotes.Sum(l => l.CantidadDisponible) < cantidadSolicitada)
                    throw new InvalidOperationException("No hay stock suficiente para el producto " + producto.Nombre + ".");

                int pendiente = cantidadSolicitada;
                foreach (Lote lote in lotes)
                {
                    if (pendiente == 0) break;
                    int cantidadLote = Math.Min(pendiente, lote.CantidadDisponible);
                    decimal descuentoUnitario = Math.Round(producto.PrecioVenta * porcentajeDescuento / 100m, 2, MidpointRounding.AwayFromZero);
                    asignaciones.Add(new DetalleVenta
                    {
                        IdProducto = producto.IdProducto,
                        IdLote = lote.IdLote,
                        Cantidad = cantidadLote,
                        PrecioUnitario = producto.PrecioVenta,
                        DescuentoUnitario = descuentoUnitario
                    });
                    pendiente -= cantidadLote;
                }
            }

            venta.Detalles = asignaciones;
            venta.Subtotal = asignaciones.Sum(d => d.Cantidad * d.PrecioUnitario);
            venta.Descuento = asignaciones.Sum(d => d.Cantidad * d.DescuentoUnitario);
            venta.Total = venta.Subtotal - venta.Descuento;
            if (venta.Fecha == DateTime.MinValue) venta.Fecha = DateTime.Now;
            if (!venta.IdUsuarioVendedor.HasValue && Session.Instance.Usuario != null)
                venta.IdUsuarioVendedor = Session.Instance.Usuario.Id;

            int idVenta = ventaRepository.RegistrarVenta(venta, ObtenerResponsable());
            venta.IdVenta = idVenta;
            registroActividad.Registrar("VENTA_REGISTRADA", "Venta " + idVenta + " registrada por " + venta.Total.ToString("0.00") + ".");
            return idVenta;
        }

        public List<Venta> ConsultarHistorialCliente(int idCliente)
        {
            if (idCliente <= 0) throw new ArgumentOutOfRangeException("idCliente");
            return ventaRepository.ListarVentasCliente(idCliente).ToList();
        }

        private static void ValidarCliente(Cliente cliente)
        {
            if (cliente == null) throw new ArgumentNullException("cliente");
            if (string.IsNullOrWhiteSpace(cliente.Nombre)) throw new ArgumentException("El nombre del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(cliente.Apellido)) throw new ArgumentException("El apellido del cliente es obligatorio.");
            if (string.IsNullOrWhiteSpace(cliente.Email) || !cliente.Email.Contains("@"))
                throw new ArgumentException("El correo electrónico del cliente no es válido.");
        }

        private static string ObtenerResponsable()
        {
            return Session.Instance.Usuario != null ? Session.Instance.Usuario.Email : "SISTEMA";
        }

        private static void ExigirPermisoVenta()
        {
            Usuario usuario = Session.Instance.Usuario;
            if (usuario != null && !usuario.TienePermiso(TipoPermiso.Ventas_Registrar))
                throw new UnauthorizedAccessException("El usuario actual no posee permiso para registrar ventas o clientes.");
        }
    }
}
