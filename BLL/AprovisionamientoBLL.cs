using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BE.Interfaces;
using DAL;
using Servicios;
using BE.Seguridad;

namespace BLL
{
    public class AprovisionamientoBLL
    {
        private const int CantidadMinimaCotizacionesComparables = 2;

        private readonly IAprovisionamientoRepository repositorio;
        private readonly IProveedorRepository proveedores;
        private readonly IProductoRepository productos;
        private readonly IRegistroActividad registroActividad;

        public AprovisionamientoBLL()
            : this(AprovisionamientoDAL.Instancia, ProveedorDAL.Instancia, ProductoDAL.Instancia, new RegistroActividadBitacora())
        {
        }

        public AprovisionamientoBLL(
            IAprovisionamientoRepository repositorio,
            IProveedorRepository proveedores,
            IProductoRepository productos,
            IRegistroActividad registroActividad)
        {
            this.repositorio = repositorio ?? throw new ArgumentNullException("repositorio");
            this.proveedores = proveedores ?? throw new ArgumentNullException("proveedores");
            this.productos = productos ?? throw new ArgumentNullException("productos");
            this.registroActividad = registroActividad ?? throw new ArgumentNullException("registroActividad");
        }

        public int RegistrarCotizacion(CotizacionProveedor cotizacion)
        {
            ExigirPermiso(TipoPermiso.Aprovisionamiento_Gestion);
            ValidarCotizacion(cotizacion);
            ValidarProveedorActivo(cotizacion.IdProveedor);

            foreach (DetalleCotizacionProveedor detalle in cotizacion.Detalles)
            {
                ValidarProductoActivo(detalle.IdProducto);
            }

            int id = repositorio.InsertarCotizacion(cotizacion);
            cotizacion.IdCotizacion = id;
            registroActividad.Registrar("COTIZACION_REGISTRADA", "Cotización " + id + " recibida del proveedor " + cotizacion.IdProveedor + ".");
            return id;
        }

        public bool CrearOrdenPreliminar(OrdenCompra orden, List<DetalleOrdenCompra> detalles)
        {
            ExigirPermiso(TipoPermiso.Aprovisionamiento_Gestion);
            if (orden == null) throw new ArgumentNullException("orden");
            if (detalles == null) throw new ArgumentNullException("detalles");

            orden.Detalles = detalles;
            ValidarOrdenPreliminar(orden);
            ValidarProveedorActivo(orden.IdProveedor);

            foreach (DetalleOrdenCompra detalle in orden.Detalles)
            {
                ValidarProductoActivo(detalle.IdProducto);
            }

            orden.Total = orden.Detalles.Sum(d => d.Subtotal);
            orden.Estado = EstadoOrdenCompra.Borrador;
            if (orden.FechaEmision == DateTime.MinValue) orden.FechaEmision = DateTime.Now;
            if (string.IsNullOrWhiteSpace(orden.CreadaPor)) orden.CreadaPor = ObtenerResponsable();

            int id = repositorio.InsertarOrden(orden);
            orden.IdOrdenCompra = id;
            registroActividad.Registrar("ORDEN_COMPRA_BORRADOR", "Orden preliminar " + id + " creada para el proveedor " + orden.IdProveedor + ".");
            return id > 0;
        }

        public bool EnviarOrdenARevisionContable(int idOrdenCompra)
        {
            ExigirPermiso(TipoPermiso.Aprovisionamiento_Gestion);
            OrdenCompra orden = ObtenerOrdenExistente(idOrdenCompra);
            ExigirEstado(orden, EstadoOrdenCompra.Borrador);

            IList<int> idsProducto = orden.Detalles.Select(d => d.IdProducto).Distinct().ToList();
            IDictionary<int, int> cantidadCotizaciones = repositorio.ContarProveedoresCotizadosPorProducto(idsProducto, DateTime.Today);

            List<int> sinComparativa = idsProducto
                .Where(id => !cantidadCotizaciones.ContainsKey(id) || cantidadCotizaciones[id] < CantidadMinimaCotizacionesComparables)
                .ToList();

            if (sinComparativa.Count > 0)
            {
                throw new InvalidOperationException(
                    "La orden no puede enviarse a Contabilidad porque faltan al menos dos cotizaciones vigentes para los productos: "
                    + string.Join(", ", sinComparativa));
            }

            CambiarEstado(orden, EstadoOrdenCompra.PendienteAprobacion, null, null);
            registroActividad.Registrar("ORDEN_ENVIADA_CONTABILIDAD", "Orden " + idOrdenCompra + " enviada a revisión presupuestaria.");
            return true;
        }

        public bool AprobarOrdenCompra(int idOrdenCompra, int idUsuarioContable)
        {
            ExigirPermiso(TipoPermiso.Contabilidad_AprobarCompras);
            if (idUsuarioContable <= 0) throw new ArgumentOutOfRangeException("idUsuarioContable");
            OrdenCompra orden = ObtenerOrdenExistente(idOrdenCompra);
            ExigirEstado(orden, EstadoOrdenCompra.PendienteAprobacion);

            CambiarEstado(orden, EstadoOrdenCompra.Aprobada, idUsuarioContable, null);
            registroActividad.Registrar("ORDEN_COMPRA_APROBADA", "Orden " + idOrdenCompra + " aprobada por Contabilidad.");
            return true;
        }

        public bool RechazarOrdenCompra(int idOrdenCompra, string motivo)
        {
            ExigirPermiso(TipoPermiso.Contabilidad_AprobarCompras);
            if (string.IsNullOrWhiteSpace(motivo)) throw new ArgumentException("Contabilidad debe indicar el motivo del rechazo.");
            OrdenCompra orden = ObtenerOrdenExistente(idOrdenCompra);
            ExigirEstado(orden, EstadoOrdenCompra.PendienteAprobacion);

            CambiarEstado(orden, EstadoOrdenCompra.Rechazada, null, motivo.Trim());
            registroActividad.Registrar("ORDEN_COMPRA_RECHAZADA", "Orden " + idOrdenCompra + " rechazada por Contabilidad. Motivo: " + motivo.Trim());
            return true;
        }

        public bool EmitirOrdenAprobada(int idOrdenCompra)
        {
            ExigirPermiso(TipoPermiso.Aprovisionamiento_Gestion);
            OrdenCompra orden = ObtenerOrdenExistente(idOrdenCompra);
            ExigirEstado(orden, EstadoOrdenCompra.Aprobada);

            CambiarEstado(orden, EstadoOrdenCompra.Emitida, orden.IdUsuarioAprobadorContable, null);
            registroActividad.Registrar("ORDEN_COMPRA_EMITIDA", "Orden " + idOrdenCompra + " emitida al proveedor " + orden.IdProveedor + ".");
            return true;
        }

        public bool RecepcionarMercaderia(int idOrdenCompra, List<Lote> lotesRecibidos)
        {
            ExigirPermiso(TipoPermiso.Inventario_Recepcionar);
            if (lotesRecibidos == null || lotesRecibidos.Count == 0)
                throw new ArgumentException("Debe indicar al menos un lote recibido.");

            OrdenCompra orden = ObtenerOrdenExistente(idOrdenCompra);
            if (orden.Estado != EstadoOrdenCompra.Emitida && orden.Estado != EstadoOrdenCompra.RecibidaParcial)
                throw new InvalidOperationException("Solo se puede recibir mercadería de una orden emitida o parcialmente recibida.");

            Dictionary<int, DetalleOrdenCompra> detallePorProducto = orden.Detalles.ToDictionary(d => d.IdProducto);
            Dictionary<int, int> cantidadNuevaPorProducto = lotesRecibidos
                .GroupBy(l => l.IdProducto)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.CantidadIngresada));

            foreach (Lote lote in lotesRecibidos)
            {
                lote.IdProveedor = orden.IdProveedor;
                lote.IdOrdenCompra = orden.IdOrdenCompra;
                InventarioBLL.ValidarLote(lote);

                if (!detallePorProducto.ContainsKey(lote.IdProducto))
                    throw new InvalidOperationException("El producto " + lote.IdProducto + " no pertenece a la orden de compra.");

                lote.CantidadDisponible = lote.CantidadIngresada;
                if (lote.FechaRecepcion == DateTime.MinValue) lote.FechaRecepcion = DateTime.Now;
            }

            foreach (KeyValuePair<int, int> recepcion in cantidadNuevaPorProducto)
            {
                DetalleOrdenCompra detalle = detallePorProducto[recepcion.Key];
                if (detalle.CantidadRecibida + recepcion.Value > detalle.Cantidad)
                    throw new InvalidOperationException("La recepción supera la cantidad ordenada para el producto " + recepcion.Key + ".");
            }

            bool recepcionCompleta = orden.Detalles.All(d =>
            {
                int nuevaCantidad = cantidadNuevaPorProducto.ContainsKey(d.IdProducto) ? cantidadNuevaPorProducto[d.IdProducto] : 0;
                return d.CantidadRecibida + nuevaCantidad == d.Cantidad;
            });

            EstadoOrdenCompra estadoFinal = recepcionCompleta
                ? EstadoOrdenCompra.Recibida
                : EstadoOrdenCompra.RecibidaParcial;

            repositorio.RegistrarRecepcion(idOrdenCompra, lotesRecibidos, estadoFinal, ObtenerResponsable());
            registroActividad.Registrar("RECEPCION_COMPRA", "Recepción de " + lotesRecibidos.Sum(l => l.CantidadIngresada) + " unidades para la orden " + idOrdenCompra + ". Estado: " + estadoFinal + ".");
            return true;
        }

        public List<OrdenCompra> ConsultarHistorialCompras(int? idProveedor)
        {
            if (idProveedor.HasValue && idProveedor.Value <= 0) throw new ArgumentOutOfRangeException("idProveedor");
            return repositorio.ListarOrdenes(idProveedor).ToList();
        }

        private void CambiarEstado(OrdenCompra orden, EstadoOrdenCompra nuevoEstado, int? idUsuarioContable, string motivo)
        {
            bool actualizado = repositorio.ActualizarEstado(orden.IdOrdenCompra, orden.Estado, nuevoEstado, idUsuarioContable, motivo);
            if (!actualizado) throw new InvalidOperationException("La orden cambió de estado durante la operación. Vuelva a consultar su estado.");
        }

        private OrdenCompra ObtenerOrdenExistente(int idOrdenCompra)
        {
            if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException("idOrdenCompra");
            OrdenCompra orden = repositorio.ObtenerOrden(idOrdenCompra);
            if (orden == null) throw new InvalidOperationException("No existe la orden de compra indicada.");
            return orden;
        }

        private static void ExigirEstado(OrdenCompra orden, EstadoOrdenCompra estadoEsperado)
        {
            if (orden.Estado != estadoEsperado)
                throw new InvalidOperationException("La orden debe estar en estado " + estadoEsperado + " y se encuentra en " + orden.Estado + ".");
        }

        private void ValidarProveedorActivo(int idProveedor)
        {
            Proveedor proveedor = proveedores.Obtener(idProveedor);
            if (proveedor == null || !proveedor.Activo) throw new InvalidOperationException("El proveedor no existe o se encuentra inactivo.");
        }

        private void ValidarProductoActivo(int idProducto)
        {
            Producto producto = productos.Obtener(idProducto);
            if (producto == null || !producto.Activo) throw new InvalidOperationException("El producto " + idProducto + " no existe o se encuentra inactivo.");
        }

        private static void ValidarCotizacion(CotizacionProveedor cotizacion)
        {
            if (cotizacion == null) throw new ArgumentNullException("cotizacion");
            if (cotizacion.IdProveedor <= 0) throw new ArgumentException("La cotización debe indicar un proveedor válido.");
            if (cotizacion.FechaVigencia.Date < DateTime.Today) throw new ArgumentException("La cotización ya se encuentra vencida.");
            if (cotizacion.Detalles == null || cotizacion.Detalles.Count == 0) throw new ArgumentException("La cotización debe contener al menos un producto.");
            if (cotizacion.Detalles.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1)) throw new ArgumentException("La cotización contiene productos duplicados.");

            foreach (DetalleCotizacionProveedor detalle in cotizacion.Detalles)
            {
                if (detalle.IdProducto <= 0) throw new ArgumentException("Cada detalle debe indicar un producto válido.");
                if (detalle.PrecioUnitario <= 0) throw new ArgumentException("El precio cotizado debe ser mayor que cero.");
                if (detalle.PlazoEntregaDias < 0) throw new ArgumentException("El plazo de entrega no puede ser negativo.");
            }
        }

        private static void ValidarOrdenPreliminar(OrdenCompra orden)
        {
            if (orden.IdProveedor <= 0) throw new ArgumentException("La orden debe indicar un proveedor válido.");
            if (orden.Detalles == null || orden.Detalles.Count == 0) throw new ArgumentException("La orden debe contener al menos un producto.");
            if (orden.Detalles.GroupBy(d => d.IdProducto).Any(g => g.Count() > 1)) throw new ArgumentException("La orden contiene productos duplicados.");

            foreach (DetalleOrdenCompra detalle in orden.Detalles)
            {
                if (detalle.IdProducto <= 0) throw new ArgumentException("Cada detalle debe indicar un producto válido.");
                if (detalle.Cantidad <= 0) throw new ArgumentException("La cantidad solicitada debe ser mayor que cero.");
                if (detalle.PrecioUnitario <= 0) throw new ArgumentException("El precio unitario debe ser mayor que cero.");
            }
        }

        private static string ObtenerResponsable()
        {
            return Session.Instance.Usuario != null ? Session.Instance.Usuario.Email : "SISTEMA";
        }

        private static void ExigirPermiso(TipoPermiso permiso)
        {
            Usuario usuario = Session.Instance.Usuario;
            if (usuario != null && !usuario.TienePermiso(permiso))
                throw new UnauthorizedAccessException("El usuario actual no posee el permiso requerido: " + permiso + ".");
        }
    }
}
