using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BE.Interfaces;
using DAL;

namespace BLL
{
    public class ProductoBLL
    {
        private readonly IProductoRepository repositorio;
        private readonly IRegistroActividad registroActividad;

        public ProductoBLL()
            : this(ProductoDAL.Instancia, new RegistroActividadBitacora())
        {
        }

        public ProductoBLL(IProductoRepository repositorio, IRegistroActividad registroActividad)
        {
            this.repositorio = repositorio ?? throw new ArgumentNullException("repositorio");
            this.registroActividad = registroActividad ?? throw new ArgumentNullException("registroActividad");
        }

        public int Guardar(Producto producto)
        {
            Validar(producto);
            int id = repositorio.Guardar(producto);
            producto.IdProducto = id;
            registroActividad.Registrar("PRODUCTO_GUARDADO", "Producto registrado o actualizado: " + producto.Codigo + " - " + producto.Nombre + ".");
            return id;
        }

        public Producto Obtener(int idProducto)
        {
            if (idProducto <= 0) throw new ArgumentOutOfRangeException("idProducto");
            return repositorio.Obtener(idProducto);
        }

        public List<Producto> Listar(bool soloActivos = true)
        {
            return repositorio.Listar(soloActivos).ToList();
        }

        private static void Validar(Producto producto)
        {
            if (producto == null) throw new ArgumentNullException("producto");
            if (string.IsNullOrWhiteSpace(producto.Codigo)) throw new ArgumentException("El código del producto es obligatorio.");
            if (string.IsNullOrWhiteSpace(producto.Nombre)) throw new ArgumentException("El nombre del producto es obligatorio.");
            if (producto.PrecioVenta < 0) throw new ArgumentException("El precio de venta no puede ser negativo.");
            if (producto.StockMinimo < 0) throw new ArgumentException("El stock mínimo no puede ser negativo.");
        }
    }
}
