-- N03: comparison snapshots and a justified, immutable supplier decision per evaluation.
IF OBJECT_ID(N'dbo.EvaluacionProveedor', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EvaluacionProveedor
    (
        IdEvaluacion INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EvaluacionProveedor PRIMARY KEY,
        IdProducto INT NOT NULL CONSTRAINT FK_Evaluacion_Producto REFERENCES dbo.Producto(IdProducto),
        FechaEvaluacion DATETIME2 NOT NULL,
        Responsable NVARCHAR(200) NOT NULL,
        SnapshotXml NVARCHAR(MAX) NOT NULL,
        IdProveedorSeleccionado INT NULL CONSTRAINT FK_Evaluacion_Proveedor REFERENCES dbo.Proveedor(IdProveedor),
        MotivoSeleccion NVARCHAR(500) NULL,
        SeleccionadaPor NVARCHAR(200) NULL,
        FechaSeleccion DATETIME2 NULL,
        IdOrdenCompra INT NULL CONSTRAINT FK_Evaluacion_Orden REFERENCES dbo.OrdenCompra(IdOrdenCompra),
        CONSTRAINT CK_Evaluacion_Seleccion CHECK
          ((IdProveedorSeleccionado IS NULL AND MotivoSeleccion IS NULL AND SeleccionadaPor IS NULL AND FechaSeleccion IS NULL AND IdOrdenCompra IS NULL)
           OR (IdProveedorSeleccionado IS NOT NULL AND MotivoSeleccion IS NOT NULL AND LEN(MotivoSeleccion)>=10 AND SeleccionadaPor IS NOT NULL AND FechaSeleccion IS NOT NULL))
    );
    CREATE INDEX IX_Evaluacion_ProductoFecha ON dbo.EvaluacionProveedor(IdProducto, FechaEvaluacion DESC);
    CREATE UNIQUE INDEX UX_Evaluacion_Orden ON dbo.EvaluacionProveedor(IdOrdenCompra) WHERE IdOrdenCompra IS NOT NULL;
END;
GO
