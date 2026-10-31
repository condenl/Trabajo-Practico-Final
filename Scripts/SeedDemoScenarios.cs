// Academic fixtures, not migrations. All writes share the actors' serializable transaction.
// Stable reserved markers identify fixtures across machines; database identities are never assumed.
using System;
using System.Data;
using BE;

internal static partial class SeedDemoActors
{
    private static readonly string[] ProductCodes = { "DEMO-FLORAL", "DEMO-MADERA", "DEMO-CITRICO" };
    private static readonly string[] ProductNames = { "DEMO Floral 50 ml", "DEMO Madera 100 ml", "DEMO Citrico sin stock" };
    private static readonly decimal[] PurchasePrices = { 12000m, 18000m, 9000m };
    private static readonly int[] InitialStates = { 0, 1, 2, 3, 4, 5, 6, 0 };

    private static string OrderMarker(int scenario)
    {
        // CreadaPor is visible in the purchase grid; retain the originating actor as well.
        return "DEMO-OC-" + scenario.ToString("D2") + " | demo.dueno@perfumeria.example";
    }
    private static int FixtureId(string sql, params object[] values)
    {
        var rows = Query(sql, values);
        Require(rows.Rows.Count == 1, "Falta un registro demo o esta duplicado: " + Convert.ToString(values[0]));
        return Convert.ToInt32(rows.Rows[0][0]);
    }
    private static void CheckScenarioConflicts()
    {
        for (int i = 0; i < ProductCodes.Length; i++)
        {
            var rows = Query("SELECT Nombre FROM dbo.Producto WHERE Codigo=@p0", ProductCodes[i]);
            Require(rows.Rows.Count == 0 || (rows.Rows.Count == 1 && (string)rows.Rows[0][0] == ProductNames[i]),
                "Codigo reservado para escenarios demo ya utilizado: " + ProductCodes[i]);
        }
        for (int i = 1; i <= 2; i++)
        {
            var rows = Query(@"SELECT p.Cuit FROM dbo.CotizacionProveedor c JOIN dbo.Proveedor p ON p.IdProveedor=c.IdProveedor
                WHERE c.Condiciones=@p0", "DEMO-COT-0" + i);
            Require(rows.Rows.Count == 0 || (rows.Rows.Count == 1 && (string)rows.Rows[0][0] == "9900000000" + i),
                "Marcador de cotizacion demo duplicado o utilizado por otro proveedor.");
        }
        for (int i = 1; i <= InitialStates.Length; i++)
        {
            var rows = Query(@"SELECT p.Cuit FROM dbo.OrdenCompra o JOIN dbo.Proveedor p ON p.IdProveedor=o.IdProveedor
                WHERE o.CreadaPor=@p0", OrderMarker(i));
            Require(rows.Rows.Count == 0 || (rows.Rows.Count == 1 && (string)rows.Rows[0][0] == "99000000001"),
                "Marcador de orden demo duplicado o utilizado por otro proveedor: " + OrderMarker(i));
        }
    }
    private static void ApplyScenarios()
    {
        int[] products = new int[3];
        string[] families = { "Floral", "Amaderada", "Citrica" };
        decimal[] salePrices = { 18000m, 27000m, 14000m };
        for (int i = 0; i < products.Length; i++)
        {
            var existing = Query("SELECT IdProducto FROM dbo.Producto WHERE Codigo=@p0", ProductCodes[i]);
            products[i] = existing.Rows.Count == 0
                ? InsertId(@"INSERT dbo.Producto(Codigo,Nombre,Marca,FamiliaOlfativa,PrecioVenta,StockMinimo,Activo)
                    VALUES(@p0,@p1,'DEMO',@p2,@p3,5,1)", ProductCodes[i], ProductNames[i], families[i], salePrices[i])
                : (int)existing.Rows[0][0];
        }
        DateTime today = DateTime.Today;
        int provider = FixtureId("SELECT IdProveedor FROM dbo.Proveedor WHERE Cuit=@p0", "99000000001");
        int accountant = FixtureId("SELECT Id FROM dbo.Usuarios WHERE Email=@p0", "demo.contabilidad@perfumeria.example");
        for (int i = 1; i <= 2; i++)
        {
            string marker = "DEMO-COT-0" + i;
            if (Query("SELECT IdCotizacion FROM dbo.CotizacionProveedor WHERE Condiciones=@p0", marker).Rows.Count != 0) continue;
            int quotedProvider = FixtureId("SELECT IdProveedor FROM dbo.Proveedor WHERE Cuit=@p0", "9900000000" + i);
            int quote = InsertId(@"INSERT dbo.CotizacionProveedor(IdProveedor,FechaEmision,FechaVigencia,Condiciones)
                VALUES(@p0,@p1,@p2,@p3)", quotedProvider, today.AddDays(-30), today.AddYears(1), marker);
            // Citrico intentionally has only ONE quote: demonstrate the two-provider validation.
            for (int p = 0; p < (i == 1 ? 3 : 2); p++)
                InsertId(@"INSERT dbo.DetalleCotizacionProveedor(IdCotizacion,IdProducto,PrecioUnitario,PlazoEntregaDias)
                    VALUES(@p0,@p1,@p2,@p3)", quote, products[p], PurchasePrices[p] * (i == 1 ? 1m : 0.95m), i == 1 ? 3 : 7);
        }
        for (int scenario = 1; scenario <= InitialStates.Length; scenario++)
        {
            string marker = OrderMarker(scenario);
            // Skip the WHOLE existing scenario. Never reset its state, restock sold units or recreate its lots.
            if (Query("SELECT IdOrdenCompra FROM dbo.OrdenCompra WHERE CreadaPor=@p0", marker).Rows.Count != 0) continue;
            int state = InitialStates[scenario - 1];
            bool approved = state == 2 || state >= 4;
            int order = InsertId(@"INSERT dbo.OrdenCompra(IdProveedor,FechaEmision,Total,Estado,
                IdUsuarioAprobadorContable,FechaAprobacion,MotivoRechazo,CreadaPor)
                VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7)", provider, today.AddDays(-10),
                scenario == 8 ? 27000m : 210000m, state, approved ? (object)accountant : null,
                approved ? (object)today.AddDays(-9) : null,
                state == 3 ? "DEMO: presupuesto excedido; solicitar una nueva propuesta." : null, marker);
            if (scenario == 8)
                AddOrderLine(order, products[2], 3, 0, PurchasePrices[2]);
            else
            {
                int floralReceived = state == 6 ? 10 : state == 5 ? 4 : 0;
                int woodReceived = state == 6 ? 5 : state == 5 ? 2 : 0;
                AddOrderLine(order, products[0], 10, floralReceived, PurchasePrices[0]);
                AddOrderLine(order, products[1], 5, woodReceived, PurchasePrices[1]);
                if (floralReceived > 0)
                {
                    int first = state == 6 ? 6 : 4;
                    AddDemoLot(scenario, order, provider, products[0], "FLORAL-A", first, today.AddDays(90));
                    if (floralReceived > first)
                        AddDemoLot(scenario, order, provider, products[0], "FLORAL-B", floralReceived - first, today.AddDays(180));
                    AddDemoLot(scenario, order, provider, products[1], "MADERA-A", woodReceived, today.AddDays(180));
                }
            }
            Console.WriteLine("CREADO DEMO-OC-" + scenario.ToString("D2") + ": orden " + order + ", " + (EstadoOrdenCompra)state);
        }
    }
    private static void AddOrderLine(int order, int product, int quantity, int received, decimal price)
    {
        InsertId(@"INSERT dbo.DetalleOrdenCompra(IdOrdenCompra,IdProducto,Cantidad,CantidadRecibida,PrecioUnitario)
            VALUES(@p0,@p1,@p2,@p3,@p4)", order, product, quantity, received, price);
    }
    private static void AddDemoLot(int scenario, int order, int provider, int product, string suffix, int quantity, DateTime expiry)
    {
        string code = "DEMO-OC-" + scenario.ToString("D2") + "-" + suffix;
        int lot = InsertId(@"INSERT dbo.Lote(IdProducto,IdProveedor,IdOrdenCompra,CodigoLote,Origen,
            CertificadoOrigen,FechaVencimiento,FechaRecepcion,CantidadIngresada,CantidadDisponible)
            VALUES(@p0,@p1,@p2,@p3,'Argentina (ficticio)',@p4,@p5,@p6,@p7,@p7)",
            product, provider, order, code, "CERT-" + code, expiry, DateTime.Today.AddDays(-7), quantity);
        InsertId(@"INSERT dbo.MovimientoStock(IdLote,Tipo,Cantidad,Fecha,Referencia,Responsable)
            VALUES(@p0,0,@p1,@p2,@p3,'demo.deposito@perfumeria.example')", lot, quantity,
            DateTime.Today.AddDays(-7), "Orden de compra " + order + " (precarga DEMO)");
    }
    private static void VerifyScenarios()
    {
        CheckScenarioConflicts();
        foreach (string code in ProductCodes) FixtureId("SELECT IdProducto FROM dbo.Producto WHERE Codigo=@p0", code);
        for (int i = 1; i <= 2; i++)
        {
            int quote = FixtureId("SELECT IdCotizacion FROM dbo.CotizacionProveedor WHERE Condiciones=@p0", "DEMO-COT-0" + i);
            Require(Query("SELECT IdDetalleCotizacion FROM dbo.DetalleCotizacionProveedor WHERE IdCotizacion=@p0", quote).Rows.Count > 0,
                "Cotizacion demo sin detalles.");
        }
        for (int i = 1; i <= InitialStates.Length; i++)
        {
            int id = FixtureId("SELECT IdOrdenCompra FROM dbo.OrdenCompra WHERE CreadaPor=@p0", OrderMarker(i));
            var order = Query("SELECT * FROM dbo.OrdenCompra WHERE IdOrdenCompra=@p0", id).Rows[0];
            var totals = Query(@"SELECT COUNT(*),COALESCE(SUM(Cantidad*PrecioUnitario),0),SUM(Cantidad),SUM(CantidadRecibida)
                FROM dbo.DetalleOrdenCompra WHERE IdOrdenCompra=@p0", id).Rows[0];
            Require((int)totals[0] > 0 && Convert.ToDecimal(totals[1]) == (decimal)order["Total"], "Total/detalles inconsistentes: orden " + id);
            int state = (int)order["Estado"], received = Convert.ToInt32(totals[3]);
            Require(state != 3 || !string.IsNullOrWhiteSpace(Convert.ToString(order["MotivoRechazo"])), "Rechazo demo sin motivo.");
            Require(!(state == 2 || (state >= 4 && state <= 6)) ||
                (order["IdUsuarioAprobadorContable"] != DBNull.Value && order["FechaAprobacion"] != DBNull.Value), "Orden demo sin aprobacion contable.");
            Require((state != 6 || received == Convert.ToInt32(totals[2])) &&
                (state != 5 || (received > 0 && received < Convert.ToInt32(totals[2]))) &&
                (state > 4 || received == 0), "Estado y recepcion demo inconsistentes: orden " + id);
            Require(Query(@"SELECT d.IdDetalle FROM dbo.DetalleOrdenCompra d WHERE d.IdOrdenCompra=@p0 AND
                d.CantidadRecibida<>COALESCE((SELECT SUM(l.CantidadIngresada) FROM dbo.Lote l
                WHERE l.IdOrdenCompra=d.IdOrdenCompra AND l.IdProducto=d.IdProducto),0)", id).Rows.Count == 0,
                "Detalle y lotes demo inconsistentes: orden " + id);
            Require(Query(@"SELECT l.IdLote FROM dbo.Lote l WHERE l.IdOrdenCompra=@p0 AND
                (l.IdProveedor<>@p1 OR NOT EXISTS(SELECT 1 FROM dbo.DetalleOrdenCompra d WHERE d.IdOrdenCompra=@p0 AND d.IdProducto=l.IdProducto)
                OR l.CantidadDisponible<>COALESCE((SELECT SUM(m.Cantidad) FROM dbo.MovimientoStock m WHERE m.IdLote=l.IdLote),0))",
                id, order["IdProveedor"]).Rows.Count == 0, "Trazabilidad o movimientos demo inconsistentes: orden " + id);
            Console.WriteLine("ESCENARIO OK DEMO-OC-" + i.ToString("D2") + ": orden " + id + ", " + (EstadoOrdenCompra)state);
        }
    }
}
