-- =============================================
-- Script: Sistema de Evaluación Flexible
-- Descripción: Crea tablas para gestionar evaluaciones
--              de Materias Académicas y Cursos Especializados
-- =============================================

USE [sistemaDB]
GO

-- =============================================
-- 1. TABLA: TipoEvaluacion (Catálogo)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TipoEvaluacion')
BEGIN
    CREATE TABLE dbo.TipoEvaluacion (
        TipoEvaluacionId INT PRIMARY KEY IDENTITY(1,1),
        Codigo NVARCHAR(50) NOT NULL UNIQUE,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        CantidadComponentes INT NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );

    PRINT 'Tabla TipoEvaluacion creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla TipoEvaluacion ya existe.';
END
GO

-- =============================================
-- 2. TABLA: ComponenteEvaluacion (Catálogo)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ComponenteEvaluacion')
BEGIN
    CREATE TABLE dbo.ComponenteEvaluacion (
        ComponenteEvaluacionId INT PRIMARY KEY IDENTITY(1,1),
        TipoEvaluacionId INT NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        PorcentajePeso DECIMAL(5,2) NOT NULL,
        Orden INT NOT NULL,
        NotaMinima DECIMAL(5,2) NULL,
        EsObligatorio BIT NOT NULL DEFAULT 1,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),

        -- Foreign Key
        CONSTRAINT FK_ComponenteEvaluacion_TipoEvaluacion
            FOREIGN KEY (TipoEvaluacionId)
            REFERENCES dbo.TipoEvaluacion(TipoEvaluacionId),

        -- Constraints
        CONSTRAINT CK_ComponenteEvaluacion_PorcentajePeso
            CHECK (PorcentajePeso >= 0 AND PorcentajePeso <= 100),

        CONSTRAINT CK_ComponenteEvaluacion_NotaMinima
            CHECK (NotaMinima IS NULL OR (NotaMinima >= 0 AND NotaMinima <= 100)),

        -- Índice único para evitar duplicados
        CONSTRAINT UQ_ComponenteEvaluacion_TipoNombre
            UNIQUE (TipoEvaluacionId, Nombre)
    );

    PRINT 'Tabla ComponenteEvaluacion creada correctamente.';
END
ELSE
BEGIN
    PRINT 'La tabla ComponenteEvaluacion ya existe.';
END
GO

-- =============================================
-- 3. INSERTAR DATOS INICIALES: TipoEvaluacion
-- =============================================
IF NOT EXISTS (SELECT * FROM dbo.TipoEvaluacion WHERE Codigo = 'REGULAR')
BEGIN
    INSERT INTO dbo.TipoEvaluacion (Codigo, Nombre, Descripcion, CantidadComponentes, Activo)
    VALUES
        ('REGULAR', 'Curso Regular', 'Evaluación de materias académicas regulares con 3 componentes', 3, 1);

    PRINT 'Tipo de evaluación REGULAR insertado.';
END
ELSE
BEGIN
    PRINT 'Tipo de evaluación REGULAR ya existe.';
END
GO

IF NOT EXISTS (SELECT * FROM dbo.TipoEvaluacion WHERE Codigo = 'ESPECIALIZADO')
BEGIN
    INSERT INTO dbo.TipoEvaluacion (Codigo, Nombre, Descripcion, CantidadComponentes, Activo)
    VALUES
        ('ESPECIALIZADO', 'Curso Especializado', 'Evaluación de cursos especializados con 2 componentes', 2, 1);

    PRINT 'Tipo de evaluación ESPECIALIZADO insertado.';
END
ELSE
BEGIN
    PRINT 'Tipo de evaluación ESPECIALIZADO ya existe.';
END
GO

-- =============================================
-- 4. INSERTAR DATOS INICIALES: ComponenteEvaluacion
-- =============================================

-- Componentes para CURSO REGULAR (TipoEvaluacionId = 1)
DECLARE @TipoRegularId INT = (SELECT TipoEvaluacionId FROM dbo.TipoEvaluacion WHERE Codigo = 'REGULAR');

IF NOT EXISTS (SELECT * FROM dbo.ComponenteEvaluacion WHERE TipoEvaluacionId = @TipoRegularId AND Nombre = 'Examen 1')
BEGIN
    INSERT INTO dbo.ComponenteEvaluacion (TipoEvaluacionId, Nombre, PorcentajePeso, Orden, EsObligatorio, Activo)
    VALUES
        (@TipoRegularId, 'Examen 1', 40.00, 1, 1, 1),
        (@TipoRegularId, 'Examen 2', 40.00, 2, 1, 1),
        (@TipoRegularId, 'Proyecto Final', 20.00, 3, 1, 1);

    PRINT 'Componentes de evaluación REGULAR insertados (Examen 1: 40%, Examen 2: 40%, Proyecto: 20%).';
END
ELSE
BEGIN
    PRINT 'Componentes de evaluación REGULAR ya existen.';
END
GO

-- Componentes para CURSO ESPECIALIZADO (TipoEvaluacionId = 2)
DECLARE @TipoEspecializadoId INT = (SELECT TipoEvaluacionId FROM dbo.TipoEvaluacion WHERE Codigo = 'ESPECIALIZADO');

IF NOT EXISTS (SELECT * FROM dbo.ComponenteEvaluacion WHERE TipoEvaluacionId = @TipoEspecializadoId AND Nombre = 'Nota Parcial 1')
BEGIN
    INSERT INTO dbo.ComponenteEvaluacion (TipoEvaluacionId, Nombre, PorcentajePeso, Orden, EsObligatorio, Activo)
    VALUES
        (@TipoEspecializadoId, 'Nota Parcial 1', 50.00, 1, 1, 1),
        (@TipoEspecializadoId, 'Nota Parcial 2', 50.00, 2, 1, 1);

    PRINT 'Componentes de evaluación ESPECIALIZADO insertados (Parcial 1: 50%, Parcial 2: 50%).';
END
ELSE
BEGIN
    PRINT 'Componentes de evaluación ESPECIALIZADO ya existen.';
END
GO

-- =============================================
-- 5. VALIDACIÓN: Verificar que los porcentajes sumen 100%
-- =============================================
SELECT
    te.Codigo,
    te.Nombre,
    SUM(ce.PorcentajePeso) as TotalPorcentaje,
    CASE
        WHEN SUM(ce.PorcentajePeso) = 100 THEN 'OK ✓'
        ELSE 'ERROR - No suma 100%'
    END as Validacion
FROM dbo.TipoEvaluacion te
INNER JOIN dbo.ComponenteEvaluacion ce ON te.TipoEvaluacionId = ce.TipoEvaluacionId
WHERE ce.Activo = 1
GROUP BY te.TipoEvaluacionId, te.Codigo, te.Nombre;
GO

PRINT '';
PRINT '====================================================';
PRINT 'Tablas de catálogo creadas e inicializadas correctamente.';
PRINT '====================================================';
