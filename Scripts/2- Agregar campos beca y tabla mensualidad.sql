-- =============================================
-- Script para agregar campos de beca y modificar tablas de precios
-- SQL Server
-- =============================================

-- =============================================
-- MODIFICACIÓN TABLA ESTUDIANTE
-- =============================================

-- Agregar campos de beca a la tabla estudiante
ALTER TABLE estudiante ADD EsBecado BIT NOT NULL DEFAULT 0;
ALTER TABLE estudiante ADD PorcentajeBeca DECIMAL(5,2) NOT NULL DEFAULT 0;

-- =============================================
-- MODIFICACIÓN TABLA PRECIO_MATRICULA
-- Quitar dependencia de año lectivo
-- =============================================

-- Si la tabla ya existe con datos, primero eliminarla y recrearla
-- O si prefieres mantener datos, usa estos comandos:

-- Opción 1: Eliminar y recrear (si no tienes datos importantes)
DROP TABLE IF EXISTS precio_matricula;

CREATE TABLE precio_matricula (
    PrecioMatriculaId INT IDENTITY(1,1) PRIMARY KEY,
    CategoriaEstudianteId INT NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_PrecioMatricula_Categoria FOREIGN KEY (CategoriaEstudianteId)
        REFERENCES categoria_estudiante(CategoriaEstudianteId),
    CONSTRAINT UQ_PrecioMatricula_Categoria UNIQUE (CategoriaEstudianteId)
);

-- =============================================
-- NUEVA TABLA: precio_mensualidad
-- =============================================

CREATE TABLE precio_mensualidad (
    PrecioMensualidadId INT IDENTITY(1,1) PRIMARY KEY,
    CategoriaEstudianteId INT NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_PrecioMensualidad_Categoria FOREIGN KEY (CategoriaEstudianteId)
        REFERENCES categoria_estudiante(CategoriaEstudianteId),
    CONSTRAINT UQ_PrecioMensualidad_Categoria UNIQUE (CategoriaEstudianteId)
);

-- =============================================
-- DATOS INICIALES
-- =============================================

-- Precios de matrícula por categoría
-- CategoriaEstudianteId 1 = Interno, 2 = Externo
INSERT INTO precio_matricula (CategoriaEstudianteId, Precio) VALUES (1, 15.00);  -- Interno
INSERT INTO precio_matricula (CategoriaEstudianteId, Precio) VALUES (2, 15.00);  -- Externo

-- Precios de mensualidad por categoría
INSERT INTO precio_mensualidad (CategoriaEstudianteId, Precio) VALUES (1, 10.00);  -- Interno
INSERT INTO precio_mensualidad (CategoriaEstudianteId, Precio) VALUES (2, 15.00);  -- Externo
