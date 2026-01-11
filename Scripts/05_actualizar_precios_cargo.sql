-- Script para actualizar tablas de precios con CargoId
-- Permite definir precios por Categoría + Cargo
-- Ejecutar en SQL Server

-- =============================================
-- 1. ACTUALIZAR TABLA precio_matricula
-- =============================================

-- Agregar columna CargoId a precio_matricula
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('precio_matricula') AND name = 'CargoId')
BEGIN
    ALTER TABLE precio_matricula ADD CargoId INT NULL;
    PRINT 'Columna CargoId agregada a precio_matricula';
END
GO

-- Eliminar índice único anterior si existe
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_precio_matricula_CategoriaEstudianteId' AND object_id = OBJECT_ID('precio_matricula'))
BEGIN
    DROP INDEX IX_precio_matricula_CategoriaEstudianteId ON precio_matricula;
    PRINT 'Índice anterior eliminado de precio_matricula';
END
GO

-- Crear FK a cargo si no existe
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_precio_matricula_cargo')
BEGIN
    ALTER TABLE precio_matricula ADD CONSTRAINT FK_precio_matricula_cargo
    FOREIGN KEY (CargoId) REFERENCES cargo(CargoId);
    PRINT 'FK a cargo creada en precio_matricula';
END
GO

-- Crear nuevo índice único compuesto (Categoría + Cargo)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_precio_matricula_Categoria_Cargo' AND object_id = OBJECT_ID('precio_matricula'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_precio_matricula_Categoria_Cargo
    ON precio_matricula(CategoriaEstudianteId, CargoId);
    PRINT 'Índice único compuesto creado en precio_matricula';
END
GO

-- =============================================
-- 2. ACTUALIZAR TABLA precio_mensualidad
-- =============================================

-- Agregar columna CargoId a precio_mensualidad
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('precio_mensualidad') AND name = 'CargoId')
BEGIN
    ALTER TABLE precio_mensualidad ADD CargoId INT NULL;
    PRINT 'Columna CargoId agregada a precio_mensualidad';
END
GO

-- Eliminar índice único anterior si existe
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_precio_mensualidad_CategoriaEstudianteId' AND object_id = OBJECT_ID('precio_mensualidad'))
BEGIN
    DROP INDEX IX_precio_mensualidad_CategoriaEstudianteId ON precio_mensualidad;
    PRINT 'Índice anterior eliminado de precio_mensualidad';
END
GO

-- Crear FK a cargo si no existe
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_precio_mensualidad_cargo')
BEGIN
    ALTER TABLE precio_mensualidad ADD CONSTRAINT FK_precio_mensualidad_cargo
    FOREIGN KEY (CargoId) REFERENCES cargo(CargoId);
    PRINT 'FK a cargo creada en precio_mensualidad';
END
GO

-- Crear nuevo índice único compuesto (Categoría + Cargo)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_precio_mensualidad_Categoria_Cargo' AND object_id = OBJECT_ID('precio_mensualidad'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_precio_mensualidad_Categoria_Cargo
    ON precio_mensualidad(CategoriaEstudianteId, CargoId);
    PRINT 'Índice único compuesto creado en precio_mensualidad';
END
GO

-- =============================================
-- 3. LIMPIAR DATOS ANTIGUOS (OPCIONAL)
-- =============================================
-- Descomentar si desea eliminar los precios existentes y comenzar desde cero

-- DELETE FROM precio_matricula;
-- DELETE FROM precio_mensualidad;
-- PRINT 'Datos de precios eliminados';
-- GO

-- =============================================
-- 4. INSERTAR DATOS DE EJEMPLO
-- =============================================
-- NOTA: Ajustar los IDs según tu base de datos
-- Ejecutar los SELECT para obtener los IDs correctos:

-- SELECT CategoriaEstudianteId, Nombre FROM categoria_estudiante;
-- SELECT CargoId, Nombre FROM cargo;

-- Ejemplo de estructura de precios:
-- | Categoría | Cargo           | Matrícula  | Mensualidad |
-- |-----------|-----------------|------------|-------------|
-- | Externo   | NULL            | $15.00     | $15.00      |
-- | Interno   | Discípulo       | $15.00     | $15.00      |
-- | Interno   | Evangelista     | $5.00      | $5.00       |
-- | Interno   | Líder Casa Paz  | $7.50      | $7.50       |
-- | Interno   | Matrimonio      | $7.50      | $7.50       |
-- | Interno   | Becado          | $0.00      | $0.00       |

-- Descomenta y ajusta los IDs según tu base de datos:

/*
-- Obtener IDs de categorías
DECLARE @CategoriaExterno INT = (SELECT CategoriaEstudianteId FROM categoria_estudiante WHERE Nombre = 'Externo');
DECLARE @CategoriaInterno INT = (SELECT CategoriaEstudianteId FROM categoria_estudiante WHERE Nombre = 'Interno');

-- Obtener IDs de cargos
DECLARE @CargoBecado INT = (SELECT CargoId FROM cargo WHERE Nombre LIKE '%Becado%');
DECLARE @CargoDiscipulo INT = (SELECT CargoId FROM cargo WHERE Nombre LIKE '%Discípulo%');
DECLARE @CargoEvangelista INT = (SELECT CargoId FROM cargo WHERE Nombre LIKE '%Evangelista%');
DECLARE @CargoLider INT = (SELECT CargoId FROM cargo WHERE Nombre LIKE '%Líder%');
DECLARE @CargoMatrimonio INT = (SELECT CargoId FROM cargo WHERE Nombre LIKE '%Matrimonio%');

-- Insertar precios de matrícula
-- Externo (sin cargo)
INSERT INTO precio_matricula (CategoriaEstudianteId, CargoId, Precio, Activo)
VALUES (@CategoriaExterno, NULL, 15.00, 1);

-- Interno con diferentes cargos
INSERT INTO precio_matricula (CategoriaEstudianteId, CargoId, Precio, Activo)
VALUES
    (@CategoriaInterno, @CargoDiscipulo, 15.00, 1),
    (@CategoriaInterno, @CargoEvangelista, 5.00, 1),
    (@CategoriaInterno, @CargoLider, 7.50, 1),
    (@CategoriaInterno, @CargoMatrimonio, 7.50, 1),
    (@CategoriaInterno, @CargoBecado, 0.00, 1);

-- Insertar precios de mensualidad
-- Externo (sin cargo)
INSERT INTO precio_mensualidad (CategoriaEstudianteId, CargoId, Precio, Activo)
VALUES (@CategoriaExterno, NULL, 15.00, 1);

-- Interno con diferentes cargos
INSERT INTO precio_mensualidad (CategoriaEstudianteId, CargoId, Precio, Activo)
VALUES
    (@CategoriaInterno, @CargoDiscipulo, 15.00, 1),
    (@CategoriaInterno, @CargoEvangelista, 5.00, 1),
    (@CategoriaInterno, @CargoLider, 7.50, 1),
    (@CategoriaInterno, @CargoMatrimonio, 7.50, 1),
    (@CategoriaInterno, @CargoBecado, 0.00, 1);

PRINT 'Precios de ejemplo insertados';
*/

PRINT '--- Script de actualización de precios completado ---';
PRINT 'IMPORTANTE: Revisa y ejecuta la sección 4 para insertar los precios de ejemplo';
GO
