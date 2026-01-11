-- Script para actualizar tabla pago con nuevo modelo
-- Ejecutar en SQL Server

-- 1. Agregar columna Codigo a pago
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'Codigo')
BEGIN
    ALTER TABLE pago ADD Codigo NVARCHAR(20) NULL;
    PRINT 'Columna Codigo agregada a pago';
END
GO

-- 2. Agregar columna MateriaId a pago
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'MateriaId')
BEGIN
    ALTER TABLE pago ADD MateriaId INT NULL;
    PRINT 'Columna MateriaId agregada a pago';
END
GO

-- 3. Agregar columna TipoPago (string) a pago
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'TipoPago')
BEGIN
    ALTER TABLE pago ADD TipoPago NVARCHAR(20) NOT NULL DEFAULT 'Matricula';
    PRINT 'Columna TipoPago agregada a pago';
END
GO

-- 4. Agregar columna Descuento a pago
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'Descuento')
BEGIN
    ALTER TABLE pago ADD Descuento DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Columna Descuento agregada a pago';
END
GO

-- 5. Agregar columna MontoFinal a pago
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'MontoFinal')
BEGIN
    ALTER TABLE pago ADD MontoFinal DECIMAL(18,2) NOT NULL DEFAULT 0;
    PRINT 'Columna MontoFinal agregada a pago';
END
GO

-- 6. Agregar columna MetodoPago a pago
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'MetodoPago')
BEGIN
    ALTER TABLE pago ADD MetodoPago NVARCHAR(50) NULL;
    PRINT 'Columna MetodoPago agregada a pago';
END
GO

-- 7. Renombrar NumeroRecibo a NumeroComprobante si existe
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'NumeroRecibo')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'NumeroComprobante')
BEGIN
    EXEC sp_rename 'pago.NumeroRecibo', 'NumeroComprobante', 'COLUMN';
    PRINT 'Columna NumeroRecibo renombrada a NumeroComprobante';
END
GO

-- Si no existe NumeroComprobante, crearla
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'NumeroComprobante')
BEGIN
    ALTER TABLE pago ADD NumeroComprobante NVARCHAR(100) NULL;
    PRINT 'Columna NumeroComprobante agregada a pago';
END
GO

-- 8. Actualizar MontoFinal para pagos existentes
UPDATE pago
SET MontoFinal = Monto
WHERE MontoFinal = 0 OR MontoFinal IS NULL;
PRINT 'MontoFinal actualizado para pagos existentes';
GO

-- 9. Generar códigos para pagos existentes
UPDATE pago
SET Codigo = 'PAG-' + CAST(YEAR(FechaPago) AS VARCHAR(4)) + '-' + RIGHT('0000' + CAST(PagoId AS VARCHAR(4)), 4)
WHERE Codigo IS NULL;
PRINT 'Códigos generados para pagos existentes';
GO

-- 10. Crear índice único en Codigo de pago
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_pago_Codigo' AND object_id = OBJECT_ID('pago'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_pago_Codigo
    ON pago(Codigo)
    WHERE Codigo IS NOT NULL;
    PRINT 'Índice único creado en pago.Codigo';
END
GO

-- 11. Crear FK a materia si no existe
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_pago_materia')
BEGIN
    ALTER TABLE pago ADD CONSTRAINT FK_pago_materia
    FOREIGN KEY (MateriaId) REFERENCES materia(MateriaId);
    PRINT 'FK a materia creada';
END
GO

-- 12. Eliminar FK y columna TipoPagoId si ya no se usa
-- NOTA: Solo ejecutar después de verificar que no hay datos importantes
-- IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_pago_tipopago' OR parent_object_id = OBJECT_ID('pago'))
-- BEGIN
--     -- Obtener nombre de la FK
--     DECLARE @fkName NVARCHAR(200);
--     SELECT @fkName = name FROM sys.foreign_keys
--     WHERE parent_object_id = OBJECT_ID('pago') AND referenced_object_id = OBJECT_ID('tipo_pago');
--
--     IF @fkName IS NOT NULL
--     BEGIN
--         EXEC('ALTER TABLE pago DROP CONSTRAINT ' + @fkName);
--         PRINT 'FK a tipo_pago eliminada';
--     END
-- END
-- GO

-- IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('pago') AND name = 'TipoPagoId')
-- BEGIN
--     ALTER TABLE pago DROP COLUMN TipoPagoId;
--     PRINT 'Columna TipoPagoId eliminada de pago';
-- END
-- GO

PRINT '--- Script de actualización de pagos completado ---';
GO
