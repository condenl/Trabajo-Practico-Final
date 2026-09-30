using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BE.Interfaces;
using DAL.Interfaces;
using BLL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arquitectura.Tests
{
    [TestClass]
    public class AprovisionamientoBLLTests
    {
        [TestMethod]
        public void CrearOrdenPreliminar_RecalculaTotalYLaDejaEnBorrador()
        {
            Fixture fixture = new Fixture();
            OrdenCompra orden = fixture.CrearOrdenBase();

            bool creada = fixture.Servicio.CrearOrdenPreliminar(orden, orden.Detalles);

            Assert.IsTrue(creada);
            Assert.AreEqual(1, orden.IdOrdenCompra);
            Assert.AreEqual(EstadoOrdenCompra.Borrador, orden.Estado);
            Assert.AreEqual(250m, orden.Total);
        }

        [TestMethod]
        public void EnviarOrdenARevisionContable_ExigeComparativaDeProveedores()
        {
            Fixture fixture = new Fixture();
            OrdenCompra orden = fixture.CrearYGuardarOrden();
            fixture.Aprovisionamiento.CotizacionesPorProducto[1] = 1;

            InvalidOperationException error = Assert.Throws<InvalidOperationException>(
                () => fixture.Servicio.EnviarOrdenARevisionContable(orden.IdOrdenCompra));

            StringAssert.Contains(error.Message, "dos cotizaciones");
            Assert.AreEqual(EstadoOrdenCompra.Borrador, orden.Estado);
        }

        [TestMethod]
        public void AprobarOrdenCompra_RespetaLaRevisionContablePrevia()
        {
            Fixture fixture = new Fixture();
            OrdenCompra orden = fixture.CrearYGuardarOrden();
            fixture.Aprovisionamiento.CotizacionesPorProducto[1] = 2;

            fixture.Servicio.EnviarOrdenARevisionContable(orden.IdOrdenCompra);
            fixture.Servicio.AprobarOrdenCompra(orden.IdOrdenCompra, 10);

            Assert.AreEqual(EstadoOrdenCompra.Aprobada, orden.Estado);
            Assert.AreEqual(10, orden.IdUsuarioAprobadorContable);
        }

        [TestMethod]
        public void RecepcionarMercaderia_RechazaOrdenNoEmitida()
        {
            Fixture fixture = new Fixture();
            OrdenCompra orden = fixture.CrearYGuardarOrden();

            Assert.Throws<InvalidOperationException>(
                () => fixture.Servicio.RecepcionarMercaderia(orden.IdOrdenCompra, new List<Lote> { fixture.CrearLote(5) }));
        }

        [TestMethod]
        public void RecepcionarMercaderia_RegistraTrazabilidadYCompletaLaOrden()
        {
            Fixture fixture = new Fixture();
            OrdenCompra orden = fixture.CrearYGuardarOrden();
            fixture.Aprovisionamiento.CotizacionesPorProducto[1] = 2;
            fixture.Servicio.EnviarOrdenARevisionContable(orden.IdOrdenCompra);
            fixture.Servicio.AprobarOrdenCompra(orden.IdOrdenCompra, 10);
            fixture.Servicio.EmitirOrdenAprobada(orden.IdOrdenCompra);

            Lote lote = fixture.CrearLote(5);
            fixture.Servicio.RecepcionarMercaderia(orden.IdOrdenCompra, new List<Lote> { lote });

            Assert.AreEqual(EstadoOrdenCompra.Recibida, orden.Estado);
            Assert.AreEqual(1, lote.IdProveedor);
            Assert.AreEqual(orden.IdOrdenCompra, lote.IdOrdenCompra);
            Assert.AreEqual(5, lote.CantidadDisponible);
            Assert.AreEqual(5, orden.Detalles[0].CantidadRecibida);
        }

        [TestMethod]
        public void RechazarOrdenCompra_ExigeMotivoContable()
        {
            Fixture fixture = new Fixture();
            OrdenCompra orden = fixture.CrearYGuardarOrden();
            fixture.Aprovisionamiento.CotizacionesPorProducto[1] = 2;
            fixture.Servicio.EnviarOrdenARevisionContable(orden.IdOrdenCompra);

            Assert.Throws<ArgumentException>(
                () => fixture.Servicio.RechazarOrdenCompra(orden.IdOrdenCompra, " "));
        }

        private sealed class Fixture
        {
            public readonly FakeAprovisionamientoRepository Aprovisionamiento = new FakeAprovisionamientoRepository();
            public readonly FakeProveedorRepository Proveedores = new FakeProveedorRepository();
            public readonly FakeProductoRepository Productos = new FakeProductoRepository();
            public readonly FakeRegistroActividad Registro = new FakeRegistroActividad();
            public readonly AprovisionamientoBLL Servicio;

            public Fixture()
            {
                Proveedores.Datos[1] = new Proveedor { IdProveedor = 1, RazonSocial = "Distribuidora Uno", Cuit = "30711111111", Activo = true };
                Productos.Datos[1] = new Producto { IdProducto = 1, Codigo = "PERF-1", Nombre = "Fragancia", Activo = true };
                Servicio = new AprovisionamientoBLL(Aprovisionamiento, Proveedores, Productos, Registro);
            }

            public OrdenCompra CrearOrdenBase()
            {
                return new OrdenCompra
                {
                    IdProveedor = 1,
                    CreadaPor = "compras@perfumeria.test",
                    Detalles = new List<DetalleOrdenCompra>
                    {
                        new DetalleOrdenCompra { IdProducto = 1, Cantidad = 5, PrecioUnitario = 50m }
                    }
                };
            }

            public OrdenCompra CrearYGuardarOrden()
            {
                OrdenCompra orden = CrearOrdenBase();
                Servicio.CrearOrdenPreliminar(orden, orden.Detalles);
                return orden;
            }

            public Lote CrearLote(int cantidad)
            {
                return new Lote
                {
                    IdProducto = 1,
                    CodigoLote = "LOTE-001",
                    Origen = "Francia",
                    CertificadoOrigen = "CERT-001",
                    FechaVencimiento = DateTime.Today.AddYears(2),
                    CantidadIngresada = cantidad
                };
            }
        }
    }

    internal sealed class FakeAprovisionamientoRepository : IAprovisionamientoRepository
    {
        private int siguienteOrden = 1;
        private int siguienteCotizacion = 1;
        public readonly Dictionary<int, OrdenCompra> Ordenes = new Dictionary<int, OrdenCompra>();
        public readonly Dictionary<int, int> CotizacionesPorProducto = new Dictionary<int, int>();

        public int InsertarCotizacion(CotizacionProveedor cotizacion)
        {
            return siguienteCotizacion++;
        }

        public int InsertarOrden(OrdenCompra orden)
        {
            int id = siguienteOrden++;
            orden.IdOrdenCompra = id;
            Ordenes[id] = orden;
            return id;
        }

        public OrdenCompra ObtenerOrden(int idOrdenCompra)
        {
            return Ordenes.ContainsKey(idOrdenCompra) ? Ordenes[idOrdenCompra] : null!;
        }

        public bool ActualizarEstado(int idOrdenCompra, EstadoOrdenCompra estadoEsperado, EstadoOrdenCompra nuevoEstado, int? idUsuarioContable, string motivoRechazo)
        {
            OrdenCompra orden = ObtenerOrden(idOrdenCompra);
            if (orden == null || orden.Estado != estadoEsperado) return false;
            orden.Estado = nuevoEstado;
            orden.MotivoRechazo = motivoRechazo;
            if (idUsuarioContable.HasValue) orden.IdUsuarioAprobadorContable = idUsuarioContable;
            if (nuevoEstado == EstadoOrdenCompra.Aprobada) orden.FechaAprobacion = DateTime.Now;
            return true;
        }

        public IDictionary<int, int> ContarProveedoresCotizadosPorProducto(IList<int> idsProducto, DateTime fechaReferencia)
        {
            return CotizacionesPorProducto
                .Where(x => idsProducto.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public IList<OrdenCompra> ListarOrdenes(int? idProveedor)
        {
            return Ordenes.Values.Where(o => !idProveedor.HasValue || o.IdProveedor == idProveedor.Value).ToList();
        }

        public void RegistrarRecepcion(int idOrdenCompra, IList<Lote> lotes, EstadoOrdenCompra estadoFinal, string responsable)
        {
            OrdenCompra orden = Ordenes[idOrdenCompra];
            foreach (Lote lote in lotes)
            {
                DetalleOrdenCompra detalle = orden.Detalles.Single(d => d.IdProducto == lote.IdProducto);
                detalle.CantidadRecibida += lote.CantidadIngresada;
            }
            orden.Estado = estadoFinal;
        }
    }

    internal sealed class FakeProveedorRepository : IProveedorRepository
    {
        public readonly Dictionary<int, Proveedor> Datos = new Dictionary<int, Proveedor>();

        public int Guardar(Proveedor proveedor)
        {
            if (proveedor.IdProveedor == 0) proveedor.IdProveedor = Datos.Count + 1;
            Datos[proveedor.IdProveedor] = proveedor;
            return proveedor.IdProveedor;
        }

        public Proveedor Obtener(int idProveedor)
        {
            return Datos.ContainsKey(idProveedor) ? Datos[idProveedor] : null!;
        }

        public IList<Proveedor> Listar(bool soloActivos)
        {
            return Datos.Values.Where(p => !soloActivos || p.Activo).ToList();
        }
    }

    internal sealed class FakeProductoRepository : IProductoRepository
    {
        public readonly Dictionary<int, Producto> Datos = new Dictionary<int, Producto>();

        public int Guardar(Producto producto)
        {
            if (producto.IdProducto == 0) producto.IdProducto = Datos.Count + 1;
            Datos[producto.IdProducto] = producto;
            return producto.IdProducto;
        }

        public Producto Obtener(int idProducto)
        {
            return Datos.ContainsKey(idProducto) ? Datos[idProducto] : null!;
        }

        public IList<Producto> Listar(bool soloActivos)
        {
            return Datos.Values.Where(p => !soloActivos || p.Activo).ToList();
        }
    }

    internal sealed class FakeRegistroActividad : IRegistroActividad
    {
        public readonly List<string> Actividades = new List<string>();

        public void Registrar(string actividad, string detalle)
        {
            Actividades.Add(actividad + ":" + detalle);
        }
    }
}
