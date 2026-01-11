-- Script para agregar la tabla Modulo y modificar las relaciones
-- Ejecutar en SQL Server

-- 1. Crear tabla modulo
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='modulo' AND xtype='U')
BEGIN
    CREATE TABLE modulo (
        ModuloId INT IDENTITY(1,1) PRIMARY KEY,
        AnioLectivoId INT NOT NULL,
        Numero INT NOT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_modulo_aniolectivo FOREIGN KEY (AnioLectivoId)
            REFERENCES anio_lectivo(AnioLectivoId),
        CONSTRAINT UQ_modulo_anio_numero UNIQUE (AnioLectivoId, Numero)
    );
    PRINT 'Tabla modulo creada correctamente';
END
ELSE
BEGIN
    PRINT 'La tabla modulo ya existe';
END
GO

-- 2. Insertar módulos por defecto para cada año lectivo existente
INSERT INTO modulo (AnioLectivoId, Numero, Nombre, Activo)
SELECT
    al.AnioLectivoId,
    n.Numero,
    'Módulo ' + CAST(n.Numero AS NVARCHAR(10)),
    1
FROM anio_lectivo al
CROSS JOIN (SELECT 1 AS Numero UNION SELECT 2 UNION SELECT 3) n
WHERE NOT EXISTS (
    SELECT 1 FROM modulo m
    WHERE m.AnioLectivoId = al.AnioLectivoId AND m.Numero = n.Numero
);
PRINT 'Módulos por defecto insertados';
GO

-- 3. Agregar columna ModuloId a materia (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('materia') AND name = 'ModuloId')
BEGIN
    ALTER TABLE materia ADD ModuloId INT NULL;
    PRINT 'Columna ModuloId agregada a materia';
END
GO

-- 4. Migrar datos de materia: asignar al módulo 1 del año lectivo correspondiente
UPDATE m
SET m.ModuloId = mod.ModuloId
FROM materia m
INNER JOIN modulo mod ON mod.AnioLectivoId = m.AnioLectivoId AND mod.Numero = 1
WHERE m.ModuloId IS NULL;
PRINT 'Materias migradas al módulo 1';
GO

-- 5. Hacer ModuloId NOT NULL y agregar FK en materia
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('materia') AND name = 'ModuloId')
BEGIN
    -- Primero verificar que no haya nulls
    IF NOT EXISTS (SELECT 1 FROM materia WHERE ModuloId IS NULL)
    BEGIN
        ALTER TABLE materia ALTER COLUMN ModuloId INT NOT NULL;

        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_materia_modulo')
        BEGIN
            ALTER TABLE materia ADD CONSTRAINT FK_materia_modulo
                FOREIGN KEY (ModuloId) REFERENCES modulo(ModuloId);
        END
        PRINT 'FK de materia a modulo creada';
    END
END
GO

-- 6. Eliminar FK y columna AnioLectivoId de materia (opcional - si deseas limpiar)
-- NOTA: Descomentar solo después de verificar que la migración fue exitosa
/*
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_materia_aniolectivo')
BEGIN
    ALTER TABLE materia DROP CONSTRAINT FK_materia_aniolectivo;
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('materia') AND name = 'AnioLectivoId')
BEGIN
    ALTER TABLE materia DROP COLUMN AnioLectivoId;
    PRINT 'Columna AnioLectivoId eliminada de materia';
END
*/
GO

-- 7. Agregar columna ModuloId a matricula (si no existe)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('matricula') AND name = 'ModuloId')
BEGIN
    ALTER TABLE matricula ADD ModuloId INT NULL;
    PRINT 'Columna ModuloId agregada a matricula';
END
GO

-- 8. Migrar datos de matricula: asignar al módulo 1 del año lectivo correspondiente
UPDATE m
SET m.ModuloId = mod.ModuloId
FROM matricula m
INNER JOIN modulo mod ON mod.AnioLectivoId = m.AnioLectivoId AND mod.Numero = 1
WHERE m.ModuloId IS NULL;
PRINT 'Matrículas migradas al módulo 1';
GO

-- 9. Hacer ModuloId NOT NULL y agregar FK en matricula
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('matricula') AND name = 'ModuloId')
BEGIN
    -- Primero verificar que no haya nulls
    IF NOT EXISTS (SELECT 1 FROM matricula WHERE ModuloId IS NULL)
    BEGIN
        ALTER TABLE matricula ALTER COLUMN ModuloId INT NOT NULL;

        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_matricula_modulo')
        BEGIN
            ALTER TABLE matricula ADD CONSTRAINT FK_matricula_modulo
                FOREIGN KEY (ModuloId) REFERENCES modulo(ModuloId);
        END
        PRINT 'FK de matricula a modulo creada';
    END
END
GO

-- 10. Eliminar FK y columna AnioLectivoId de matricula (opcional - si deseas limpiar)
-- NOTA: Descomentar solo después de verificar que la migración fue exitosa
/*
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_matricula_aniolectivo')
BEGIN
    ALTER TABLE matricula DROP CONSTRAINT FK_matricula_aniolectivo;
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('matricula') AND name = 'AnioLectivoId')
BEGIN
    ALTER TABLE matricula DROP COLUMN AnioLectivoId;
    PRINT 'Columna AnioLectivoId eliminada de matricula';
END
*/
GO

PRINT '--- Script completado ---';
PRINT 'IMPORTANTE: Los datos existentes fueron migrados al Módulo 1 de cada año.';
PRINT 'Puedes reasignar materias y matrículas a otros módulos según sea necesario.';
GO
