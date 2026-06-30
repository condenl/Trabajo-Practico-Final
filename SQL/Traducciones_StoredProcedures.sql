-- =======================================================
-- STORED PROCEDURES PARA LA GESTIÓN DE IDIOMAS Y TRADUCCIONES
-- =======================================================

-- 1. SPs para la Tabla Idioma
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_Idioma_Insertar') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_Idioma_Insertar;
GO
CREATE PROCEDURE sp_Idioma_Insertar
    @Nombre NVARCHAR(50),
    @EsDefault BIT
AS
BEGIN
    INSERT INTO Idioma (Nombre, EsDefault) VALUES (@Nombre, @EsDefault);
END;
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_Idioma_Modificar') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_Idioma_Modificar;
GO
CREATE PROCEDURE sp_Idioma_Modificar
    @Id INT,
    @Nombre NVARCHAR(50),
    @EsDefault BIT
AS
BEGIN
    UPDATE Idioma SET Nombre = @Nombre, EsDefault = @EsDefault WHERE Id = @Id;
END;
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_Idioma_Eliminar') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_Idioma_Eliminar;
GO
CREATE PROCEDURE sp_Idioma_Eliminar
    @Id INT
AS
BEGIN
    -- Borrado en cascada manual de traducciones dependientes para mantener integridad referencial
    DELETE FROM Traduccion WHERE IdIdioma = @Id;
    DELETE FROM Idioma WHERE Id = @Id;
END;
GO

-- 2. SPs para la Tabla Traduccion
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_Traduccion_Insertar') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_Traduccion_Insertar;
GO
CREATE PROCEDURE sp_Traduccion_Insertar
    @IdIdioma INT,
    @NombreEtiqueta NVARCHAR(100),
    @Texto NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@IdIdioma, @NombreEtiqueta, @Texto);
END;
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_Traduccion_Modificar') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_Traduccion_Modificar;
GO
CREATE PROCEDURE sp_Traduccion_Modificar
    @Id INT,
    @IdIdioma INT,
    @NombreEtiqueta NVARCHAR(100),
    @Texto NVARCHAR(MAX)
AS
BEGIN
    UPDATE Traduccion SET IdIdioma = @IdIdioma, NombreEtiqueta = @NombreEtiqueta, Texto = @Texto WHERE Id = @Id;
END;
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_Traduccion_Eliminar') AND type in (N'P', N'PC'))
    DROP PROCEDURE sp_Traduccion_Eliminar;
GO
CREATE PROCEDURE sp_Traduccion_Eliminar
    @Id INT
AS
BEGIN
    DELETE FROM Traduccion WHERE Id = @Id;
END;
GO
