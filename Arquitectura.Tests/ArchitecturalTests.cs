using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BE;
using BLL;
using DAL;
using BE.Interfaces;

namespace Arquitectura.Tests
{
    [TestClass]
    public class ArchitecturalTests
    {
        [TestMethod]
        public void DAL_ShouldNotReference_UI_Or_BLL()
        {
            var dalAssembly = typeof(UsuarioDAL).Assembly;
            var referencedAssemblies = dalAssembly.GetReferencedAssemblies();

            bool referencesUI = referencedAssemblies.Any(a => a.Name == "UI");
            bool referencesBLL = referencedAssemblies.Any(a => a.Name == "BLL");

            Assert.IsFalse(referencesUI, "DAL assembly should not reference UI assembly.");
            Assert.IsFalse(referencesBLL, "DAL assembly should not reference BLL assembly.");
        }

        [TestMethod]
        public void BLL_ShouldNotReference_UI()
        {
            var bllAssembly = typeof(UsuarioBLL).Assembly;
            var referencedAssemblies = bllAssembly.GetReferencedAssemblies();

            bool referencesUI = referencedAssemblies.Any(a => a.Name == "UI");

            Assert.IsFalse(referencesUI, "BLL assembly should not reference UI assembly.");
        }

        [TestMethod]
        public void DALClasses_ShouldHave_PrivateConstructorAndSingleton()
        {
            var dalAssembly = typeof(UsuarioDAL).Assembly;
            var dalClasses = dalAssembly.GetTypes()
                .Where(t => t.IsClass && t.Name.EndsWith("DAL") && !t.IsAbstract)
                .ToList();

            foreach (var type in dalClasses)
            {
                // Check for private constructor
                var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var publicConstructors = constructors.Where(c => c.IsPublic).ToList();
                
                Assert.AreEqual(0, publicConstructors.Count, $"Class {type.Name} should not have public constructors.");

                // Check for static property or field named Instancia or similar
                var instanceProperty = type.GetProperty("Instancia", BindingFlags.Public | BindingFlags.Static);
                var instanceField = type.GetField("instancia", BindingFlags.NonPublic | BindingFlags.Static);

                Assert.IsNotNull(instanceProperty ?? (object)instanceField, $"Class {type.Name} should have a static 'Instancia' property or 'instancia' field for Singleton pattern.");
            }
        }

        [TestMethod]
        public void UsuarioBLL_ShouldImplement_IObservado()
        {
            var type = typeof(UsuarioBLL);
            bool implementsIObservado = typeof(IObservado).IsAssignableFrom(type);

            Assert.IsTrue(implementsIObservado, "UsuarioBLL should implement IObservado interface.");
        }

        [TestMethod]
        public void BitacoraBLL_ShouldImplement_IObserver()
        {
            var type = typeof(BitacoraBLL);
            bool implementsIObserver = typeof(IObserver).IsAssignableFrom(type);

            Assert.IsTrue(implementsIObserver, "BitacoraBLL should implement IObserver interface.");
        }
    }
}
