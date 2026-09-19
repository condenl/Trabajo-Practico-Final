/*
    Iteración 2 - Aprovisionamiento, aprobación contable, inventario y trazabilidad.

    Estados de OrdenCompra:
      0 Borrador, 1 PendienteAprobacion, 2 Aprobada, 3 Rechazada,
      4 Emitida, 5 RecibidaParcial, 6 Recibida, 7 Cancelada.
*/

IF OBJECT_ID(N'dbo.Proveedor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Proveedor
    (
        IdProveedor INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Proveedor PRIMARY KEY,
        RazonSocial NVARCHAR(150) NOT NULL,
        Cuit VARCHAR(11) NOT NULL,
        Contacto NVARCHAR(200) NULL,
        AntiguedadMercado INT NOT NULL CONSTRAINT DF_Proveedor_Antiguedad DEFAULT (0),
        IndicePrestigio DECIMAL(5,2) NOT NULL CONSTRAINT DF_Proveedor_Prestigio DEFAULT (0),
        Activo BIT NOT NULL CONSTRAINT DF_Proveedor_Activo DEFAULT (1),
        CONSTRAINT UQ_Proveedor_Cuit UNIQUE (Cuit),
        CONSTRAINT CK_Proveedor_Antiguedad CHECK (AntiguedadMercado >= 0),
        CONSTRAINT CK_Proveedor_Prestigio CHECK (IndicePrestigio BETWEEN 0 AND 100)
    );
END;
GO

IF OBJECT_ID(N'dbo.Producto', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Producto
    (
        IdProducto INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Producto PRIMARY KEY,
        Codigo NVARCHAR(50) NOT NULL,
        Nombre NVARCHAR(150) NOT NULL,
        Marca NVARCHAR(100) NULL,
        FamiliaOlfativa NVARCHAR(100) NULL,
        PrecioVenta DECIMAL(18,2) NOT NULL CONSTRAINT DF_Producto_Precio DEFAULT (0),
        StockMinimo INT NOT NULL CONSTRAINT DF_Producto_StockMinimo DEFAULT (0),
        Activo BIT NOT NULL CONSTRAINT DF_Producto_Activo DEFAULT (1),
        CONSTRAINT UQ_Producto_Codigo UNIQUE (Codigo),
        CONSTRAINT CK_Producto_Precio CHECK (PrecioVenta >= 0),
        CONSTRAINT CK_Producto_StockMinimo CHECK (StockMinimo >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.CotizacionProveedor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CotizacionProveedor
    (
        IdCotizacion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CotizacionProveedor PRIMARY KEY,
        IdProveedor INT NOT NULL,
        FechaEmision DATE NOT NULL,
        FechaVigencia DATE NOT NULL,
        Condiciones NVARCHAR(500) NULL,
        CONSTRAINT FK_CotizacionProveedor_Proveedor FOREIGN KEY (IdProveedor) REFERENCES dbo.Proveedor(IdProveedor),
        CONSTRAINT CK_CotizacionProveedor_Vigencia CHECK (FechaVigencia >= FechaEmision)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetalleCotizacionProveedor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetalleCotizacionProveedor
    (
        IdDetalleCotizacion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetalleCotizacionProveedor PRIMARY KEY,
        IdCotizacion INT NOT NULL,
        IdProducto INT NOT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        PlazoEntregaDias INT NOT NULL CONSTRAINT DF_DetalleCotizacion_Plazo DEFAULT (0),
        CONSTRAINT FK_DetalleCotizacion_Cotizacion FOREIGN KEY (IdCotizacion) REFERENCES dbo.CotizacionProveedor(IdCotizacion),
        CONSTRAINT FK_DetalleCotizacion_Producto FOREIGN KEY (IdProducto) REFERENCES dbo.Producto(IdProducto),
        CONSTRAINT UQ_DetalleCotizacion_Producto UNIQUE (IdCotizacion, IdProducto),
        CONSTRAINT CK_DetalleCotizacion_Precio CHECK (PrecioUnitario > 0),
        CONSTRAINT CK_DetalleCotizacion_Plazo CHECK (PlazoEntregaDias >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.OrdenCompra', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrdenCompra
    (
        IdOrdenCompra INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrdenCompra PRIMARY KEY,
        IdProveedor INT NOT NULL,
        FechaEmision DATETIME NOT NULL CONSTRAINT DF_OrdenCompra_Fecha DEFAULT (GETDATE()),
        Total DECIMAL(18,2) NOT NULL,
        Estado INT NOT NULL CONSTRAINT DF_OrdenCompra_Estado DEFAULT (0),
        IdUsuarioAprobadorContable INT NULL,
        FechaAprobacion DATETIME NULL,
        MotivoRechazo NVARCHAR(500) NULL,
        CreadaPor NVARCHAR(200) NOT NULL,
        CONSTRAINT FK_OrdenCompra_Proveedor FOREIGN KEY (IdProveedor) REFERENCES dbo.Proveedor(IdProveedor),
        CONSTRAINT FK_OrdenCompra_UsuarioAprobador FOREIGN KEY (IdUsuarioAprobadorContable) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_OrdenCompra_Total CHECK (Total > 0),
        CONSTRAINT CK_OrdenCompra_Estado CHECK (Estado BETWEEN 0 AND 7)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetalleOrdenCompra', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetalleOrdenCompra
    (
        IdDetalle INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetalleOrdenCompra PRIMARY KEY,
        IdOrdenCompra INT NOT NULL,
        IdProducto INT NOT NULL,
        Cantidad INT NOT NULL,
        CantidadRecibida INT NOT NULL CONSTRAINT DF_DetalleOrden_CantidadRecibida DEFAULT (0),
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_DetalleOrden_Orden FOREIGN KEY (IdOrdenCompra) REFERENCES dbo.OrdenCompra(IdOrdenCompra),
        CONSTRAINT FK_DetalleOrden_Producto FOREIGN KEY (IdProducto) REFERENCES dbo.Producto(IdProducto),
        CONSTRAINT UQ_DetalleOrden_Producto UNIQUE (IdOrdenCompra, IdProducto),
        CONSTRAINT CK_DetalleOrden_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_DetalleOrden_Recibida CHECK (CantidadRecibida BETWEEN 0 AND Cantidad),
        CONSTRAINT CK_DetalleOrden_Precio CHECK (PrecioUnitario > 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Lote', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Lote
    (
        IdLote INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Lote PRIMARY KEY,
        IdProducto INT NOT NULL,
        IdProveedor INT NOT NULL,
        IdOrdenCompra INT NULL,
        CodigoLote NVARCHAR(100) NOT NULL,
        Origen NVARCHAR(200) NOT NULL,
        CertificadoOrigen NVARCHAR(250) NOT NULL,
        FechaVencimiento DATE NULL,
        FechaRecepcion DATETIME NOT NULL CONSTRAINT DF_Lote_FechaRecepcion DEFAULT (GETDATE()),
        CantidadIngresada INT NOT NULL,
        CantidadDisponible INT NOT NULL,
        CONSTRAINT FK_Lote_Producto FOREIGN KEY (IdProducto) REFERENCES dbo.Producto(IdProducto),
        CONSTRAINT FK_Lote_Proveedor FOREIGN KEY (IdProveedor) REFERENCES dbo.Proveedor(IdProveedor),
        CONSTRAINT FK_Lote_OrdenCompra FOREIGN KEY (IdOrdenCompra) REFERENCES dbo.OrdenCompra(IdOrdenCompra),
        CONSTRAINT UQ_Lote_Origen UNIQUE (IdProducto, IdProveedor, CodigoLote),
        CONSTRAINT CK_Lote_CantidadIngresada CHECK (CantidadIngresada > 0),
        CONSTRAINT CK_Lote_CantidadDisponible CHECK (CantidadDisponible BETWEEN 0 AND CantidadIngresada)
    );
END;
GO

IF OBJECT_ID(N'dbo.MovimientoStock', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MovimientoStock
    (
        IdMovimiento INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MovimientoStock PRIMARY KEY,
        IdLote INT NOT NULL,
        Tipo INT NOT NULL,
        Cantidad INT NOT NULL,
        Fecha DATETIME NOT NULL CONSTRAINT DF_MovimientoStock_Fecha DEFAULT (GETDATE()),
        Referencia NVARCHAR(250) NULL,
        Responsable NVARCHAR(200) NOT NULL,
        CONSTRAINT FK_MovimientoStock_Lote FOREIGN KEY (IdLote) REFERENCES dbo.Lote(IdLote),
        CONSTRAINT CK_MovimientoStock_Tipo CHECK (Tipo BETWEEN 0 AND 3),
        CONSTRAINT CK_MovimientoStock_Cantidad CHECK (Cantidad <> 0)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CotizacionProveedor_Vigencia' AND object_id = OBJECT_ID(N'dbo.CotizacionProveedor'))
    CREATE INDEX IX_CotizacionProveedor_Vigencia ON dbo.CotizacionProveedor (FechaVigencia, IdProveedor);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_OrdenCompra_ProveedorFecha' AND object_id = OBJECT_ID(N'dbo.OrdenCompra'))
    CREATE INDEX IX_OrdenCompra_ProveedorFecha ON dbo.OrdenCompra (IdProveedor, FechaEmision DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Lote_ProductoDisponible' AND object_id = OBJECT_ID(N'dbo.Lote'))
    CREATE INDEX IX_Lote_ProductoDisponible ON dbo.Lote (IdProducto, CantidadDisponible, FechaVencimiento);
GO

IF OBJECT_ID(N'dbo.sp_Proveedor_Guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Proveedor_Guardar;
GO
CREATE PROCEDURE dbo.sp_Proveedor_Guardar
    @IdProveedor INT,
    @RazonSocial NVARCHAR(150),
    @Cuit VARCHAR(11),
    @Contacto NVARCHAR(200) = NULL,
    @AntiguedadMercado INT,
    @IndicePrestigio DECIMAL(5,2),
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @IdProveedor = 0
    BEGIN
        INSERT INTO dbo.Proveedor (RazonSocial, Cuit, Contacto, AntiguedadMercado, IndicePrestigio, Activo)
        VALUES (@RazonSocial, @Cuit, @Contacto, @AntiguedadMercado, @IndicePrestigio, @Activo);
        SET @IdProveedor = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.Proveedor
        SET RazonSocial = @RazonSocial,
            Cuit = @Cuit,
            Contacto = @Contacto,
            AntiguedadMercado = @AntiguedadMercado,
            IndicePrestigio = @IndicePrestigio,
            Activo = @Activo
        WHERE IdProveedor = @IdProveedor;

        IF @@ROWCOUNT = 0 THROW 50001, 'El proveedor indicado no existe.', 1;
    END;

    SELECT @IdProveedor;
END;
GO

IF OBJECT_ID(N'dbo.sp_Proveedor_Obtener', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Proveedor_Obtener;
GO
CREATE PROCEDURE dbo.sp_Proveedor_Obtener @IdProveedor INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProveedor, RazonSocial, Cuit, Contacto, AntiguedadMercado, IndicePrestigio, Activo
    FROM dbo.Proveedor
    WHERE IdProveedor = @IdProveedor;
END;
GO

IF OBJECT_ID(N'dbo.sp_Proveedor_Listar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Proveedor_Listar;
GO
CREATE PROCEDURE dbo.sp_Proveedor_Listar @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProveedor, RazonSocial, Cuit, Contacto, AntiguedadMercado, IndicePrestigio, Activo
    FROM dbo.Proveedor
    WHERE @SoloActivos = 0 OR Activo = 1
    ORDER BY RazonSocial;
END;
GO

IF OBJECT_ID(N'dbo.sp_Producto_Guardar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Producto_Guardar;
GO
CREATE PROCEDURE dbo.sp_Producto_Guardar
    @IdProducto INT,
    @Codigo NVARCHAR(50),
    @Nombre NVARCHAR(150),
    @Marca NVARCHAR(100) = NULL,
    @FamiliaOlfativa NVARCHAR(100) = NULL,
    @PrecioVenta DECIMAL(18,2),
    @StockMinimo INT,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF @IdProducto = 0
    BEGIN
        INSERT INTO dbo.Producto (Codigo, Nombre, Marca, FamiliaOlfativa, PrecioVenta, StockMinimo, Activo)
        VALUES (@Codigo, @Nombre, @Marca, @FamiliaOlfativa, @PrecioVenta, @StockMinimo, @Activo);
        SET @IdProducto = CONVERT(INT, SCOPE_IDENTITY());
    END
    ELSE
    BEGIN
        UPDATE dbo.Producto
        SET Codigo = @Codigo,
            Nombre = @Nombre,
            Marca = @Marca,
            FamiliaOlfativa = @FamiliaOlfativa,
            PrecioVenta = @PrecioVenta,
            StockMinimo = @StockMinimo,
            Activo = @Activo
        WHERE IdProducto = @IdProducto;

        IF @@ROWCOUNT = 0 THROW 50002, 'El producto indicado no existe.', 1;
    END;

    SELECT @IdProducto;
END;
GO

IF OBJECT_ID(N'dbo.sp_Producto_Obtener', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Producto_Obtener;
GO
CREATE PROCEDURE dbo.sp_Producto_Obtener @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProducto, Codigo, Nombre, Marca, FamiliaOlfativa, PrecioVenta, StockMinimo, Activo
    FROM dbo.Producto
    WHERE IdProducto = @IdProducto;
END;
GO

IF OBJECT_ID(N'dbo.sp_Producto_Listar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_Producto_Listar;
GO
CREATE PROCEDURE dbo.sp_Producto_Listar @SoloActivos BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    SELECT IdProducto, Codigo, Nombre, Marca, FamiliaOlfativa, PrecioVenta, StockMinimo, Activo
    FROM dbo.Producto
    WHERE @SoloActivos = 0 OR Activo = 1
    ORDER BY Marca, Nombre;
END;
GO

/*
    Permisos y perfiles operativos de las iteraciones 1 y 2.
    Los identificadores se resuelven por nombre para que la migracion sea
    idempotente y compatible con cualquier base destino seleccionada.
*/
IF NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente WHERE PermisoAsociado = N'Aprovisionamiento_Gestion')
    INSERT INTO dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
    VALUES (N'Gestionar aprovisionamiento', N'Patente', N'Aprovisionamiento_Gestion');

IF NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente WHERE PermisoAsociado = N'Contabilidad_AprobarCompras')
    INSERT INTO dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
    VALUES (N'Aprobar compras', N'Patente', N'Contabilidad_AprobarCompras');

IF NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente WHERE PermisoAsociado = N'Inventario_Recepcionar')
    INSERT INTO dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
    VALUES (N'Recepcionar mercaderia', N'Patente', N'Inventario_Recepcionar');

IF NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente WHERE PermisoAsociado = N'Ventas_Registrar')
    INSERT INTO dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
    VALUES (N'Registrar ventas y clientes', N'Patente', N'Ventas_Registrar');

IF NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente WHERE Nombre = N'Dueño de Perfumeria' AND Tipo = N'Familia')
    INSERT INTO dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
    VALUES (N'Dueño de Perfumeria', N'Familia', NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente WHERE Nombre = N'Equipo de Contabilidad' AND Tipo = N'Familia')
    INSERT INTO dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
    VALUES (N'Equipo de Contabilidad', N'Familia', NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Seguridad_Componente WHERE Nombre = N'Ventas y Fidelizacion' AND Tipo = N'Familia')
    INSERT INTO dbo.Seguridad_Componente (Nombre, Tipo, PermisoAsociado)
    VALUES (N'Ventas y Fidelizacion', N'Familia', NULL);

;WITH Relaciones(IdPadre, IdHijo) AS
(
    SELECT padre.Id, hijo.Id
    FROM dbo.Seguridad_Componente padre
    CROSS JOIN dbo.Seguridad_Componente hijo
    WHERE padre.Nombre = N'Dueño de Perfumeria'
      AND padre.Tipo = N'Familia'
      AND hijo.PermisoAsociado IN (N'Aprovisionamiento_Gestion', N'Inventario_Recepcionar', N'Ventas_Registrar')
    UNION ALL
    SELECT padre.Id, hijo.Id
    FROM dbo.Seguridad_Componente padre
    CROSS JOIN dbo.Seguridad_Componente hijo
    WHERE padre.Nombre = N'Equipo de Contabilidad'
      AND padre.Tipo = N'Familia'
      AND hijo.PermisoAsociado = N'Contabilidad_AprobarCompras'
    UNION ALL
    SELECT padre.Id, hijo.Id
    FROM dbo.Seguridad_Componente padre
    CROSS JOIN dbo.Seguridad_Componente hijo
    WHERE padre.Nombre = N'Ventas y Fidelizacion'
      AND padre.Tipo = N'Familia'
      AND hijo.PermisoAsociado = N'Ventas_Registrar'
    UNION ALL
    SELECT padre.Id, hijo.Id
    FROM dbo.Seguridad_Componente padre
    CROSS JOIN dbo.Seguridad_Componente hijo
    WHERE padre.Nombre = N'Administrador'
      AND padre.Tipo = N'Familia'
      AND hijo.PermisoAsociado IN (N'Aprovisionamiento_Gestion', N'Contabilidad_AprobarCompras', N'Inventario_Recepcionar', N'Ventas_Registrar')
)
INSERT INTO dbo.Seguridad_Familia_Hijo (IdPadre, IdHijo)
SELECT r.IdPadre, r.IdHijo
FROM Relaciones r
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Seguridad_Familia_Hijo existente
    WHERE existente.IdPadre = r.IdPadre AND existente.IdHijo = r.IdHijo
);
GO
