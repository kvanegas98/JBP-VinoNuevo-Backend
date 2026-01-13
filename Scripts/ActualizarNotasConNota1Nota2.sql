-- ============================================================================
-- SCRIPT PARA ACTUALIZAR TABLA NOTA CON NOTA1, NOTA2 Y PROMEDIO
-- ============================================================================
-- Este script modifica la estructura de la tabla nota para soportar
-- dos calificaciones por materia y calcular el promedio automáticamente
-- ============================================================================

-- PASO 1: Agregar columna Nota1 si no existe
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'nota' AND COLUMN_NAME = 'Nota1'
)
BEGIN
    ALTER TABLE [dbo].[nota]
    ADD [Nota1] DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Columna Nota1 agregada exitosamente'
END
ELSE
BEGIN
    PRINT 'La columna Nota1 ya existe'
END
GO

-- PASO 2: Agregar columna Nota2 si no existe
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'nota' AND COLUMN_NAME = 'Nota2'
)
BEGIN
    ALTER TABLE [dbo].[nota]
    ADD [Nota2] DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Columna Nota2 agregada exitosamente'
END
ELSE
BEGIN
    PRINT 'La columna Nota2 ya existe'
END
GO

-- PASO 3: Agregar columna Promedio si no existe
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'nota' AND COLUMN_NAME = 'Promedio'
)
BEGIN
    ALTER TABLE [dbo].[nota]
    ADD [Promedio] DECIMAL(5,2) NOT NULL DEFAULT 0;
    PRINT 'Columna Promedio agregada exitosamente'
END
ELSE
BEGIN
    PRINT 'La columna Promedio ya existe'
END
GO

-- PASO 4: Migrar datos existentes (si hay columna Calificacion)
-- Copiar el valor de Calificacion a Nota1 y calcular el promedio
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'nota' AND COLUMN_NAME = 'Calificacion'
)
BEGIN
    PRINT 'Migrando datos de Calificacion a Nota1...'

    UPDATE [dbo].[nota]
    SET Nota1 = Calificacion,
        Nota2 = 0,
        Promedio = Calificacion / 2  -- Solo Nota1 tiene valor, Nota2 = 0
    WHERE Nota1 = 0;

    PRINT 'Datos migrados: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' registros'
END
GO

-- PASO 5: (OPCIONAL) Eliminar columna Calificacion antigua
-- Descomenta las siguientes líneas si deseas eliminar la columna antigua
/*
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'nota' AND COLUMN_NAME = 'Calificacion'
)
BEGIN
    ALTER TABLE [dbo].[nota]
    DROP COLUMN [Calificacion];
    PRINT 'Columna Calificacion eliminada'
END
GO
*/

-- ============================================================================
-- VERIFICACIÓN: Mostrar estructura actual de la tabla
-- ============================================================================

PRINT ''
PRINT 'VERIFICACIÓN - Estructura de la tabla nota:'
PRINT '============================================='

SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'nota'
ORDER BY ORDINAL_POSITION;

PRINT ''
PRINT '============================================='
PRINT 'Script completado exitosamente'
PRINT '============================================='
GO
