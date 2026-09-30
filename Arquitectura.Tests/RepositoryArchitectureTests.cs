using System;
using System.Linq;
using BE;
using DAL;
using DAL.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arquitectura.Tests
{
    [TestClass]
    public class RepositoryArchitectureTests
    {
        [TestMethod]
        public void RepositoryContracts_BelongToDAL_NotBE()
        {
            Type[] contracts = { typeof(IAprovisionamientoRepository), typeof(IInventarioRepository),
                typeof(IProductoRepository), typeof(IProveedorRepository), typeof(IVentaRepository) };
            foreach (var contract in contracts)
            {
                Assert.IsTrue(contract.IsInterface);
                Assert.AreEqual(typeof(ProductoDAL).Assembly, contract.Assembly);
                Assert.AreEqual("DAL.Interfaces", contract.Namespace);
            }
            Assert.IsFalse(typeof(Producto).Assembly.GetTypes()
                .Any(t => t.IsInterface && t.Name.EndsWith("Repository", StringComparison.Ordinal)));
        }

        [TestMethod]
        public void BE_DoesNotDependOnDAL_OrBLL()
        {
            var dependencies = typeof(Producto).Assembly.GetReferencedAssemblies();
            Assert.IsFalse(dependencies.Any(a => a.Name == "DAL" || a.Name == "BLL" || a.Name == "UI"));
        }

        [TestMethod]
        public void DALImplementations_RealizeTheirRepositoryContracts()
        {
            Assert.IsTrue(typeof(IAprovisionamientoRepository).IsAssignableFrom(typeof(AprovisionamientoDAL)));
            Assert.IsTrue(typeof(IInventarioRepository).IsAssignableFrom(typeof(InventarioDAL)));
            Assert.IsTrue(typeof(IProductoRepository).IsAssignableFrom(typeof(ProductoDAL)));
            Assert.IsTrue(typeof(IProveedorRepository).IsAssignableFrom(typeof(ProveedorDAL)));
            Assert.IsTrue(typeof(IVentaRepository).IsAssignableFrom(typeof(VentaDAL)));
        }
    }
}
