-- =============================================
-- Script: Agregar campo Observaciones a matrículas
-- Descripción: Agrega el campo Observaciones (varchar(500))
--              a las tablas Matriculas y MatriculasCurso
-- Fecha: 2026-01-24
-- =============================================

USE [SistemaVinoNuevo]
GO

-- Agregar Observaciones a tabla Matriculas (académico regular)
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID('Matriculas')
               AND name = 'Observaciones')
BEGIN
    ALTER TABLE [dbo].[Matriculas]
    ADD [Observaciones] NVARCHAR(500) NULL

    PRINT 'Campo Observaciones agregado a tabla Matriculas'
END
ELSE
BEGIN
    PRINT 'Campo Observaciones ya existe en tabla Matriculas'
END
GO

-- Agregar Observaciones a tabla MatriculasCurso (cursos especializados)
IF NOT EXISTS (SELECT * FROM sys.columns
               WHERE object_id = OBJECT_ID('MatriculasCurso')
               AND name = 'Observaciones')
BEGIN
    ALTER TABLE [dbo].[MatriculasCurso]
    ADD [Observaciones] NVARCHAR(500) NULL

    PRINT 'Campo Observaciones agregado a tabla MatriculasCurso'
END
ELSE
BEGIN
    PRINT 'Campo Observaciones ya existe en tabla MatriculasCurso'
END
GO

-- Actualizar matrículas existentes con MontoFinal = 0 (becados 100%)
UPDATE [dbo].[Matriculas]
SET [Observaciones] = 'Becado 100%'
WHERE [MontoFinal] = 0
  AND ([Observaciones] IS NULL OR [Observaciones] = '')
GO

UPDATE [dbo].[MatriculasCurso]
SET [Observaciones] = 'Becado 100%'
WHERE [MontoFinal] = 0
  AND ([Observaciones] IS NULL OR [Observaciones] = '')
GO

PRINT 'Script completado exitosamente'
