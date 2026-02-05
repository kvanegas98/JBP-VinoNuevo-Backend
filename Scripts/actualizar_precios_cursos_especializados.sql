-- ===================================================================
-- SCRIPT: Actualizar estructura de precios para cursos especializados
-- FECHA: 2026-01-19
-- DESCRIPCIÓN: Cambiar de precios por curso+modalidad+categoría a precios globales por categoría+cargo (igual que sistema académico)
-- ===================================================================

USE [VinoNuevoJBP]
GO

BEGIN TRANSACTION;

PRINT '====================================================================';
PRINT 'INICIO: Actualización de estructura de precios para cursos especializados';
PRINT '====================================================================';

-- ===================================================================
-- PASO 1: Limpiar tabla PreciosMatriculaCurso (datos existentes)
-- ===================================================================
PRINT '';
PRINT 'PASO 1: Limpiando datos existentes...';

DELETE FROM [PreciosMatriculaCurso];
PRINT '  - Registros en PreciosMatriculaCurso eliminados.';

DELETE FROM [PreciosMensualidadCurso];
PRINT '  - Registros en PreciosMensualidadCurso eliminados.';

-- ===================================================================
-- PASO 2: Modificar tabla PreciosMatriculaCurso
-- ===================================================================

PRINT '';
PRINT 'PASO 2: Modificando tabla PreciosMatriculaCurso...';

-- 2.1: Eliminar TODOS los índices de la tabla
DECLARE @sql NVARCHAR(MAX) = '';

-- Eliminar índices
SELECT @sql = @sql + 'DROP INDEX [' + i.name + '] ON [PreciosMatriculaCurso];' + CHAR(13) + CHAR(10)
FROM sys.indexes i
INNER JOIN sys.objects o ON i.object_id = o.object_id
WHERE o.name = 'PreciosMatriculaCurso'
  AND i.name IS NOT NULL
  AND i.is_primary_key = 0
  AND i.is_unique_constraint = 0;

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '  - Índices eliminados.';
END

-- 2.2: Eliminar TODAS las foreign keys de la tabla
SET @sql = '';

SELECT @sql = @sql + 'ALTER TABLE [PreciosMatriculaCurso] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13) + CHAR(10)
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID('PreciosMatriculaCurso');

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '  - Foreign keys eliminadas.';
END

-- 2.3: Eliminar TODOS los constraints DEFAULT
SET @sql = '';

SELECT @sql = @sql + 'ALTER TABLE [PreciosMatriculaCurso] DROP CONSTRAINT [' + dc.name + '];' + CHAR(13) + CHAR(10)
FROM sys.default_constraints dc
WHERE dc.parent_object_id = OBJECT_ID('PreciosMatriculaCurso');

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '  - Constraints DEFAULT eliminados.';
END

-- 2.4: Eliminar columnas antiguas
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMatriculaCurso') AND name = 'CursoEspecializadoId')
BEGIN
    ALTER TABLE [PreciosMatriculaCurso] DROP COLUMN [CursoEspecializadoId];
    PRINT '  - Columna CursoEspecializadoId eliminada.';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMatriculaCurso') AND name = 'ModalidadId')
BEGIN
    ALTER TABLE [PreciosMatriculaCurso] DROP COLUMN [ModalidadId];
    PRINT '  - Columna ModalidadId eliminada.';
END

-- 2.5: Renombrar columna Monto a Precio
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMatriculaCurso') AND name = 'Monto')
BEGIN
    EXEC sp_rename 'PreciosMatriculaCurso.Monto', 'Precio', 'COLUMN';
    PRINT '  - Columna Monto renombrada a Precio.';
END

-- 2.6: Agregar nueva columna CargoId
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMatriculaCurso') AND name = 'CargoId')
BEGIN
    ALTER TABLE [PreciosMatriculaCurso] ADD [CargoId] INT NULL;
    PRINT '  - Columna CargoId agregada.';
END

-- 2.7: Recrear foreign key a CategoriaEstudiante (por si se eliminó)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PreciosMatriculaCurso_CategoriasEstudiante_CategoriaEstudianteId')
BEGIN
    ALTER TABLE [PreciosMatriculaCurso]
        ADD CONSTRAINT [FK_PreciosMatriculaCurso_CategoriasEstudiante_CategoriaEstudianteId]
        FOREIGN KEY ([CategoriaEstudianteId]) REFERENCES [CategoriasEstudiante]([CategoriaEstudianteId]) ON DELETE CASCADE;
    PRINT '  - FK_PreciosMatriculaCurso_CategoriasEstudiante_CategoriaEstudianteId creada.';
END

-- 2.8: Agregar foreign key a Cargos
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PreciosMatriculaCurso_Cargos_CargoId')
BEGIN
    ALTER TABLE [PreciosMatriculaCurso]
        ADD CONSTRAINT [FK_PreciosMatriculaCurso_Cargos_CargoId]
        FOREIGN KEY ([CargoId]) REFERENCES [Cargos]([CargoId]);
    PRINT '  - FK_PreciosMatriculaCurso_Cargos_CargoId creada.';
END

-- 2.9: Crear índice en CategoriaEstudianteId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PreciosMatriculaCurso_CategoriaEstudianteId' AND object_id = OBJECT_ID('PreciosMatriculaCurso'))
BEGIN
    CREATE INDEX [IX_PreciosMatriculaCurso_CategoriaEstudianteId] ON [PreciosMatriculaCurso]([CategoriaEstudianteId]);
    PRINT '  - Índice IX_PreciosMatriculaCurso_CategoriaEstudianteId creado.';
END

-- 2.10: Crear índice en CargoId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PreciosMatriculaCurso_CargoId' AND object_id = OBJECT_ID('PreciosMatriculaCurso'))
BEGIN
    CREATE INDEX [IX_PreciosMatriculaCurso_CargoId] ON [PreciosMatriculaCurso]([CargoId]);
    PRINT '  - Índice IX_PreciosMatriculaCurso_CargoId creado.';
END

PRINT '  ✓ Tabla PreciosMatriculaCurso actualizada correctamente.';

-- ===================================================================
-- PASO 3: Modificar tabla PreciosMensualidadCurso
-- ===================================================================

PRINT '';
PRINT 'PASO 3: Modificando tabla PreciosMensualidadCurso...';

-- 3.1: Eliminar TODOS los índices
SET @sql = '';

SELECT @sql = @sql + 'DROP INDEX [' + i.name + '] ON [PreciosMensualidadCurso];' + CHAR(13) + CHAR(10)
FROM sys.indexes i
INNER JOIN sys.objects o ON i.object_id = o.object_id
WHERE o.name = 'PreciosMensualidadCurso'
  AND i.name IS NOT NULL
  AND i.is_primary_key = 0
  AND i.is_unique_constraint = 0;

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '  - Índices eliminados.';
END

-- 3.2: Eliminar TODAS las foreign keys
SET @sql = '';

SELECT @sql = @sql + 'ALTER TABLE [PreciosMensualidadCurso] DROP CONSTRAINT [' + fk.name + '];' + CHAR(13) + CHAR(10)
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID('PreciosMensualidadCurso');

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '  - Foreign keys eliminadas.';
END

-- 3.3: Eliminar TODOS los constraints DEFAULT
SET @sql = '';

SELECT @sql = @sql + 'ALTER TABLE [PreciosMensualidadCurso] DROP CONSTRAINT [' + dc.name + '];' + CHAR(13) + CHAR(10)
FROM sys.default_constraints dc
WHERE dc.parent_object_id = OBJECT_ID('PreciosMensualidadCurso');

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '  - Constraints DEFAULT eliminados.';
END

-- 3.4: Eliminar columnas antiguas
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMensualidadCurso') AND name = 'CursoEspecializadoId')
BEGIN
    ALTER TABLE [PreciosMensualidadCurso] DROP COLUMN [CursoEspecializadoId];
    PRINT '  - Columna CursoEspecializadoId eliminada.';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMensualidadCurso') AND name = 'ModalidadId')
BEGIN
    ALTER TABLE [PreciosMensualidadCurso] DROP COLUMN [ModalidadId];
    PRINT '  - Columna ModalidadId eliminada.';
END

-- 3.5: Renombrar columna Monto a Precio
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMensualidadCurso') AND name = 'Monto')
BEGIN
    EXEC sp_rename 'PreciosMensualidadCurso.Monto', 'Precio', 'COLUMN';
    PRINT '  - Columna Monto renombrada a Precio.';
END

-- 3.6: Agregar nueva columna CargoId
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PreciosMensualidadCurso') AND name = 'CargoId')
BEGIN
    ALTER TABLE [PreciosMensualidadCurso] ADD [CargoId] INT NULL;
    PRINT '  - Columna CargoId agregada.';
END

-- 3.7: Recrear foreign key a CategoriaEstudiante
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PreciosMensualidadCurso_CategoriasEstudiante_CategoriaEstudianteId')
BEGIN
    ALTER TABLE [PreciosMensualidadCurso]
        ADD CONSTRAINT [FK_PreciosMensualidadCurso_CategoriasEstudiante_CategoriaEstudianteId]
        FOREIGN KEY ([CategoriaEstudianteId]) REFERENCES [CategoriasEstudiante]([CategoriaEstudianteId]) ON DELETE CASCADE;
    PRINT '  - FK_PreciosMensualidadCurso_CategoriasEstudiante_CategoriaEstudianteId creada.';
END

-- 3.8: Agregar foreign key a Cargos
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PreciosMensualidadCurso_Cargos_CargoId')
BEGIN
    ALTER TABLE [PreciosMensualidadCurso]
        ADD CONSTRAINT [FK_PreciosMensualidadCurso_Cargos_CargoId]
        FOREIGN KEY ([CargoId]) REFERENCES [Cargos]([CargoId]);
    PRINT '  - FK_PreciosMensualidadCurso_Cargos_CargoId creada.';
END

-- 3.9: Crear índice en CategoriaEstudianteId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PreciosMensualidadCurso_CategoriaEstudianteId' AND object_id = OBJECT_ID('PreciosMensualidadCurso'))
BEGIN
    CREATE INDEX [IX_PreciosMensualidadCurso_CategoriaEstudianteId] ON [PreciosMensualidadCurso]([CategoriaEstudianteId]);
    PRINT '  - Índice IX_PreciosMensualidadCurso_CategoriaEstudianteId creado.';
END

-- 3.10: Crear índice en CargoId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PreciosMensualidadCurso_CargoId' AND object_id = OBJECT_ID('PreciosMensualidadCurso'))
BEGIN
    CREATE INDEX [IX_PreciosMensualidadCurso_CargoId] ON [PreciosMensualidadCurso]([CargoId]);
    PRINT '  - Índice IX_PreciosMensualidadCurso_CargoId creado.';
END

PRINT '  ✓ Tabla PreciosMensualidadCurso actualizada correctamente.';

-- ===================================================================
-- FINALIZAR
-- ===================================================================

COMMIT TRANSACTION;

PRINT '';
PRINT '====================================================================';
PRINT 'ÉXITO: Estructura de precios actualizada correctamente';
PRINT '====================================================================';
PRINT '';
PRINT 'RESUMEN DE CAMBIOS:';
PRINT '  - Datos anteriores eliminados (se deben reconfigurar los precios)';
PRINT '  - PreciosMatriculaCurso: Ahora usa CategoriaEstudianteId + CargoId';
PRINT '  - PreciosMensualidadCurso: Ahora usa CategoriaEstudianteId + CargoId';
PRINT '  - Los precios son globales, no por curso específico';
PRINT '  - Igual que el sistema académico normal (PrecioMatricula, PrecioMensualidad)';
PRINT '';
GO
