-- =============================================
-- Script: Limpiar datos de estudiantes y pagos
-- Descripción: Elimina todos los registros relacionados
--              con estudiantes para empezar desde cero
-- ADVERTENCIA: Este script elimina TODOS los datos de estudiantes
-- Fecha: 2026-01-24
-- =============================================

USE [SistemaVinoNuevo]
GO

PRINT '========================================'
PRINT 'INICIANDO LIMPIEZA DE DATOS'
PRINT '========================================'
PRINT ''

-- Deshabilitar restricciones de clave foránea temporalmente
PRINT 'Deshabilitando restricciones de claves foráneas...'
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
GO

-- 1. Eliminar Notas (dependen de Matriculas)
PRINT '1. Eliminando Notas...'
DELETE FROM [dbo].[Notas]
PRINT 'Notas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
GO

-- 2. Eliminar Pagos académicos (dependen de Matriculas)
PRINT '2. Eliminando Pagos académicos...'
DELETE FROM [dbo].[Pagos]
PRINT 'Pagos académicos eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
GO

-- 3. Eliminar Matrículas académicas (dependen de Estudiantes)
PRINT '3. Eliminando Matrículas académicas...'
DELETE FROM [dbo].[Matriculas]
PRINT 'Matrículas académicas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
GO

-- 4. Eliminar Pagos de cursos especializados (dependen de MatriculasCurso)
PRINT '4. Eliminando Pagos de cursos especializados...'
DELETE FROM [dbo].[PagosCurso]
PRINT 'Pagos de cursos eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
GO

-- 5. Eliminar Matrículas de cursos especializados (dependen de Estudiantes)
PRINT '5. Eliminando Matrículas de cursos especializados...'
DELETE FROM [dbo].[MatriculasCurso]
PRINT 'Matrículas de cursos eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
GO

-- 6. Eliminar EstudianteCargos (relación estudiante-cargos)
PRINT '6. Eliminando EstudianteCargos...'
DELETE FROM [dbo].[EstudianteCargos]
PRINT 'EstudianteCargos eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
GO

-- 7. Eliminar Estudiantes
PRINT '7. Eliminando Estudiantes...'
DELETE FROM [dbo].[Estudiantes]
PRINT 'Estudiantes eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
GO

-- Rehabilitar restricciones de clave foránea
PRINT ''
PRINT 'Rehabilitando restricciones de claves foráneas...'
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'
GO

-- Verificar que todo está limpio
PRINT ''
PRINT '========================================'
PRINT 'VERIFICACIÓN DE LIMPIEZA'
PRINT '========================================'

DECLARE @countNotas INT
DECLARE @countPagos INT
DECLARE @countMatriculas INT
DECLARE @countPagosCurso INT
DECLARE @countMatriculasCurso INT
DECLARE @countEstudianteCargos INT
DECLARE @countEstudiantes INT

SELECT @countNotas = COUNT(*) FROM [dbo].[Notas]
SELECT @countPagos = COUNT(*) FROM [dbo].[Pagos]
SELECT @countMatriculas = COUNT(*) FROM [dbo].[Matriculas]
SELECT @countPagosCurso = COUNT(*) FROM [dbo].[PagosCurso]
SELECT @countMatriculasCurso = COUNT(*) FROM [dbo].[MatriculasCurso]
SELECT @countEstudianteCargos = COUNT(*) FROM [dbo].[EstudianteCargos]
SELECT @countEstudiantes = COUNT(*) FROM [dbo].[Estudiantes]

PRINT 'Notas restantes: ' + CAST(@countNotas AS VARCHAR(10))
PRINT 'Pagos restantes: ' + CAST(@countPagos AS VARCHAR(10))
PRINT 'Matrículas restantes: ' + CAST(@countMatriculas AS VARCHAR(10))
PRINT 'PagosCurso restantes: ' + CAST(@countPagosCurso AS VARCHAR(10))
PRINT 'MatriculasCurso restantes: ' + CAST(@countMatriculasCurso AS VARCHAR(10))
PRINT 'EstudianteCargos restantes: ' + CAST(@countEstudianteCargos AS VARCHAR(10))
PRINT 'Estudiantes restantes: ' + CAST(@countEstudiantes AS VARCHAR(10))

IF (@countNotas = 0 AND @countPagos = 0 AND @countMatriculas = 0 AND
    @countPagosCurso = 0 AND @countMatriculasCurso = 0 AND
    @countEstudianteCargos = 0 AND @countEstudiantes = 0)
BEGIN
    PRINT ''
    PRINT '✓ LIMPIEZA COMPLETADA EXITOSAMENTE'
    PRINT '  Todos los datos de estudiantes han sido eliminados'
END
ELSE
BEGIN
    PRINT ''
    PRINT '⚠ ADVERTENCIA: Algunos registros no pudieron ser eliminados'
    PRINT '  Revisa las restricciones de claves foráneas'
END

PRINT ''
PRINT '========================================'
PRINT 'SCRIPT FINALIZADO'
PRINT '========================================'
GO
