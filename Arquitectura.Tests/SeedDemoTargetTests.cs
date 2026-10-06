using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arquitectura.Tests
{
    [TestClass]
    public class SeedDemoTargetTests
    {
        [TestMethod]
        [DataRow(@"(localdb)\MSSQLLocalDB", "different-metadata", true, "PC", true)]
        [DataRow(@" (LOCALDB)\MSSQLLocalDB ", null, true, "PC", true)]
        [DataRow(@"(localdb)\.\SharedDemo", "different-metadata", true, "PC", true)]
        [DataRow(@"(localdb)\", "PC", true, "PC", false)]
        [DataRow(@"(localdb)\MSSQLLocalDB", "REMOTE", false, "PC", false)]
        [DataRow(@"tcp:remote,1433", "REMOTE", true, "PC", false)]
        [DataRow(@"remote\SQL", "REMOTE", false, "PC", false)]
        [DataRow(@".\SQLEXPRESS", "pc", false, "PC", true)]
        [DataRow(".", "PC", false, "PC", true)]
        [DataRow(".", null, false, "PC", false)]
        public void AcceptOnlyLocalSqlOrConfirmedLocalDb(string source, string machine, bool isLocalDb, string client, bool expected)
        {
            Assert.AreEqual(expected, SeedDemoTargetPolicy.IsLocalTarget(source, machine, isLocalDb, client));
        }
    }
}
