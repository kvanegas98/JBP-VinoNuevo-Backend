-- =============================================
-- Script: Migración de tabla nota
-- Descripción: Agrega campos para sistema de evaluación flexible
--              manteniendo compatibilidad con datos históricos
-- =============================================

USE [sistemaDB]
GO

PRINT '====================================================';
PRINT 'Iniciando migración de tabla nota...';
PRINT '====================================================';
PRINT '';

-- =============================================
-- 1. AGREGAR CAMPO: MatriculaCursoId
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'MatriculaCursoId')
BEGIN
    ALTER TABLE dbo.nota
    ADD MatriculaCursoId INT NULL;

    PRINT '✓ Campo MatriculaCursoId agregado (para cursos especializados).';
END
ELSE
BEGIN
    PRINT '- Campo MatriculaCursoId ya existe.';
END
GO

-- =============================================
-- 2. AGREGAR CAMPO: ComponenteEvaluacionId
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'ComponenteEvaluacionId')
BEGIN
    ALTER TABLE dbo.nota
    ADD ComponenteEvaluacionId INT NULL;

    PRINT '✓ Campo ComponenteEvaluacionId agregado (para componentes configurables).';
END
ELSE
BEGIN
    PRINT '- Campo ComponenteEvaluacionId ya existe.';
END
GO

-- =============================================
-- 3. AGREGAR CAMPO: Nota (individual por componente)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota')
BEGIN
    ALTER TABLE dbo.nota
    ADD Nota DECIMAL(5,2) NULL;

    PRINT '✓ Campo Nota agregado (para nota individual por componente).';
END
ELSE
BEGIN
    PRINT '- Campo Nota ya existe.';
END
GO

-- =============================================
-- 4. AGREGAR CAMPO: UsuarioRegistroId
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'UsuarioRegistroId')
BEGIN
    ALTER TABLE dbo.nota
    ADD UsuarioRegistroId INT NULL;

    PRINT '✓ Campo UsuarioRegistroId agregado.';
END
ELSE
BEGIN
    PRINT '- Campo UsuarioRegistroId ya existe.';
END
GO

-- =============================================
-- 5. HACER CAMPOS LEGACY NULLABLES
-- =============================================
-- Hacer Nota1 nullable (solo se usa en sistema legacy)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota1' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.nota
    ALTER COLUMN Nota1 DECIMAL(18,2) NULL;

    PRINT '✓ Campo Nota1 ahora es nullable (legacy).';
END
ELSE
BEGIN
    PRINT '- Campo Nota1 ya es nullable.';
END
GO

-- Hacer Nota2 nullable (solo se usa en sistema legacy)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Nota2' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.nota
    ALTER COLUMN Nota2 DECIMAL(18,2) NULL;

    PRINT '✓ Campo Nota2 ahora es nullable (legacy).';
END
ELSE
BEGIN
    PRINT '- Campo Nota2 ya es nullable.';
END
GO

-- Hacer Promedio nullable (se calculará dinámicamente)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'Promedio' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.nota
    ALTER COLUMN Promedio DECIMAL(18,2) NULL;

    PRINT '✓ Campo Promedio ahora es nullable.';
END
ELSE
BEGIN
    PRINT '- Campo Promedio ya es nullable.';
END
GO

-- Hacer MateriaId nullable (no se usa en cursos especializados)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'MateriaId' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.nota
    ALTER COLUMN MateriaId INT NULL;

    PRINT '✓ Campo MateriaId ahora es nullable.';
END
ELSE
BEGIN
    PRINT '- Campo MateriaId ya es nullable.';
END
GO

-- Hacer MatriculaId nullable (no se usa en cursos especializados)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'MatriculaId' AND is_nullable = 0)
BEGIN
    ALTER TABLE dbo.nota
    ALTER COLUMN MatriculaId INT NULL;

    PRINT '✓ Campo MatriculaId ahora es nullable.';
END
ELSE
BEGIN
    PRINT '- Campo MatriculaId ya es nullable.';
END
GO

-- =============================================
-- 6. AGREGAR FOREIGN KEYS
-- =============================================

-- FK para MatriculaCurso
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Nota_MatriculaCurso')
BEGIN
    ALTER TABLE dbo.nota
    ADD CONSTRAINT FK_Nota_MatriculaCurso
        FOREIGN KEY (MatriculaCursoId)
        REFERENCES dbo.MatriculasCurso(MatriculaCursoId)
        ON DELETE CASCADE;

    PRINT '✓ Foreign Key FK_Nota_MatriculaCurso agregada.';
END
ELSE
BEGIN
    PRINT '- Foreign Key FK_Nota_MatriculaCurso ya existe.';
END
GO

-- FK para ComponenteEvaluacion
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Nota_ComponenteEvaluacion')
BEGIN
    ALTER TABLE dbo.nota
    ADD CONSTRAINT FK_Nota_ComponenteEvaluacion
        FOREIGN KEY (ComponenteEvaluacionId)
        REFERENCES dbo.ComponenteEvaluacion(ComponenteEvaluacionId);

    PRINT '✓ Foreign Key FK_Nota_ComponenteEvaluacion agregada.';
END
ELSE
BEGIN
    PRINT '- Foreign Key FK_Nota_ComponenteEvaluacion ya existe.';
END
GO

-- FK para Usuario
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Nota_Usuario')
BEGIN
    ALTER TABLE dbo.nota
    ADD CONSTRAINT FK_Nota_Usuario
        FOREIGN KEY (UsuarioRegistroId)
        REFERENCES dbo.usuario(UsuarioId);

    PRINT '✓ Foreign Key FK_Nota_Usuario agregada.';
END
ELSE
BEGIN
    PRINT '- Foreign Key FK_Nota_Usuario ya existe.';
END
GO

-- =============================================
-- 7. AGREGAR CONSTRAINTS DE VALIDACIÓN
-- =============================================

-- Constraint: Una matrícula debe ser académica O especializada (no ambas)
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Nota_TipoMatricula')
BEGIN
    ALTER TABLE dbo.nota
    ADD CONSTRAINT CK_Nota_TipoMatricula
        CHECK (
            (MatriculaId IS NOT NULL AND MatriculaCursoId IS NULL)
            OR
            (MatriculaId IS NULL AND MatriculaCursoId IS NOT NULL)
        );

    PRINT '✓ Constraint CK_Nota_TipoMatricula agregado (solo una referencia).';
END
ELSE
BEGIN
    PRINT '- Constraint CK_Nota_TipoMatricula ya existe.';
END
GO

-- Constraint: Rango de nota individual (0-100)
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Nota_RangoNota')
BEGIN
    ALTER TABLE dbo.nota
    ADD CONSTRAINT CK_Nota_RangoNota
        CHECK (Nota IS NULL OR (Nota >= 0 AND Nota <= 100));

    PRINT '✓ Constraint CK_Nota_RangoNota agregado (0-100).';
END
ELSE
BEGIN
    PRINT '- Constraint CK_Nota_RangoNota ya existe.';
END
GO

-- =============================================
-- 8. CREAR ÍNDICES ÚNICOS
-- =============================================

-- Índice único: No duplicar componentes en matrícula académica
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Nota_Matricula_Componente')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Nota_Matricula_Componente
    ON dbo.nota(MatriculaId, ComponenteEvaluacionId)
    WHERE MatriculaId IS NOT NULL AND ComponenteEvaluacionId IS NOT NULL;

    PRINT '✓ Índice único UQ_Nota_Matricula_Componente creado.';
END
ELSE
BEGIN
    PRINT '- Índice único UQ_Nota_Matricula_Componente ya existe.';
END
GO

-- Índice único: No duplicar componentes en matrícula de curso
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Nota_MatriculaCurso_Componente')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_Nota_MatriculaCurso_Componente
    ON dbo.nota(MatriculaCursoId, ComponenteEvaluacionId)
    WHERE MatriculaCursoId IS NOT NULL AND ComponenteEvaluacionId IS NOT NULL;

    PRINT '✓ Índice único UQ_Nota_MatriculaCurso_Componente creado.';
END
ELSE
BEGIN
    PRINT '- Índice único UQ_Nota_MatriculaCurso_Componente ya existe.';
END
GO

-- =============================================
-- 9. CREAR ÍNDICES DE RENDIMIENTO
-- =============================================

-- Índice para ComponenteEvaluacionId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Nota_ComponenteEvaluacionId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Nota_ComponenteEvaluacionId
    ON dbo.nota(ComponenteEvaluacionId);

    PRINT '✓ Índice de rendimiento IX_Nota_ComponenteEvaluacionId creado.';
END
ELSE
BEGIN
    PRINT '- Índice IX_Nota_ComponenteEvaluacionId ya existe.';
END
GO

-- Índice para MatriculaCursoId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Nota_MatriculaCursoId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Nota_MatriculaCursoId
    ON dbo.nota(MatriculaCursoId)
    INCLUDE (ComponenteEvaluacionId, Nota);

    PRINT '✓ Índice de rendimiento IX_Nota_MatriculaCursoId creado.';
END
ELSE
BEGIN
    PRINT '- Índice IX_Nota_MatriculaCursoId ya existe.';
END
GO

-- =============================================
-- 10. AGREGAR COLUMNA COMPUTED: EsNotaNuevoSistema
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.nota') AND name = 'EsNotaNuevoSistema')
BEGIN
    ALTER TABLE dbo.nota
    ADD EsNotaNuevoSistema AS (CASE WHEN ComponenteEvaluacionId IS NOT NULL THEN 1 ELSE 0 END) PERSISTED;

    PRINT '✓ Columna computed EsNotaNuevoSistema agregada (distingue sistema legacy vs nuevo).';
END
ELSE
BEGIN
    PRINT '- Columna EsNotaNuevoSistema ya existe.';
END
GO

-- =============================================
-- RESUMEN DE ESTRUCTURA FINAL
-- =============================================
PRINT '';
PRINT '====================================================';
PRINT 'Estructura final de la tabla nota:';
PRINT '====================================================';
PRINT '';
PRINT 'CAMPOS LEGACY (solo para datos históricos):';
PRINT '  - Nota1, Nota2, Promedio (nullable)';
PRINT '  - MateriaId (nullable)';
PRINT '';
PRINT 'CAMPOS NUEVOS (sistema flexible):';
PRINT '  - ComponenteEvaluacionId (FK a ComponenteEvaluacion)';
PRINT '  - Nota (0-100, individual por componente)';
PRINT '  - MatriculaCursoId (FK a MatriculasCurso)';
PRINT '  - UsuarioRegistroId (FK a usuario)';
PRINT '';
PRINT 'CAMPOS COMPARTIDOS:';
PRINT '  - NotaId, MatriculaId, FechaRegistro, Observaciones';
PRINT '';
PRINT 'DISTINGUIR SISTEMAS:';
PRINT '  - Si ComponenteEvaluacionId IS NOT NULL → Sistema nuevo';
PRINT '  - Si ComponenteEvaluacionId IS NULL → Sistema legacy';
PRINT '';
PRINT '====================================================';
PRINT 'Migración completada exitosamente.';
PRINT '====================================================';
