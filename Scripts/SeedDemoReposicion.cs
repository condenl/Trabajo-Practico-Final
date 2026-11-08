// Escenario nuevo y atomico: no modifica ni reinicia las demos N03 existentes.
using System;

internal static partial class SeedDemoActors
{
    private const string ReposicionCodigo = "DEMO-EVAL-REPOSICION";
    private const string ReposicionNombre = "DEMO Reposicion con historial";
    private const string ReposicionOrden = "DEMO-REP-HISTORIAL | demo.dueno@perfumeria.example";
    private const string ReposicionParcial = "DEMO-REP-PARCIAL | demo.dueno@perfumeria.example";

    private static void CheckReplenishmentConflicts()
    {
        var producto = Query("SELECT IdProducto,Nombre,Marca FROM Producto WHERE Codigo=@p0", ReposicionCodigo);
        Require(producto.Rows.Count == 0 || (producto.Rows.Count == 1 && (string)producto.Rows[0]["Nombre"] == ReposicionNombre &&
            (string)producto.Rows[0]["Marca"] == "DEMO N03"), "Codigo reservado de reposicion utilizado por otro producto.");
        var raiz = Query("SELECT IdOrdenCompra FROM OrdenCompra WHERE CreadaPor=@p0", ReposicionOrden);
        Require(raiz.Rows.Count <= 1, "Escenario de reposicion duplicado.");
        if (raiz.Rows.Count == 0) {
            Require(Query("SELECT IdOrdenCompra FROM OrdenCompra WHERE CreadaPor=@p0", ReposicionParcial).Rows.Count == 0 &&
                Query("SELECT IdLote FROM Lote WHERE CodigoLote IN ('DEMO-REP-ANTERIOR','DEMO-REP-PARCIAL')").Rows.Count == 0,
                "Escenario de reposicion incompleto o marcadores utilizados; no se recrea parcialmente.");
            if (producto.Rows.Count == 1)
                Require(Query(@"SELECT IdProducto FROM DetalleOrdenCompra WHERE IdProducto=@p0
 UNION ALL SELECT IdProducto FROM DetalleVenta WHERE IdProducto=@p0
 UNION ALL SELECT IdProducto FROM Lote WHERE IdProducto=@p0", producto.Rows[0]["IdProducto"]).Rows.Count == 0,
                    "El producto reservado de reposicion ya tiene operaciones ajenas al escenario.");
        }
        for (int i = 1; i <= 3; i++) {
            var q = Query(@"SELECT p.Cuit FROM CotizacionProveedor c JOIN Proveedor p ON p.IdProveedor=c.IdProveedor
 WHERE c.Condiciones=@p0", "DEMO-REP-COT-0" + i);
            Require(q.Rows.Count == 0 || (q.Rows.Count == 1 && (string)q.Rows[0][0] == "9900000000" + i), "Marcador de cotizacion de reposicion utilizado.");
        }
    }
    private static void ApplyReplenishmentScenario()
    {
        CheckReplenishmentConflicts();
        if (Query("SELECT IdOrdenCompra FROM OrdenCompra WHERE CreadaPor=@p0", ReposicionOrden).Rows.Count > 0) return;
        var existente = Query("SELECT IdProducto FROM Producto WHERE Codigo=@p0", ReposicionCodigo);
        int producto = existente.Rows.Count == 1 ? (int)existente.Rows[0][0] : InsertId(@"INSERT Producto(Codigo,Nombre,Marca,FamiliaOlfativa,PrecioVenta,StockMinimo,Activo)
 VALUES(@p0,@p1,'DEMO N03','Floral',18000,5,1)", ReposicionCodigo, ReposicionNombre);
        DateTime hoy = DateTime.Today;
        int proveedor = FixtureId("SELECT IdProveedor FROM Proveedor WHERE Cuit=@p0", "99000000001");
        int contable = FixtureId("SELECT Id FROM Usuarios WHERE Email=@p0", "demo.contabilidad@perfumeria.example");
        for (int i = 1; i <= 3; i++) {
            string marca = "DEMO-REP-COT-0" + i;
            Require(Query("SELECT IdCotizacion FROM CotizacionProveedor WHERE Condiciones=@p0", marca).Rows.Count == 0,
                "Hay cotizaciones de reposicion sin escenario; revisar los marcadores antes de precargar.");
            int cotizacion = InsertId(@"INSERT CotizacionProveedor(IdProveedor,FechaEmision,FechaVigencia,Condiciones)
 VALUES(@p0,@p1,@p2,@p3)", FixtureId("SELECT IdProveedor FROM Proveedor WHERE Cuit=@p0", "9900000000" + i), hoy.AddDays(-90), hoy.AddYears(1), marca);
            InsertId(@"INSERT DetalleCotizacionProveedor(IdCotizacion,IdProducto,PrecioUnitario,PlazoEntregaDias)
 VALUES(@p0,@p1,@p2,@p3)", cotizacion, producto, i == 1 ? 12000m : i == 2 ? 12500m : 10000m, i == 3 ? 14 : 5);
        }
        int anterior = InsertId(@"INSERT OrdenCompra(IdProveedor,FechaEmision,Total,Estado,IdUsuarioAprobadorContable,FechaAprobacion,CreadaPor)
 VALUES(@p0,@p1,1056000,6,@p2,@p3,@p4)", proveedor, hoy.AddDays(-75), contable, hoy.AddDays(-74), ReposicionOrden);
        AddOrderLine(anterior, producto, 88, 88, 12000m);
        int parcial = InsertId(@"INSERT OrdenCompra(IdProveedor,FechaEmision,Total,Estado,IdUsuarioAprobadorContable,FechaAprobacion,CreadaPor)
 VALUES(@p0,@p1,240000,5,@p2,@p3,@p4)", proveedor, hoy.AddDays(-5), contable, hoy.AddDays(-4), ReposicionParcial);
        AddOrderLine(parcial, producto, 20, 12, 12000m);
        int loteAnterior = AddReplenishmentLot(anterior, proveedor, producto, "DEMO-REP-ANTERIOR", 88, 0, hoy.AddDays(-73), hoy.AddDays(90));
        int loteParcial = AddReplenishmentLot(parcial, proveedor, producto, "DEMO-REP-PARCIAL", 12, 10, hoy.AddDays(-2), hoy.AddDays(180));
        int cliente = FixtureId("SELECT IdCliente FROM Cliente WHERE Email=@p0", "demo.cliente@perfumeria.example");
        int vendedor = FixtureId("SELECT Id FROM Usuarios WHERE Email=@p0", "demo.ventas@perfumeria.example");
        foreach (int dias in new[] { -60, -30, -1 }) {
            DateTime fecha = hoy.AddDays(dias);
            int venta = InsertId(@"INSERT Venta(IdCliente,IdUsuarioVendedor,Fecha,Subtotal,Descuento,Total)
 VALUES(@p0,@p1,@p2,540000,54000,486000)", cliente, vendedor, fecha);
            AddReplenishmentSaleLine(venta, producto, loteAnterior, dias == -1 ? 28 : 30, fecha);
            if (dias == -1) AddReplenishmentSaleLine(venta, producto, loteParcial, 2, fecha);
        }
        Console.WriteLine("CREADO N03 reposicion: 90 unidades vendidas, stock 10, pendientes 8; sugerencia inicial 12 unidades.");
    }
    private static int AddReplenishmentLot(int orden, int proveedor, int producto, string codigo, int ingresado, int disponible, DateTime fecha, DateTime vence)
    {
        int lote = InsertId(@"INSERT Lote(IdProducto,IdProveedor,IdOrdenCompra,CodigoLote,Origen,CertificadoOrigen,
 FechaVencimiento,FechaRecepcion,CantidadIngresada,CantidadDisponible) VALUES(@p0,@p1,@p2,@p3,'Argentina (ficticio)','',@p4,@p5,@p6,@p7)",
            producto, proveedor, orden, codigo, vence, fecha, ingresado, disponible);
        InsertId(@"INSERT MovimientoStock(IdLote,Tipo,Cantidad,Fecha,Referencia,Responsable)
 VALUES(@p0,0,@p1,@p2,@p3,'demo.deposito@perfumeria.example')", lote, ingresado, fecha, "Orden de compra " + orden + " (precarga DEMO N03)");
        return lote;
    }
    private static void AddReplenishmentSaleLine(int venta, int producto, int lote, int cantidad, DateTime fecha)
    {
        InsertId(@"INSERT DetalleVenta(IdVenta,IdProducto,IdLote,Cantidad,PrecioUnitario,DescuentoUnitario)
 VALUES(@p0,@p1,@p2,@p3,18000,1800)", venta, producto, lote, cantidad);
        InsertId(@"INSERT MovimientoStock(IdLote,Tipo,Cantidad,Fecha,Referencia,Responsable)
 VALUES(@p0,1,@p1,@p2,@p3,'demo.ventas@perfumeria.example')", lote, -cantidad, fecha, "Venta " + venta);
    }
    private static void VerifyReplenishmentScenario()
    {
        CheckReplenishmentConflicts();
        int producto = FixtureId("SELECT IdProducto FROM Producto WHERE Codigo=@p0", ReposicionCodigo);
        foreach (string marca in new[] { ReposicionOrden, ReposicionParcial }) {
            int orden = FixtureId("SELECT IdOrdenCompra FROM OrdenCompra WHERE CreadaPor=@p0", marca);
            Require(Query(@"SELECT o.IdOrdenCompra FROM OrdenCompra o WHERE o.IdOrdenCompra=@p0
 AND o.IdProveedor=(SELECT IdProveedor FROM Proveedor WHERE Cuit='99000000001')
 AND o.IdUsuarioAprobadorContable IS NOT NULL AND o.FechaAprobacion IS NOT NULL AND o.Estado IN(5,6)
 AND o.Total=(SELECT SUM(Cantidad*PrecioUnitario) FROM DetalleOrdenCompra WHERE IdOrdenCompra=o.IdOrdenCompra)
 AND (SELECT COUNT(*) FROM DetalleOrdenCompra WHERE IdOrdenCompra=o.IdOrdenCompra AND IdProducto=@p1)=1
 AND NOT EXISTS(SELECT 1 FROM DetalleOrdenCompra d WHERE d.IdOrdenCompra=o.IdOrdenCompra AND
 (d.CantidadRecibida<>COALESCE((SELECT SUM(CantidadIngresada) FROM Lote WHERE IdOrdenCompra=o.IdOrdenCompra AND IdProducto=d.IdProducto),0)
 OR (o.Estado=6 AND d.CantidadRecibida<>d.Cantidad) OR (o.Estado=5 AND (d.CantidadRecibida<=0 OR d.CantidadRecibida>=d.Cantidad))))", orden, producto).Rows.Count == 1,
                "Orden, aprobacion o recepcion de reposicion inconsistente.");
        }
        Require(Query(@"SELECT l.IdLote FROM Lote l WHERE l.IdProducto=@p0 AND (l.CantidadDisponible<0 OR l.CantidadDisponible>l.CantidadIngresada
 OR l.CantidadDisponible<>COALESCE((SELECT SUM(Cantidad) FROM MovimientoStock WHERE IdLote=l.IdLote),0))", producto).Rows.Count == 0,
            "Stock y movimientos de reposicion inconsistentes.");
        Require(Query(@"SELECT v.IdVenta FROM Venta v WHERE EXISTS(SELECT 1 FROM DetalleVenta WHERE IdVenta=v.IdVenta AND IdProducto=@p0)
 AND (v.Subtotal<>(SELECT SUM(Cantidad*PrecioUnitario) FROM DetalleVenta WHERE IdVenta=v.IdVenta)
 OR v.Descuento<>(SELECT SUM(Cantidad*DescuentoUnitario) FROM DetalleVenta WHERE IdVenta=v.IdVenta) OR v.Total<>v.Subtotal-v.Descuento)", producto).Rows.Count == 0,
            "Ventas y descuentos de reposicion inconsistentes.");
        Require(Query("SELECT DISTINCT IdVenta FROM DetalleVenta WHERE IdProducto=@p0", producto).Rows.Count >= 3,
            "Falta el historial de ventas de reposicion; no se recrea parcialmente.");
        for (int i = 1; i <= 3; i++) {
            int cotizacion = FixtureId("SELECT IdCotizacion FROM CotizacionProveedor WHERE Condiciones=@p0", "DEMO-REP-COT-0" + i);
            Require(Query("SELECT IdDetalleCotizacion FROM DetalleCotizacionProveedor WHERE IdCotizacion=@p0 AND IdProducto=@p1", cotizacion, producto).Rows.Count == 1,
                "Cotizacion de reposicion incompleta.");
        }
        Console.WriteLine("ESCENARIO N03 REPOSICION OK: compras, recepciones, ventas, descuentos y movimientos coherentes; progreso conservado.");
    }
}
