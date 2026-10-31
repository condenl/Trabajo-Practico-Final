// Integration test ONLY for a newly generated, isolated Perfumeria_Demo_Test_* database.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BE;
using BLL;

internal static class TestDemoScenarios
{
    private static SqlConnection connection;
    private static string database, seed;
    private static int assertions;
    private static readonly UsuarioBLL Users = new UsuarioBLL();
    private static readonly AprovisionamientoBLL Purchases = new AprovisionamientoBLL();
    private static readonly InventarioBLL Inventory = new InventarioBLL();
    private static readonly VentaBLL Sales = new VentaBLL();

    private static DataTable Query(string sql, params object[] args)
    {
        using (var c = new SqlCommand(sql, connection))
        {
            for (int i = 0; i < args.Length; i++) c.Parameters.AddWithValue("@p" + i, args[i]);
            using (var adapter = new SqlDataAdapter(c)) { var t = new DataTable("Row"); adapter.Fill(t); return t; }
        }
    }
    private static void Assert(bool ok, string message)
    {
        if (!ok) throw new Exception(message);
        assertions++;
        Console.WriteLine("PASS " + message);
    }
    private static void Reject<T>(Action action, string message) where T : Exception
    {
        try { action(); }
        catch (T) { Assert(true, message); return; }
        throw new Exception("No se rechazo: " + message);
    }
    private static int Id(string sql, object key) { return Convert.ToInt32(Query(sql, key).Rows[0][0]); }
    private static int Order(int number)
    {
        return Id("SELECT IdOrdenCompra FROM dbo.OrdenCompra WHERE CreadaPor=@p0", "DEMO-OC-" + number.ToString("D2") + " | demo.dueno@perfumeria.example");
    }
    private static void State(int number, EstadoOrdenCompra expected)
    {
        Assert(Purchases.ConsultarHistorialCompras(null).Single(o => o.IdOrdenCompra == Order(number)).Estado == expected,
            "DEMO-OC-" + number.ToString("D2") + " = " + expected);
    }
    private static Usuario Login(string actor, string password)
    {
        Users.Logout();
        var user = Users.Login("demo." + actor + "@perfumeria.example", password);
        Assert(user != null, "Login " + actor);
        return user;
    }
    private static string Snapshot()
    {
        var s = new StringBuilder();
        foreach (string table in new[] { "Producto", "CotizacionProveedor", "DetalleCotizacionProveedor", "OrdenCompra", "DetalleOrdenCompra",
            "Lote", "MovimientoStock", "Venta", "DetalleVenta", "Cliente", "Membresia", "Usuarios", "DVV" })
            using (var writer = new StringWriter()) { Query("SELECT * FROM dbo.[" + table + "] ORDER BY 1").WriteXml(writer); s.Append(writer); }
        return s.ToString();
    }
    private static string RunSeed(string mode, bool success)
    {
        var start = new ProcessStartInfo(seed, mode + " \"" + database + "\"") {
            UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var p = Process.Start(start))
        {
            string output = p.StandardOutput.ReadToEnd(), error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            Assert((p.ExitCode == 0) == success, "Seed " + mode +
                (p.ExitCode == 0 ? " OK" : " rechazo: " + error.Trim()) + (success && p.ExitCode != 0 ? output : ""));
            return output + error;
        }
    }
    private static List<Lote> Lots(int floral, int wood, int floralQuantity, int woodQuantity, string marker)
    {
        return new List<Lote> {
            new Lote { IdProducto=floral, CodigoLote=marker+"-F", CantidadIngresada=floralQuantity,
                Origen="Argentina (test)", CertificadoOrigen="CERT-"+marker+"-F", FechaVencimiento=DateTime.Today.AddDays(365) },
            new Lote { IdProducto=wood, CodigoLote=marker+"-M", CantidadIngresada=woodQuantity,
                Origen="Argentina (test)", CertificadoOrigen="CERT-"+marker+"-M", FechaVencimiento=DateTime.Today.AddDays(365) }
        };
    }
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length != 2 || !args[0].StartsWith("Perfumeria_Demo_Test_", StringComparison.Ordinal))
                throw new Exception("Solo se admiten bases aisladas creadas por test-demo-scenarios.ps1.");
            database = args[0]; seed = args[1];
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["MiConexionDB"].ConnectionString);
            if (builder.InitialCatalog != database) throw new Exception("La conexion no coincide con la base de prueba.");
            using (connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                int floral = Id("SELECT IdProducto FROM dbo.Producto WHERE Codigo=@p0", "DEMO-FLORAL");
                int wood = Id("SELECT IdProducto FROM dbo.Producto WHERE Codigo=@p0", "DEMO-MADERA");
                int citrus = Id("SELECT IdProducto FROM dbo.Producto WHERE Codigo=@p0", "DEMO-CITRICO");
                int[] states = { 0, 1, 2, 3, 4, 5, 6, 0 };
                for (int i = 1; i <= states.Length; i++) State(i, (EstadoOrdenCompra)states[i - 1]);
                Assert(Inventory.VerificarStock(floral) == 14 && Inventory.VerificarStock(wood) == 7 && Inventory.VerificarStock(citrus) == 0, "Stock inicial 14 / 7 / 0");
                Assert(Inventory.ObtenerTrazabilidad(floral).Count == 3 && Inventory.ObtenerTrazabilidad(wood).Count == 2, "Cinco lotes con trazabilidad");
                string before = Snapshot();
                RunSeed("--check", true);
                Assert(before == Snapshot(), "Prevalidacion sin escrituras");
                Assert(RunSeed("--apply", true).Contains("operaciones de precarga: 0"), "Segunda precarga sin operaciones");
                Assert(before == Snapshot(), "Segunda precarga no altera ningun dato de negocio");

                Login("dueno", "DemoDueno2026!");
                Assert(Purchases.EnviarOrdenARevisionContable(Order(1)), "Borrador con dos cotizaciones enviado");
                State(1, EstadoOrdenCompra.PendienteAprobacion);
                Reject<InvalidOperationException>(() => Purchases.EnviarOrdenARevisionContable(Order(8)), "Una cotizacion no alcanza");
                State(8, EstadoOrdenCompra.Borrador);
                Reject<UnauthorizedAccessException>(() => Purchases.AprobarOrdenCompra(Order(2), 1), "Compras no aprueba");
                Reject<InvalidOperationException>(() => Purchases.EmitirOrdenAprobada(Order(4)), "No se emite una rechazada");

                var accountant = Login("contabilidad", "DemoContabilidad2026!");
                Assert(Purchases.AprobarOrdenCompra(Order(2), accountant.Id), "Contabilidad aprueba pendiente");
                Assert(Purchases.RechazarOrdenCompra(Order(1), "TEST: propuesta fuera del presupuesto"), "Contabilidad rechaza con motivo");
                Reject<UnauthorizedAccessException>(() => Purchases.EmitirOrdenAprobada(Order(3)), "Contabilidad no emite");
                Login("dueno", "DemoDueno2026!");
                Assert(Purchases.EmitirOrdenAprobada(Order(2)) && Purchases.EmitirOrdenAprobada(Order(3)), "Compras emite aprobadas");
                int provider2 = Id("SELECT IdProveedor FROM dbo.Proveedor WHERE Cuit=@p0", "99000000002");
                Purchases.RegistrarCotizacion(new CotizacionProveedor { IdProveedor=provider2, FechaVigencia=DateTime.Today.AddMonths(6),
                    Condiciones="TEST segunda cotizacion Citrico", Detalles=new List<DetalleCotizacionProveedor> {
                        new DetalleCotizacionProveedor { IdProducto=citrus, PrecioUnitario=8900m, PlazoEntregaDias=5 } } });
                Assert(Purchases.EnviarOrdenARevisionContable(Order(8)), "Agregar segunda cotizacion desbloquea escenario 08");

                Login("deposito", "DemoDeposito2026!");
                before = Snapshot();
                Reject<InvalidOperationException>(() => Purchases.RecepcionarMercaderia(Order(6), Lots(floral, wood, 7, 3, "TEST-EXCESO")), "Recepcion no supera pendiente");
                Assert(before == Snapshot(), "Recepcion rechazada sin efectos parciales");
                Assert(Purchases.RecepcionarMercaderia(Order(5), Lots(floral, wood, 3, 2, "TEST-PARCIAL")), "Primera recepcion parcial");
                State(5, EstadoOrdenCompra.RecibidaParcial);
                Assert(Purchases.RecepcionarMercaderia(Order(5), Lots(floral, wood, 7, 3, "TEST-COMPLETA")), "Completar recepcion emitida");
                Assert(Purchases.RecepcionarMercaderia(Order(6), Lots(floral, wood, 6, 3, "TEST-PENDIENTE")), "Completar escenario parcial precargado");
                State(5, EstadoOrdenCompra.Recibida); State(6, EstadoOrdenCompra.Recibida);

                Login("ventas", "DemoVentas2026!");
                int stock = Inventory.VerificarStock(floral);
                var sale = new Venta { IdCliente=Sales.BuscarClientePorEmail("demo.cliente@perfumeria.example").IdCliente,
                    Detalles=new List<DetalleVenta> { new DetalleVenta { IdProducto=floral, Cantidad=2 }, new DetalleVenta { IdProducto=wood, Cantidad=1 } } };
                Assert(Sales.RegistrarVenta(sale) > 0 && sale.Subtotal == 63000m && sale.Descuento == 6300m && sale.Total == 56700m,
                    "Venta con membresia: 63000 - 6300 = 56700");
                Assert(Inventory.VerificarStock(floral) == stock - 2, "Venta descuenta stock");
                var trace = Inventory.ObtenerTrazabilidad(floral);
                Assert(sale.Detalles.Where(d => d.IdProducto == floral).All(d => trace.Single(l => l.IdLote == d.IdLote).FechaVencimiento == DateTime.Today.AddDays(90)), "Venta usa lotes FEFO");
                before = Snapshot();
                Reject<InvalidOperationException>(() => Sales.RegistrarVenta(new Venta { Detalles=new List<DetalleVenta> {
                    new DetalleVenta { IdProducto=citrus, Cantidad=1 } } }), "Venta sin stock rechazada");
                Assert(before == Snapshot(), "Venta rechazada sin efectos parciales");
                Users.Logout();
                before = Snapshot();
                Assert(RunSeed("--apply", true).Contains("operaciones de precarga: 0"), "Precarga tras usar escenarios sin operaciones");
                Assert(before == Snapshot(), "Conserva estados avanzados, nueva cotizacion, venta y stock consumido");
                RunSeed("--verify", true);

                // Force a late validation failure AFTER the seed creates a missing scenario.
                // Only this throwaway database is altered: prove the ENTIRE seed rolls back.
                int blocked = Order(8), complete = Order(7);
                Query("DELETE dbo.DetalleOrdenCompra WHERE IdOrdenCompra=@p0; DELETE dbo.OrdenCompra WHERE IdOrdenCompra=@p0", blocked);
                Query("UPDATE dbo.OrdenCompra SET Total=Total+1 WHERE IdOrdenCompra=@p0", complete);
                before = Snapshot();
                Assert(RunSeed("--apply", false).Contains("Total/detalles inconsistentes"), "Fallo por inconsistencia de negocio esperada");
                Assert(before == Snapshot(), "Fallo tardio revierte tambien el escenario recreado (transaccion completa)");
                Query("UPDATE dbo.OrdenCompra SET Total=Total-1 WHERE IdOrdenCompra=@p0", complete);
                RunSeed("--apply", true); RunSeed("--verify", true);
                Console.WriteLine("RESULTADO: " + assertions + " verificaciones OK; base conservada: " + database);
            }
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error.ToString()); return 1; }
        finally { Users.Logout(); }
    }
}
