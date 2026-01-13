-- ============================================================================
-- SCRIPT PARA AGREGAR CAMPO ORDEN A LA TABLA MATERIA
-- ============================================================================
-- Este script agrega el campo Orden para determinar el mes correspondiente
-- de cada materia dentro del módulo (usado para cálculo de mora)
-- ============================================================================

-- PASO 1: Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'materia' AND COLUMN_NAME = 'Orden'
)
BEGIN
    -- PASO 2: Agregar la columna Orden con valor por defecto 1
    ALTER TABLE [dbo].[materia]
    ADD [Orden] INT NOT NULL DEFAULT 1;

    PRINT 'Columna Orden agregada exitosamente'
END
ELSE
BEGIN
    PRINT 'La columna Orden ya existe'
END
GO

-- ============================================================================
-- PASO 3: Actualizar el orden de las materias existentes
-- Asigna orden secuencial basado en MateriaId dentro de cada módulo
-- ============================================================================

PRINT 'Actualizando orden de materias existentes...'

;WITH MateriasOrdenadas AS (
    SELECT
        MateriaId,
        ModuloId,
        ROW_NUMBER() OVER (PARTITION BY ModuloId ORDER BY MateriaId) AS NuevoOrden
    FROM [dbo].[materia]
)
UPDATE m
SET m.Orden = mo.NuevoOrden
FROM [dbo].[materia] m
INNER JOIN MateriasOrdenadas mo ON m.MateriaId = mo.MateriaId;

PRINT 'Orden de materias actualizado: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' registros'
GO

-- ============================================================================
-- VERIFICACIÓN: Mostrar las materias con su nuevo orden
-- ============================================================================

PRINT ''
PRINT 'VERIFICACIÓN - Materias ordenadas por módulo:'
PRINT '=============================================='

SELECT
    mod.Nombre AS Modulo,
    m.MateriaId,
    m.Nombre AS Materia,
    m.Orden,
    CASE
        WHEN m.Orden = 1 THEN 'Mes 1 (desde fecha matrícula)'
        ELSE 'Mes ' + CAST(m.Orden AS VARCHAR(2))
    END AS MesCorrespondiente
FROM [dbo].[materia] m
INNER JOIN [dbo].[modulo] mod ON m.ModuloId = mod.ModuloId
WHERE m.Activo = 1
ORDER BY mod.ModuloId, m.Orden;

PRINT ''
PRINT '=============================================='
PRINT 'Script completado exitosamente'
PRINT '=============================================='
GO
