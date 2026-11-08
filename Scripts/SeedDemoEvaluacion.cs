// Additional idempotent N03 fixtures; never overwrite an existing demo's progress.
using System;

internal static partial class SeedDemoActors
{
    private static readonly string[] EvaluationCodes = { "DEMO-EVAL-VALOR", "DEMO-EVAL-SIN-MARGEN" };
    private static void CheckEvaluationConflicts()
    {
        CheckReplenishmentConflicts();
        var provider = Query("SELECT RazonSocial FROM Proveedor WHERE Cuit='99000000003'");
        Require(provider.Rows.Count == 0 || (provider.Rows.Count == 1 && (string)provider.Rows[0][0] == "DEMO Proveedor economico N03"), "CUIT reservado N03 utilizado por otro proveedor.");
        foreach (string code in EvaluationCodes)
        {
            var p = Query("SELECT Marca FROM Producto WHERE Codigo=@p0", code);
            Require(p.Rows.Count == 0 || (p.Rows.Count == 1 && (string)p.Rows[0][0] == "DEMO N03"), "Codigo reservado N03 utilizado por otro producto.");
        }
        for (int i = 1; i <= 3; i++)
        {
            var q = Query("SELECT p.Cuit FROM CotizacionProveedor c JOIN Proveedor p ON p.IdProveedor=c.IdProveedor WHERE c.Condiciones=@p0", "DEMO-EVAL-COT-" + i);
            Require(q.Rows.Count == 0 || (q.Rows.Count == 1 && (string)q.Rows[0][0] == "9900000000" + i), "Marcador de cotizacion N03 ya utilizado.");
        }
    }
    private static void ApplyEvaluationScenarios()
    {
        CheckEvaluationConflicts();
        if (Query("SELECT IdProveedor FROM Proveedor WHERE Cuit='99000000003'").Rows.Count == 0)
            InsertId(@"INSERT Proveedor(RazonSocial,Cuit,Contacto,AntiguedadMercado,IndicePrestigio,Activo)
 VALUES('DEMO Proveedor economico N03','99000000003','demo.economico@perfumeria.example',1,35,1)");
        int[] products = new int[2];
        for (int i = 0; i < 2; i++)
        {
            var p = Query("SELECT IdProducto FROM Producto WHERE Codigo=@p0", EvaluationCodes[i]);
            products[i] = p.Rows.Count > 0 ? (int)p.Rows[0][0] : InsertId(@"INSERT Producto(Codigo,Nombre,Marca,FamiliaOlfativa,PrecioVenta,StockMinimo,Activo)
 VALUES(@p0,@p1,'DEMO N03','Floral',@p2,5,1)", EvaluationCodes[i], i == 0 ? "DEMO Evaluacion - valor vs precio" : "DEMO Evaluacion - sin margen", i == 0 ? 18000m : 9000m);
        }
        for (int i = 1; i <= 3; i++)
        {
            string marker = "DEMO-EVAL-COT-" + i;
            if (Query("SELECT IdCotizacion FROM CotizacionProveedor WHERE Condiciones=@p0", marker).Rows.Count > 0) continue;
            int provider = FixtureId("SELECT IdProveedor FROM Proveedor WHERE Cuit=@p0", "9900000000" + i);
            int quote = InsertId("INSERT CotizacionProveedor(IdProveedor,FechaEmision,FechaVigencia,Condiciones) VALUES(@p0,@p1,@p2,@p3)", provider, DateTime.Today, DateTime.Today.AddYears(1), marker);
            foreach (int product in products)
                InsertId("INSERT DetalleCotizacionProveedor(IdCotizacion,IdProducto,PrecioUnitario,PlazoEntregaDias) VALUES(@p0,@p1,@p2,@p3)", quote, product, i == 1 ? 12000m : i == 2 ? 12500m : 10000m, i == 3 ? 14 : 5);
        }
        ApplyReplenishmentScenario();
    }
    private static void VerifyEvaluationScenarios()
    {
        VerifyReplenishmentScenario();
        CheckEvaluationConflicts();
        foreach (string code in EvaluationCodes) FixtureId("SELECT IdProducto FROM Producto WHERE Codigo=@p0", code);
        for (int i = 1; i <= 3; i++)
        {
            int q = FixtureId("SELECT IdCotizacion FROM CotizacionProveedor WHERE Condiciones=@p0", "DEMO-EVAL-COT-" + i);
            Require(Query("SELECT IdDetalleCotizacion FROM DetalleCotizacionProveedor WHERE IdCotizacion=@p0", q).Rows.Count == 2, "Cotizacion N03 incompleta.");
        }
        Console.WriteLine("ESCENARIOS N03 OK: valor frente al precio, cambio de ponderaciones y ausencia de margen.");
    }
}
