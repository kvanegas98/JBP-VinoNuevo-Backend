-- =============================================
-- Script: Modificar tablas materia y CursosEspecializados
-- Descripción: Agrega campo TipoEvaluacionId para vincular
--              con el sistema de evaluación flexible
-- =============================================

USE [sistemaDB]
GO

PRINT '====================================================';
PRINT 'Modificando tablas materia y CursosEspecializados...';
PRINT '====================================================';
PRINT '';

-- =============================================
-- 1. MODIFICAR TABLA: materia
-- =============================================

-- Agregar campo TipoEvaluacionId a materia
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.materia') AND name = 'TipoEvaluacionId')
BEGIN
    -- Agregar campo como nullable primero
    ALTER TABLE dbo.materia
    ADD TipoEvaluacionId INT NULL;

    PRINT '✓ Campo TipoEvaluacionId agregado a materia.';
END
ELSE
BEGIN
    PRINT '- Campo TipoEvaluacionId ya existe en materia.';
END
GO

-- Actualizar materias existentes y hacer el campo NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.materia') AND name = 'TipoEvaluacionId' AND is_nullable = 1)
BEGIN
    -- Actualizar todas las materias existentes al tipo REGULAR (id=1)
    DECLARE @TipoRegularId INT = (SELECT TipoEvaluacionId FROM dbo.TipoEvaluacion WHERE Codigo = 'REGULAR');

    UPDATE dbo.materia
    SET TipoEvaluacionId = @TipoRegularId
    WHERE TipoEvaluacionId IS NULL;

    PRINT '✓ Materias existentes asignadas al tipo REGULAR.';

    -- Ahora hacer el campo NOT NULL
    ALTER TABLE dbo.materia
    ALTER COLUMN TipoEvaluacionId INT NOT NULL;

    PRINT '✓ Campo TipoEvaluacionId ahora es NOT NULL.';
END
GO

-- Agregar constraint con default para nuevos registros
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Materia_TipoEvaluacionId')
BEGIN
    -- Usar valor literal 1 (REGULAR) como default
    ALTER TABLE dbo.materia
    ADD CONSTRAINT DF_Materia_TipoEvaluacionId DEFAULT 1 FOR TipoEvaluacionId;

    PRINT '✓ Default constraint agregado a materia (default=1 REGULAR).';
END
GO

-- Agregar Foreign Key a materia
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Materia_TipoEvaluacion')
BEGIN
    ALTER TABLE dbo.materia
    ADD CONSTRAINT FK_Materia_TipoEvaluacion
        FOREIGN KEY (TipoEvaluacionId)
        REFERENCES dbo.TipoEvaluacion(TipoEvaluacionId);

    PRINT '✓ Foreign Key FK_Materia_TipoEvaluacion agregada.';
END
ELSE
BEGIN
    PRINT '- Foreign Key FK_Materia_TipoEvaluacion ya existe.';
END
GO

-- =============================================
-- 2. MODIFICAR TABLA: CursosEspecializados
-- =============================================

-- Agregar campo TipoEvaluacionId a CursosEspecializados
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CursosEspecializados') AND name = 'TipoEvaluacionId')
BEGIN
    -- Agregar campo como nullable primero
    ALTER TABLE dbo.CursosEspecializados
    ADD TipoEvaluacionId INT NULL;

    PRINT '✓ Campo TipoEvaluacionId agregado a CursosEspecializados.';
END
ELSE
BEGIN
    PRINT '- Campo TipoEvaluacionId ya existe en CursosEspecializados.';
END
GO

-- Actualizar cursos existentes y hacer el campo NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CursosEspecializados') AND name = 'TipoEvaluacionId' AND is_nullable = 1)
BEGIN
    -- Actualizar todos los cursos existentes al tipo ESPECIALIZADO (id=2)
    DECLARE @TipoEspecializadoId INT = (SELECT TipoEvaluacionId FROM dbo.TipoEvaluacion WHERE Codigo = 'ESPECIALIZADO');

    UPDATE dbo.CursosEspecializados
    SET TipoEvaluacionId = @TipoEspecializadoId
    WHERE TipoEvaluacionId IS NULL;

    PRINT '✓ Cursos existentes asignados al tipo ESPECIALIZADO.';

    -- Ahora hacer el campo NOT NULL
    ALTER TABLE dbo.CursosEspecializados
    ALTER COLUMN TipoEvaluacionId INT NOT NULL;

    PRINT '✓ Campo TipoEvaluacionId ahora es NOT NULL.';
END
GO

-- Agregar constraint con default para nuevos registros
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_CursoEspecializado_TipoEvaluacionId')
BEGIN
    -- Usar valor literal 2 (ESPECIALIZADO) como default
    ALTER TABLE dbo.CursosEspecializados
    ADD CONSTRAINT DF_CursoEspecializado_TipoEvaluacionId DEFAULT 2 FOR TipoEvaluacionId;

    PRINT '✓ Default constraint agregado a CursosEspecializados (default=2 ESPECIALIZADO).';
END
GO

-- Agregar Foreign Key a CursosEspecializados
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CursoEspecializado_TipoEvaluacion')
BEGIN
    ALTER TABLE dbo.CursosEspecializados
    ADD CONSTRAINT FK_CursoEspecializado_TipoEvaluacion
        FOREIGN KEY (TipoEvaluacionId)
        REFERENCES dbo.TipoEvaluacion(TipoEvaluacionId);

    PRINT '✓ Foreign Key FK_CursoEspecializado_TipoEvaluacion agregada.';
END
ELSE
BEGIN
    PRINT '- Foreign Key FK_CursoEspecializado_TipoEvaluacion ya existe.';
END
GO

-- =============================================
-- 3. VERIFICACIÓN
-- =============================================
PRINT '';
PRINT '====================================================';
PRINT 'Verificando configuración...';
PRINT '====================================================';
PRINT '';

-- Ver distribución de materias por tipo
SELECT
    te.Nombre as TipoEvaluacion,
    COUNT(*) as CantidadMaterias
FROM dbo.materia m
INNER JOIN dbo.TipoEvaluacion te ON m.TipoEvaluacionId = te.TipoEvaluacionId
GROUP BY te.Nombre;

-- Ver distribución de cursos por tipo
SELECT
    te.Nombre as TipoEvaluacion,
    COUNT(*) as CantidadCursos
FROM dbo.CursosEspecializados c
INNER JOIN dbo.TipoEvaluacion te ON c.TipoEvaluacionId = te.TipoEvaluacionId
GROUP BY te.Nombre;

PRINT '';
PRINT '====================================================';
PRINT 'Modificación completada exitosamente.';
PRINT '====================================================';
