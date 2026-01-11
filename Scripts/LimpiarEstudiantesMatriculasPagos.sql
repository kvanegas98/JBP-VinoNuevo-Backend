-- ============================================================================
-- SCRIPT PARA LIMPIAR ESTUDIANTES, MATRÍCULAS Y PAGOS
-- ============================================================================
-- ADVERTENCIA: Este script eliminará TODOS los datos de:
-- - Notas
-- - Pagos
-- - Matrículas
-- - Cargos de estudiantes
-- - Estudiantes
--
-- IMPORTANTE: Ejecutar con precaución. Respaldar la base de datos antes.
-- ============================================================================

USE [SistemaJBP]  -- Reemplazar con el nombre de tu base de datos
GO

PRINT '=========================================='
PRINT 'Iniciando limpieza de datos...'
PRINT '=========================================='
PRINT ''

-- ============================================================================
-- PASO 1: Eliminar NOTAS (dependen de Matrícula)
-- ============================================================================
PRINT 'PASO 1: Eliminando Notas...'

DELETE FROM [dbo].[Notas]

PRINT 'Notas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
PRINT ''

-- ============================================================================
-- PASO 2: Eliminar PAGOS (dependen de Matrícula)
-- ============================================================================
PRINT 'PASO 2: Eliminando Pagos...'

DELETE FROM [dbo].[Pagos]

PRINT 'Pagos eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
PRINT ''

-- ============================================================================
-- PASO 3: Eliminar MATRÍCULAS (dependen de Estudiante)
-- ============================================================================
PRINT 'PASO 3: Eliminando Matrículas...'

DELETE FROM [dbo].[Matriculas]

PRINT 'Matrículas eliminadas: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
PRINT ''

-- ============================================================================
-- PASO 4: Eliminar ESTUDIANTE_CARGOS (tabla intermedia)
-- ============================================================================
PRINT 'PASO 4: Eliminando Cargos de Estudiantes...'

DELETE FROM [dbo].[EstudianteCargos]

PRINT 'Cargos de estudiantes eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
PRINT ''

-- ============================================================================
-- PASO 5: Eliminar ESTUDIANTES
-- ============================================================================
PRINT 'PASO 5: Eliminando Estudiantes...'

DELETE FROM [dbo].[Estudiantes]

PRINT 'Estudiantes eliminados: ' + CAST(@@ROWCOUNT AS VARCHAR(10))
PRINT ''

-- ============================================================================
-- PASO 6: Reiniciar contadores de identidad (OPCIONAL)
-- ============================================================================
PRINT 'PASO 6: Reiniciando contadores de identidad...'

-- Reiniciar contador de Notas
DBCC CHECKIDENT ('[dbo].[Notas]', RESEED, 0)
PRINT 'Contador de Notas reiniciado'

-- Reiniciar contador de Pagos
DBCC CHECKIDENT ('[dbo].[Pagos]', RESEED, 0)
PRINT 'Contador de Pagos reiniciado'

-- Reiniciar contador de Matrículas
DBCC CHECKIDENT ('[dbo].[Matriculas]', RESEED, 0)
PRINT 'Contador de Matrículas reiniciado'

-- Reiniciar contador de EstudianteCargos
DBCC CHECKIDENT ('[dbo].[EstudianteCargos]', RESEED, 0)
PRINT 'Contador de EstudianteCargos reiniciado'

-- Reiniciar contador de Estudiantes
DBCC CHECKIDENT ('[dbo].[Estudiantes]', RESEED, 0)
PRINT 'Contador de Estudiantes reiniciado'

PRINT ''
PRINT '=========================================='
PRINT 'Limpieza completada exitosamente'
PRINT '=========================================='
PRINT ''

-- ============================================================================
-- VERIFICACIÓN: Contar registros restantes
-- ============================================================================
PRINT 'VERIFICACIÓN DE LIMPIEZA:'
PRINT '-------------------------'

DECLARE @CountEstudiantes INT = (SELECT COUNT(*) FROM [dbo].[Estudiantes])
DECLARE @CountMatriculas INT = (SELECT COUNT(*) FROM [dbo].[Matriculas])
DECLARE @CountPagos INT = (SELECT COUNT(*) FROM [dbo].[Pagos])
DECLARE @CountNotas INT = (SELECT COUNT(*) FROM [dbo].[Notas])
DECLARE @CountCargos INT = (SELECT COUNT(*) FROM [dbo].[EstudianteCargos])

PRINT 'Estudiantes restantes: ' + CAST(@CountEstudiantes AS VARCHAR(10))
PRINT 'Matrículas restantes: ' + CAST(@CountMatriculas AS VARCHAR(10))
PRINT 'Pagos restantes: ' + CAST(@CountPagos AS VARCHAR(10))
PRINT 'Notas restantes: ' + CAST(@CountNotas AS VARCHAR(10))
PRINT 'Cargos restantes: ' + CAST(@CountCargos AS VARCHAR(10))

IF @CountEstudiantes = 0 AND @CountMatriculas = 0 AND @CountPagos = 0 AND @CountNotas = 0 AND @CountCargos = 0
BEGIN
    PRINT ''
    PRINT '✓ TODAS LAS TABLAS LIMPIADAS CORRECTAMENTE'
END
ELSE
BEGIN
    PRINT ''
    PRINT '⚠ ADVERTENCIA: Aún quedan registros en algunas tablas'
END

PRINT ''
PRINT '=========================================='
GO
