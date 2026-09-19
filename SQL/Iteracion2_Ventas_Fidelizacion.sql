IF OBJECT_ID(N'dbo.Cliente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cliente
    (
        IdCliente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Cliente PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Apellido NVARCHAR(100) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        Telefono NVARCHAR(50) NULL,
        PreferenciasOlfativas NVARCHAR(500) NULL,
        AutorizaMarketing BIT NOT NULL CONSTRAINT DF_Cliente_AutorizaMarketing DEFAULT (0),
        FechaAlta DATETIME NOT NULL CONSTRAINT DF_Cliente_FechaAlta DEFAULT (GETDATE()),
        Activo BIT NOT NULL CONSTRAINT DF_Cliente_Activo DEFAULT (1)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Cliente') AND name = N'UX_Cliente_Email')
    CREATE UNIQUE INDEX UX_Cliente_Email ON dbo.Cliente(Email);
GO

IF OBJECT_ID(N'dbo.Membresia', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Membresia
    (
        IdMembresia INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Membresia PRIMARY KEY,
        IdCliente INT NOT NULL,
        Numero NVARCHAR(30) NOT NULL,
        FechaAlta DATETIME NOT NULL CONSTRAINT DF_Membresia_FechaAlta DEFAULT (GETDATE()),
        PorcentajeDescuento DECIMAL(5,2) NOT NULL CONSTRAINT DF_Membresia_Descuento DEFAULT (10),
        Activa BIT NOT NULL CONSTRAINT DF_Membresia_Activa DEFAULT (1),
        CONSTRAINT FK_Membresia_Cliente FOREIGN KEY (IdCliente) REFERENCES dbo.Cliente(IdCliente),
        CONSTRAINT CK_Membresia_Descuento CHECK (PorcentajeDescuento BETWEEN 0 AND 30)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Membresia') AND name = N'UX_Membresia_Numero')
    CREATE UNIQUE INDEX UX_Membresia_Numero ON dbo.Membresia(Numero);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Membresia') AND name = N'UX_Membresia_ClienteActiva')
    CREATE UNIQUE INDEX UX_Membresia_ClienteActiva ON dbo.Membresia(IdCliente) WHERE Activa = 1;
GO

IF OBJECT_ID(N'dbo.Venta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Venta
    (
        IdVenta INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Venta PRIMARY KEY,
        IdCliente INT NULL,
        IdUsuarioVendedor INT NULL,
        Fecha DATETIME NOT NULL CONSTRAINT DF_Venta_Fecha DEFAULT (GETDATE()),
        Subtotal DECIMAL(18,2) NOT NULL,
        Descuento DECIMAL(18,2) NOT NULL CONSTRAINT DF_Venta_Descuento DEFAULT (0),
        Total DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_Venta_Cliente FOREIGN KEY (IdCliente) REFERENCES dbo.Cliente(IdCliente),
        CONSTRAINT FK_Venta_Usuario FOREIGN KEY (IdUsuarioVendedor) REFERENCES dbo.Usuarios(Id),
        CONSTRAINT CK_Venta_Importes CHECK (Subtotal >= 0 AND Descuento >= 0 AND Total >= 0 AND Total = Subtotal - Descuento)
    );
END;
GO

IF OBJECT_ID(N'dbo.DetalleVenta', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DetalleVenta
    (
        IdDetalleVenta INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetalleVenta PRIMARY KEY,
        IdVenta INT NOT NULL,
        IdProducto INT NOT NULL,
        IdLote INT NOT NULL,
        Cantidad INT NOT NULL,
        PrecioUnitario DECIMAL(18,2) NOT NULL,
        DescuentoUnitario DECIMAL(18,2) NOT NULL CONSTRAINT DF_DetalleVenta_Descuento DEFAULT (0),
        CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY (IdVenta) REFERENCES dbo.Venta(IdVenta),
        CONSTRAINT FK_DetalleVenta_Producto FOREIGN KEY (IdProducto) REFERENCES dbo.Producto(IdProducto),
        CONSTRAINT FK_DetalleVenta_Lote FOREIGN KEY (IdLote) REFERENCES dbo.Lote(IdLote),
        CONSTRAINT CK_DetalleVenta_Cantidad CHECK (Cantidad > 0),
        CONSTRAINT CK_DetalleVenta_Importes CHECK (PrecioUnitario >= 0 AND DescuentoUnitario >= 0 AND DescuentoUnitario <= PrecioUnitario)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Venta') AND name = N'IX_Venta_ClienteFecha')
    CREATE INDEX IX_Venta_ClienteFecha ON dbo.Venta(IdCliente, Fecha DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.DetalleVenta') AND name = N'IX_DetalleVenta_Lote')
    CREATE INDEX IX_DetalleVenta_Lote ON dbo.DetalleVenta(IdLote);
GO
