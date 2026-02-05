-- =============================================
-- Script: Convertir notas a enteros
-- Descripción: Convierte las columnas de notas de DECIMAL a INT
-- =============================================

USE [sistemaDB]
GO

PRINT '====================================================';
PRINT 'Convirtiendo columnas de notas a enteros...';
PRINT '====================================================';
PRINT '';

-- =============================================
-- 0. ELIMINAR OBJETOS DEPENDIENTES TEMPORALMENTE
-- =============================================

-- Eliminar constraint CK_Nota_RangoNota si existe
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Nota_RangoNota')
BEGIN
    ALTER TABLE dbo.nota
    DROP CONSTRAINT CK_Nota_RangoNota;
    PRINT '✓ Constraint CK_Nota_RangoNota eliminado temporalmente.';
END
GO

-- Eliminar índice IX_Nota_MatriculaCursoId si existe
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Nota_MatriculaCursoId')
BEGIN
    DROP INDEX IX_Nota_MatriculaCursoId ON dbo.nota;
    PRINT '✓ Índice IX_Nota_MatriculaCursoId eliminado temporalmente.';
END
GO

-- Eliminar DEFAULT constraints de columnas legacy
DECLARE @sql NVARCHAR(MAX);

-- Default para Nota1
IF EXISTS (SELECT * FROM sys.default_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.nota')
           AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota1'))
BEGIN
    SELECT @sql = 'ALTER TABLE dbo.nota DROP CONSTRAINT ' + name
    FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('dbo.nota')
    AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota1');

    EXEC sp_executesql @sql;
    PRINT '✓ DEFAULT constraint para Nota1 eliminado temporalmente.';
END
GO

-- Default para Nota2
DECLARE @sql NVARCHAR(MAX);
IF EXISTS (SELECT * FROM sys.default_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.nota')
           AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota2'))
BEGIN
    SELECT @sql = 'ALTER TABLE dbo.nota DROP CONSTRAINT ' + name
    FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('dbo.nota')
    AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota2');

    EXEC sp_executesql @sql;
    PRINT '✓ DEFAULT constraint para Nota2 eliminado temporalmente.';
END
GO

-- Default para Promedio
DECLARE @sql NVARCHAR(MAX);
IF EXISTS (SELECT * FROM sys.default_constraints
           WHERE parent_object_id = OBJECT_ID('dbo.nota')
           AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Promedio'))
BEGIN
    SELECT @sql = 'ALTER TABLE dbo.nota DROP CONSTRAINT ' + name
    FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID('dbo.nota')
    AND parent_column_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Promedio');

    EXEC sp_executesql @sql;
    PRINT '✓ DEFAULT constraint para Promedio eliminado temporalmente.';
END
GO

-- =============================================
-- 1. CONVERTIR COLUMNA: Nota (sistema nuevo)
-- =============================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota')
BEGIN
    -- Redondear valores existentes
    UPDATE dbo.nota
    SET Nota = ROUND(Nota, 0)
    WHERE Nota IS NOT NULL;

    PRINT '✓ Valores de Nota redondeados.';

    -- Cambiar tipo de columna a INT
    ALTER TABLE dbo.nota
    ALTER COLUMN Nota INT NULL;

    PRINT '✓ Columna Nota convertida a INT.';
END
GO

-- =============================================
-- 2. CONVERTIR COLUMNA: Nota1 (sistema legacy)
-- =============================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota1')
BEGIN
    -- Redondear valores existentes
    UPDATE dbo.nota
    SET Nota1 = ROUND(Nota1, 0)
    WHERE Nota1 IS NOT NULL;

    PRINT '✓ Valores de Nota1 redondeados.';

    -- Cambiar tipo de columna a INT
    ALTER TABLE dbo.nota
    ALTER COLUMN Nota1 INT NULL;

    PRINT '✓ Columna Nota1 convertida a INT.';
END
GO

-- =============================================
-- 3. CONVERTIR COLUMNA: Nota2 (sistema legacy)
-- =============================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota2')
BEGIN
    -- Redondear valores existentes
    UPDATE dbo.nota
    SET Nota2 = ROUND(Nota2, 0)
    WHERE Nota2 IS NOT NULL;

    PRINT '✓ Valores de Nota2 redondeados.';

    -- Cambiar tipo de columna a INT
    ALTER TABLE dbo.nota
    ALTER COLUMN Nota2 INT NULL;

    PRINT '✓ Columna Nota2 convertida a INT.';
END
GO

-- =============================================
-- 4. CONVERTIR COLUMNA: Promedio (sistema legacy)
-- =============================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Promedio')
BEGIN
    -- Redondear valores existentes
    UPDATE dbo.nota
    SET Promedio = ROUND(Promedio, 0)
    WHERE Promedio IS NOT NULL;

    PRINT '✓ Valores de Promedio redondeados.';

    -- Cambiar tipo de columna a INT
    ALTER TABLE dbo.nota
    ALTER COLUMN Promedio INT NULL;

    PRINT '✓ Columna Promedio convertida a INT.';
END
GO

-- =============================================
-- 5. RECREAR OBJETOS DEPENDIENTES
-- =============================================

-- Recrear constraint CK_Nota_RangoNota
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Nota_RangoNota')
BEGIN
    ALTER TABLE dbo.nota
    ADD CONSTRAINT CK_Nota_RangoNota
        CHECK (Nota IS NULL OR (Nota >= 0 AND Nota <= 100));

    PRINT '✓ Constraint CK_Nota_RangoNota recreado (INT, 0-100).';
END
GO

-- Recrear índice IX_Nota_MatriculaCursoId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Nota_MatriculaCursoId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Nota_MatriculaCursoId
    ON dbo.nota(MatriculaCursoId)
    INCLUDE (ComponenteEvaluacionId, Nota);

    PRINT '✓ Índice IX_Nota_MatriculaCursoId recreado.';
END
GO

-- =============================================
-- VERIFICACIÓN
-- =============================================
PRINT '';
PRINT '====================================================';
PRINT 'Verificando tipos de columnas...';
PRINT '====================================================';
PRINT '';

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'nota'
AND COLUMN_NAME IN ('Nota', 'Nota1', 'Nota2', 'Promedio')
ORDER BY COLUMN_NAME;

PRINT '';
PRINT '====================================================';
PRINT 'Conversión completada exitosamente.';
PRINT '====================================================';
