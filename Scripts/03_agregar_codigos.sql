-- Script para agregar códigos automáticos a Estudiante y Matrícula
-- Ejecutar en SQL Server

-- 1. Agregar columna Codigo a estudiante (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('estudiante') AND name = 'Codigo')
BEGIN
    ALTER TABLE estudiante ADD Codigo NVARCHAR(20) NULL;
    PRINT 'Columna Codigo agregada a estudiante';
END
GO

-- 2. Generar códigos para estudiantes existentes
UPDATE estudiante
SET Codigo = 'EST-' + RIGHT('0000' + CAST(EstudianteId AS VARCHAR(4)), 4)
WHERE Codigo IS NULL;
PRINT 'Códigos generados para estudiantes existentes';
GO

-- 3. Crear índice único en Codigo de estudiante
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_estudiante_Codigo' AND object_id = OBJECT_ID('estudiante'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_estudiante_Codigo
    ON estudiante(Codigo)
    WHERE Codigo IS NOT NULL;
    PRINT 'Índice único creado en estudiante.Codigo';
END
GO

-- 4. Agregar columna Codigo a matricula (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('matricula') AND name = 'Codigo')
BEGIN
    ALTER TABLE matricula ADD Codigo NVARCHAR(20) NULL;
    PRINT 'Columna Codigo agregada a matricula';
END
GO

-- 5. Generar códigos para matrículas existentes
UPDATE matricula
SET Codigo = 'MAT-' + CAST(YEAR(FechaMatricula) AS VARCHAR(4)) + '-' + RIGHT('0000' + CAST(MatriculaId AS VARCHAR(4)), 4)
WHERE Codigo IS NULL;
PRINT 'Códigos generados para matrículas existentes';
GO

-- 6. Crear índice único en Codigo de matricula
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_matricula_Codigo' AND object_id = OBJECT_ID('matricula'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_matricula_Codigo
    ON matricula(Codigo)
    WHERE Codigo IS NOT NULL;
    PRINT 'Índice único creado en matricula.Codigo';
END
GO

PRINT '--- Script de códigos completado ---';
GO
