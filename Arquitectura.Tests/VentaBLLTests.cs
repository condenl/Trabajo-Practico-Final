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
    public class VentaBLLTests
    {
        [TestMethod]
        public void RegistrarCliente_CreaMembresiaYPreferencias()
        {
            Fixture fixture = new Fixture();
            Cliente cliente = new Cliente
            {
                Nombre = "Ana",
                Apellido = "Pérez",
                Email = "ANA@EXAMPLE.COM",
                PreferenciasOlfativas = "floral",
                AutorizaMarketing = true
            };

            int id = fixture.Servicio.RegistrarCliente(cliente, true, 10m);

            Assert.AreEqual(1, id);
            Assert.AreEqual("ana@example.com", cliente.Email);
            Assert.AreEqual("SOC-000001", fixture.Ventas.Membresias.Single().Numero);
            Assert.IsTrue(fixture.Ventas.Clientes.Single().AutorizaMarketing);
        }

        [TestMethod]
        public void RegistrarCliente_RechazaCorreoDuplicado()
        {
            Fixture fixture = new Fixture();
            fixture.Servicio.RegistrarCliente(fixture.CrearCliente(), false);

            Assert.Throws<InvalidOperationException>(() => fixture.Servicio.RegistrarCliente(fixture.CrearCliente(), false));
        }

        [TestMethod]
        public void RegistrarVenta_MiembroAplicaDescuentoYAsignaLotesFefo()
        {
            Fixture fixture = new Fixture();
            int idCliente = fixture.Servicio.RegistrarCliente(fixture.CrearCliente(), true, 10m);
            Venta venta = new Venta
            {
                IdCliente = idCliente,
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = 1, Cantidad = 3 }
                }
            };

            int idVenta = fixture.Servicio.RegistrarVenta(venta);

            Assert.AreEqual(1, idVenta);
            Assert.AreEqual(300m, venta.Subtotal);
            Assert.AreEqual(30m, venta.Descuento);
            Assert.AreEqual(270m, venta.Total);
            Assert.AreEqual(2, venta.Detalles.Count);
            Assert.AreEqual(1, venta.Detalles[0].IdLote);
            Assert.AreEqual(2, venta.Detalles[0].Cantidad);
            Assert.AreEqual(2, venta.Detalles[1].IdLote);
        }

        [TestMethod]
        public void RegistrarVenta_RechazaStockInsuficiente()
        {
            Fixture fixture = new Fixture();
            Venta venta = new Venta
            {
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta { IdProducto = 1, Cantidad = 99 }
                }
            };

            Assert.Throws<InvalidOperationException>(() => fixture.Servicio.RegistrarVenta(venta));
            Assert.IsEmpty(fixture.Ventas.Ventas);
        }

        private sealed class Fixture
        {
            public readonly FakeVentaRepository Ventas = new FakeVentaRepository();
            public readonly FakeProductoRepository Productos = new FakeProductoRepository();
            public readonly FakeVentaInventarioRepository Inventario = new FakeVentaInventarioRepository();
            public readonly VentaBLL Servicio;

            public Fixture()
            {
                Productos.Datos[1] = new Producto
                {
                    IdProducto = 1,
                    Codigo = "P-001",
                    Nombre = "Perfume floral",
                    PrecioVenta = 100m,
                    Activo = true
                };
                Inventario.Lotes[1] = new List<Lote>
                {
                    new Lote { IdLote = 1, IdProducto = 1, CantidadDisponible = 2, FechaVencimiento = DateTime.Today.AddMonths(2) },
                    new Lote { IdLote = 2, IdProducto = 1, CantidadDisponible = 4, FechaVencimiento = DateTime.Today.AddMonths(8) }
                };
                Servicio = new VentaBLL(Ventas, Productos, Inventario, new FakeRegistroActividad());
            }

            public Cliente CrearCliente()
            {
                return new Cliente { Nombre = "Ana", Apellido = "Pérez", Email = "ana@example.com" };
            }
        }
    }

    internal sealed class FakeVentaRepository : IVentaRepository
    {
        public readonly List<Cliente> Clientes = new List<Cliente>();
        public readonly List<Membresia> Membresias = new List<Membresia>();
        public readonly List<Venta> Ventas = new List<Venta>();

        public Cliente BuscarClientePorEmail(string email)
        {
            return Clientes.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase))!;
        }

        public int GuardarCliente(Cliente cliente)
        {
            cliente.IdCliente = Clientes.Count + 1;
            Clientes.Add(cliente);
            return cliente.IdCliente;
        }

        public Membresia ObtenerMembresiaActiva(int idCliente)
        {
            return Membresias.FirstOrDefault(m => m.IdCliente == idCliente && m.Activa)!;
        }

        public int CrearMembresia(Membresia membresia)
        {
            membresia.IdMembresia = Membresias.Count + 1;
            Membresias.Add(membresia);
            return membresia.IdMembresia;
        }

        public int RegistrarVenta(Venta venta, string responsable)
        {
            venta.IdVenta = Ventas.Count + 1;
            Ventas.Add(venta);
            return venta.IdVenta;
        }

        public IList<Venta> ListarVentasCliente(int idCliente)
        {
            return Ventas.Where(v => v.IdCliente == idCliente).ToList();
        }
    }

    internal sealed class FakeVentaInventarioRepository : IInventarioRepository
    {
        public readonly Dictionary<int, List<Lote>> Lotes = new Dictionary<int, List<Lote>>();

        public int VerificarStock(int idProducto)
        {
            return ObtenerLotesDisponibles(idProducto).Sum(l => l.CantidadDisponible);
        }

        public int RegistrarIngresoLote(Lote lote, string responsable, string referencia) { return 1; }
        public bool DescontarStockLote(int idLote, int cantidad, string responsable, string referencia) { return true; }

        public IList<Lote> ObtenerLotesDisponibles(int idProducto)
        {
            return Lotes.ContainsKey(idProducto)
                ? Lotes[idProducto].OrderBy(l => l.FechaVencimiento).ToList()
                : new List<Lote>();
        }

        public IList<Lote> ObtenerTrazabilidad(int idProducto) { return ObtenerLotesDisponibles(idProducto); }
    }
}
