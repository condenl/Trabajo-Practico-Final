using System;
using System.Collections.Generic;
using BE;
using BE.Interfaces;
using BLL;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arquitectura.Tests
{
    [TestClass]
    public class InventarioBLLTests
    {
        [TestMethod]
        public void RegistrarIngresoLote_ExigeOrigenYCertificado()
        {
            FakeInventarioRepository repositorio = new FakeInventarioRepository();
            InventarioBLL servicio = new InventarioBLL(repositorio, new FakeRegistroActividad());
            Lote lote = new Lote
            {
                IdProducto = 1,
                IdProveedor = 1,
                CodigoLote = "L-1",
                Origen = "Francia",
                CantidadIngresada = 2
            };

            Assert.Throws<ArgumentException>(() => servicio.RegistrarIngresoLote(lote));
        }

        [TestMethod]
        public void DescontarStockLote_NoPermiteStockNegativo()
        {
            FakeInventarioRepository repositorio = new FakeInventarioRepository { DescuentoExitoso = false };
            InventarioBLL servicio = new InventarioBLL(repositorio, new FakeRegistroActividad());

            Assert.Throws<InvalidOperationException>(() => servicio.DescontarStockLote(1, 5));
        }
    }

    internal sealed class FakeInventarioRepository : IInventarioRepository
    {
        public bool DescuentoExitoso { get; set; }

        public int VerificarStock(int idProducto) { return 0; }

        public int RegistrarIngresoLote(Lote lote, string responsable, string referencia) { return 1; }

        public bool DescontarStockLote(int idLote, int cantidad, string responsable, string referencia)
        {
            return DescuentoExitoso;
        }

        public IList<Lote> ObtenerLotesDisponibles(int idProducto) { return new List<Lote>(); }

        public IList<Lote> ObtenerTrazabilidad(int idProducto) { return new List<Lote>(); }
    }
}
