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
    public class InventarioBLL
    {
        private readonly IInventarioRepository repositorio;
        private readonly IRegistroActividad registroActividad;

        public InventarioBLL()
            : this(InventarioDAL.Instancia, new RegistroActividadBitacora())
        {
        }

        public InventarioBLL(IInventarioRepository repositorio, IRegistroActividad registroActividad)
        {
            this.repositorio = repositorio ?? throw new ArgumentNullException("repositorio");
            this.registroActividad = registroActividad ?? throw new ArgumentNullException("registroActividad");
        }

        public int VerificarStock(int idProducto)
        {
            if (idProducto <= 0) throw new ArgumentOutOfRangeException("idProducto");
            return repositorio.VerificarStock(idProducto);
        }

        public bool RegistrarIngresoLote(Lote lote)
        {
            ExigirPermisoRecepcion();
            ValidarLote(lote);
            lote.CantidadDisponible = lote.CantidadIngresada;
            if (lote.FechaRecepcion == DateTime.MinValue) lote.FechaRecepcion = DateTime.Now;

            string responsable = ObtenerResponsable();
            int id = repositorio.RegistrarIngresoLote(lote, responsable, "Ingreso manual de inventario");
            lote.IdLote = id;
            registroActividad.Registrar("INGRESO_LOTE", "Ingreso del lote " + lote.CodigoLote + " para el producto " + lote.IdProducto + ".");
            return id > 0;
        }

        public bool DescontarStockLote(int idLote, int cantidad)
        {
            ExigirPermisoRecepcion();
            if (idLote <= 0) throw new ArgumentOutOfRangeException("idLote");
            if (cantidad <= 0) throw new ArgumentException("La cantidad a descontar debe ser mayor que cero.");

            bool actualizado = repositorio.DescontarStockLote(idLote, cantidad, ObtenerResponsable(), "Salida de inventario");
            if (!actualizado) throw new InvalidOperationException("El lote no existe o no posee stock suficiente.");

            registroActividad.Registrar("SALIDA_LOTE", "Salida de " + cantidad + " unidades del lote " + idLote + ".");
            return true;
        }

        public List<Lote> ObtenerLotesDisponibles(int idProducto)
        {
            if (idProducto <= 0) throw new ArgumentOutOfRangeException("idProducto");
            return repositorio.ObtenerLotesDisponibles(idProducto).ToList();
        }

        public List<Lote> ObtenerTrazabilidad(int idProducto)
        {
            if (idProducto <= 0) throw new ArgumentOutOfRangeException("idProducto");
            return repositorio.ObtenerTrazabilidad(idProducto).ToList();
        }

        internal static void ValidarLote(Lote lote)
        {
            if (lote == null) throw new ArgumentNullException("lote");
            if (lote.IdProducto <= 0) throw new ArgumentException("El lote debe indicar un producto válido.");
            if (lote.IdProveedor <= 0) throw new ArgumentException("El lote debe indicar el proveedor de origen.");
            if (string.IsNullOrWhiteSpace(lote.CodigoLote)) throw new ArgumentException("El código de lote es obligatorio.");
            if (string.IsNullOrWhiteSpace(lote.Origen)) throw new ArgumentException("El origen del lote es obligatorio.");
            if (string.IsNullOrWhiteSpace(lote.CertificadoOrigen)) throw new ArgumentException("El certificado de origen es obligatorio.");
            if (lote.CantidadIngresada <= 0) throw new ArgumentException("La cantidad ingresada debe ser mayor que cero.");
            if (lote.FechaVencimiento.HasValue && lote.FechaVencimiento.Value.Date <= DateTime.Today)
                throw new ArgumentException("No se puede ingresar un lote vencido o con vencimiento en la fecha actual.");
        }

        private static string ObtenerResponsable()
        {
            return Session.Instance.Usuario != null ? Session.Instance.Usuario.Email : "SISTEMA";
        }

        private static void ExigirPermisoRecepcion()
        {
            Usuario usuario = Session.Instance.Usuario;
            if (usuario != null && !usuario.TienePermiso(TipoPermiso.Inventario_Recepcionar))
                throw new UnauthorizedAccessException("El usuario actual no posee permiso para gestionar inventario.");
        }
    }
}
