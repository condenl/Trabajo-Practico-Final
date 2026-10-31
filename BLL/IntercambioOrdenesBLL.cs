using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BE.Interfaces;
using BE.Seguridad;
using DAL;
using DAL.Interfaces;
using Servicios;

namespace BLL
{
    public class IntercambioOrdenesBLL
    {
        private readonly IAprovisionamientoRepository ordenes;
        private readonly IProveedorRepository proveedores;
        private readonly IProductoRepository productos;
        private readonly IRegistroActividad registro;
        private readonly Func<Usuario> obtenerUsuario;

        public IntercambioOrdenesBLL() : this(AprovisionamientoDAL.Instancia, ProveedorDAL.Instancia,
            ProductoDAL.Instancia, new RegistroActividadBitacora(), () => Session.Instance.Usuario) { }

        public IntercambioOrdenesBLL(IAprovisionamientoRepository ordenes, IProveedorRepository proveedores,
            IProductoRepository productos, IRegistroActividad registro, Func<Usuario> obtenerUsuario)
        {
            this.ordenes = ordenes ?? throw new ArgumentNullException("ordenes");
            this.proveedores = proveedores ?? throw new ArgumentNullException("proveedores");
            this.productos = productos ?? throw new ArgumentNullException("productos");
            this.registro = registro ?? throw new ArgumentNullException("registro");
            this.obtenerUsuario = obtenerUsuario ?? throw new ArgumentNullException("obtenerUsuario");
        }

        public ArchivoOrdenesCompra ObtenerListado(IEnumerable<int> identificadores)
        {
            ExigirActor();
            if (identificadores == null) throw new ArgumentNullException("identificadores");
            var ids = identificadores.ToList();
            if (ids.Count == 0 || ids.Count > OrdenesXml.MaximoOrdenes || ids.Any(id => id <= 0) || ids.Distinct().Count() != ids.Count)
                throw new ArgumentException("El listado debe contener entre 1 y 1000 órdenes distintas.");
            var todas = ordenes.ListarOrdenes(null).ToDictionary(o => o.IdOrdenCompra);
            var catalogoProveedores = proveedores.Listar(false).ToDictionary(p => p.IdProveedor);
            var catalogoProductos = productos.Listar(false).ToDictionary(p => p.IdProducto);
            var archivo = new ArchivoOrdenesCompra { Version = 1, FechaExportacionUtc = DateTime.UtcNow };
            foreach (int id in ids)
            {
                OrdenCompra orden;
                if (!todas.TryGetValue(id, out orden)) throw new InvalidOperationException("La orden " + id + " ya no está disponible. Actualice la grilla.");
                Proveedor proveedor;
                if (!catalogoProveedores.TryGetValue(orden.IdProveedor, out proveedor)) throw new InvalidOperationException("No se encontró el proveedor de la orden " + id + ".");
                var exportada = new OrdenCompraArchivo { IdOrdenOrigen = id, ProveedorCuit = proveedor.Cuit,
                    ProveedorNombre = proveedor.RazonSocial, FechaEmision = orden.FechaEmision, Estado = orden.Estado,
                    Total = orden.Total, CreadaPor = orden.CreadaPor, IdUsuarioAprobadorContable = orden.IdUsuarioAprobadorContable,
                    FechaAprobacion = orden.FechaAprobacion, MotivoRechazo = orden.MotivoRechazo };
                foreach (DetalleOrdenCompra detalle in orden.Detalles)
                {
                    Producto producto;
                    if (!catalogoProductos.TryGetValue(detalle.IdProducto, out producto)) throw new InvalidOperationException("No se encontró el producto " + detalle.IdProducto + ".");
                    exportada.Detalles.Add(new DetalleOrdenArchivo { ProductoCodigo = producto.Codigo, ProductoNombre = producto.Nombre,
                        Cantidad = detalle.Cantidad, CantidadRecibida = detalle.CantidadRecibida, PrecioUnitario = detalle.PrecioUnitario });
                }
                archivo.Ordenes.Add(exportada);
            }
            OrdenesXml.Validar(archivo);
            return archivo;
        }

        public int ValidarImportacion(ArchivoOrdenesCompra archivo)
        {
            return PrepararImportacion(archivo).Count;
        }

        public List<int> Importar(ArchivoOrdenesCompra archivo)
        {
            // Revalidate after the confirmation dialog; never persist a previously trusted preview.
            List<OrdenCompra> nuevas = PrepararImportacion(archivo);
            var ids = ordenes.InsertarOrdenes(nuevas).ToList();
            registro.Registrar("ORDENES_IMPORTADAS_XML", "Se crearon " + ids.Count + " borradores por " + ExigirActor().Email +
                ". Órdenes de origen: " + string.Join(", ", archivo.Ordenes.Select(o => o.IdOrdenOrigen)) + "; nuevas: " + string.Join(", ", ids) + ".");
            return ids;
        }

        private List<OrdenCompra> PrepararImportacion(ArchivoOrdenesCompra archivo)
        {
            Usuario actor = ExigirActor();
            OrdenesXml.Validar(archivo);
            var catalogoProveedores = proveedores.Listar(true).ToDictionary(p => p.Cuit.Trim(), StringComparer.OrdinalIgnoreCase);
            var catalogoProductos = productos.Listar(true).ToDictionary(p => p.Codigo.Trim(), StringComparer.OrdinalIgnoreCase);
            var nuevas = new List<OrdenCompra>();
            foreach (OrdenCompraArchivo origen in archivo.Ordenes)
            {
                Proveedor proveedor;
                if (!catalogoProveedores.TryGetValue(origen.ProveedorCuit, out proveedor) || !proveedor.Activo)
                    throw new InvalidOperationException("No existe un proveedor activo con CUIT " + origen.ProveedorCuit + ". No se importó ninguna orden.");
                var nueva = new OrdenCompra { IdProveedor = proveedor.IdProveedor, FechaEmision = DateTime.Now,
                    Estado = EstadoOrdenCompra.Borrador, CreadaPor = actor.Email };
                foreach (DetalleOrdenArchivo detalle in origen.Detalles)
                {
                    Producto producto;
                    if (!catalogoProductos.TryGetValue(detalle.ProductoCodigo.Trim(), out producto) || !producto.Activo)
                        throw new InvalidOperationException("No existe un producto activo con código " + detalle.ProductoCodigo + ". No se importó ninguna orden.");
                    nueva.Detalles.Add(new DetalleOrdenCompra { IdProducto = producto.IdProducto, Cantidad = detalle.Cantidad,
                        CantidadRecibida = 0, PrecioUnitario = detalle.PrecioUnitario });
                }
                // IDs, prior approval, receipts, author and supplied total confer no authority in this database.
                nueva.Total = nueva.Detalles.Sum(d => d.Subtotal);
                nuevas.Add(nueva);
            }
            return nuevas;
        }

        private Usuario ExigirActor()
        {
            Usuario actor = obtenerUsuario();
            if (actor == null || !actor.Activo || !actor.TienePermiso(TipoPermiso.Aprovisionamiento_Gestion))
                throw new UnauthorizedAccessException("Se requiere una sesión con permiso de gestión de aprovisionamiento.");
            if (string.IsNullOrWhiteSpace(actor.Email) || actor.Email.Length > 200)
                throw new InvalidOperationException("La cuenta debe tener un correo válido para identificar al responsable.");
            return actor;
        }
    }
}
