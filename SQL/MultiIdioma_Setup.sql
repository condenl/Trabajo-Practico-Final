CREATE TABLE Idioma (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL,
    EsDefault BIT DEFAULT 0
);

CREATE TABLE Traduccion (
    Id INT PRIMARY KEY IDENTITY(1,1),
    IdIdioma INT NOT NULL,
    NombreEtiqueta NVARCHAR(100) NOT NULL,
    Texto NVARCHAR(MAX) NOT NULL,
    CONSTRAINT FK_Traduccion_Idioma FOREIGN KEY (IdIdioma) REFERENCES Idioma(Id)
);

CREATE INDEX IX_Traduccion_Idioma_Etiqueta ON Traduccion(IdIdioma, NombreEtiqueta);
GO

-- Initial Data
INSERT INTO Idioma (Nombre, EsDefault) VALUES ('Español', 1);
INSERT INTO Idioma (Nombre, EsDefault) VALUES ('English', 0);

-- Spanish Translations (Example keys)
DECLARE @EsId INT = (SELECT Id FROM Idioma WHERE Nombre = 'Español');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_title', 'Iniciar Sesión');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_btn', 'Ingresar');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_user', 'Usuario');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EsId, 'login_pass', 'Contraseña');

-- English Translations
DECLARE @EnId INT = (SELECT Id FROM Idioma WHERE Nombre = 'English');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_title', 'Login');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_btn', 'Enter');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_user', 'Username');
INSERT INTO Traduccion (IdIdioma, NombreEtiqueta, Texto) VALUES (@EnId, 'login_pass', 'Password');
GO

-- Stored Procedures
CREATE PROCEDURE sp_ListarIdiomas
AS
BEGIN
    SELECT Id, Nombre, EsDefault FROM Idioma;
END
GO

CREATE PROCEDURE sp_ObtenerTraduccion
    @IdIdioma INT,
    @NombreEtiqueta NVARCHAR(100)
AS
BEGIN
    SELECT Id, IdIdioma, NombreEtiqueta, Texto 
    FROM Traduccion 
    WHERE IdIdioma = @IdIdioma AND NombreEtiqueta = @NombreEtiqueta;
END
GO

CREATE PROCEDURE sp_ObtenerTraduccionesPorIdioma
    @IdIdioma INT
AS
BEGIN
    SELECT Id, IdIdioma, NombreEtiqueta, Texto 
    FROM Traduccion 
    WHERE IdIdioma = @IdIdioma;
END
GO

