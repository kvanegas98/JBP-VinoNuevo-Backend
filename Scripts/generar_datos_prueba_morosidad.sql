-- =============================================
-- Script: Generar datos de prueba para morosidad
-- Descripción: Crea estudiantes, matrículas y pagos
--              para probar diferentes escenarios de morosidad
-- Fecha: 2026-01-24
-- =============================================

SET NOCOUNT ON
GO

PRINT '========================================'
PRINT 'GENERANDO DATOS DE PRUEBA - MOROSIDAD'
PRINT '========================================'
PRINT ''

-- Variables para IDs
DECLARE @RedId INT
DECLARE @ModuloId INT
DECLARE @CursoEspecializadoId INT
DECLARE @ModalidadPresencialId INT
DECLARE @CategoriaInternoId INT
DECLARE @CategoriaExternoId INT
DECLARE @CargoMatrimonioId INT
DECLARE @CargoDiscipuloId INT

-- Obtener IDs de catálogos existentes
SELECT TOP 1 @RedId = RedId FROM red WHERE Activo = 1
SELECT TOP 1 @ModuloId = ModuloId FROM modulo WHERE Activo = 1
SELECT TOP 1 @CursoEspecializadoId = CursoEspecializadoId FROM CursosEspecializados WHERE Activo = 1
SELECT TOP 1 @ModalidadPresencialId = ModalidadId FROM modalidad WHERE Nombre LIKE '%Presencial%'
SELECT TOP 1 @CategoriaInternoId = CategoriaEstudianteId FROM categoria_estudiante WHERE Nombre LIKE '%Interno%'
SELECT TOP 1 @CategoriaExternoId = CategoriaEstudianteId FROM categoria_estudiante WHERE Nombre LIKE '%Externo%'
SELECT TOP 1 @CargoMatrimonioId = CargoId FROM cargo WHERE Nombre LIKE '%Matrimonio%'
SELECT TOP 1 @CargoDiscipuloId = CargoId FROM cargo WHERE Nombre LIKE '%Discipulo%' OR Nombre LIKE '%Discípulo%'

PRINT 'Catálogos obtenidos:'
PRINT '  Red ID: ' + CAST(ISNULL(@RedId, 0) AS VARCHAR)
PRINT '  Módulo ID: ' + CAST(ISNULL(@ModuloId, 0) AS VARCHAR)
PRINT '  Curso Especializado ID: ' + CAST(ISNULL(@CursoEspecializadoId, 0) AS VARCHAR)
PRINT '  Modalidad Presencial ID: ' + CAST(ISNULL(@ModalidadPresencialId, 0) AS VARCHAR)
PRINT '  Categoría Interno ID: ' + CAST(ISNULL(@CategoriaInternoId, 0) AS VARCHAR)
PRINT '  Categoría Externo ID: ' + CAST(ISNULL(@CategoriaExternoId, 0) AS VARCHAR)
PRINT '  Cargo Matrimonio ID: ' + CAST(ISNULL(@CargoMatrimonioId, 0) AS VARCHAR)
PRINT '  Cargo Discipulo ID: ' + CAST(ISNULL(@CargoDiscipuloId, 0) AS VARCHAR)
PRINT ''

-- Validar que existan los catálogos necesarios
IF @RedId IS NULL OR @ModuloId IS NULL OR @CursoEspecializadoId IS NULL OR
   @ModalidadPresencialId IS NULL OR @CategoriaInternoId IS NULL OR
   @CargoMatrimonioId IS NULL OR @CargoDiscipuloId IS NULL
BEGIN
    PRINT ''
    PRINT '❌ ERROR: Faltan catálogos necesarios'
    PRINT 'Por favor, asegúrate de tener:'
    PRINT '  - Al menos 1 Red activa'
    PRINT '  - Al menos 1 Módulo activo'
    PRINT '  - Al menos 1 Curso Especializado activo'
    PRINT '  - Una Modalidad tipo "Presencial"'
    PRINT '  - Una Categoría de estudiante tipo "Interno"'
    PRINT '  - Cargos: Matrimonio y Discipulo'
    PRINT ''
    PRINT 'Script detenido'
    RETURN
END

-- =============================================
-- ESCENARIO 1: Estudiante con MORA GRAVE (académico regular)
-- Matriculado hace 5 meses, solo ha pagado 1 mensualidad
-- =============================================
PRINT '1. Creando estudiante con MORA GRAVE (académico)...'

DECLARE @EstudianteId1 INT

INSERT INTO estudiante (
    Codigo, NombreCompleto, Cedula, CorreoElectronico, Celular, Ciudad,
    TipoEstudiante, EsInterno, EsBecado, PorcentajeBeca, RedId, Activo
)
VALUES (
    '2026-IVN-0001', 'Juan Pérez Mora Grave', '001-010101-0001N',
    'juan.perez@test.com', '8888-1111', 'Managua',
    'Regular', 1, 0, 0, @RedId, 1
)

SET @EstudianteId1 = SCOPE_IDENTITY()

-- Asignar cargo
INSERT INTO estudiante_cargo (EstudianteId, CargoId)
VALUES (@EstudianteId1, @CargoMatrimonioId)

-- Crear matrícula (hace 5 meses - noviembre 2025)
DECLARE @MatriculaId1 INT
DECLARE @FechaMatricula1 DATE = DATEADD(MONTH, -5, GETDATE())

INSERT INTO matricula (
    Codigo, EstudianteId, ModuloId, ModalidadId, CategoriaEstudianteId,
    FechaMatricula, MontoMatricula, DescuentoAplicado, MontoFinal, Estado, Observaciones
)
VALUES (
    'MAT-2025-0001', @EstudianteId1, @ModuloId, @ModalidadPresencialId, @CategoriaInternoId,
    @FechaMatricula1, 50.00, 0, 50.00, 'Activa', NULL
)

SET @MatriculaId1 = SCOPE_IDENTITY()

-- Pago de matrícula (hace 5 meses)
INSERT INTO pago (
    Codigo, MatriculaId, MateriaId, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PAG-2025-0001', @MatriculaId1, NULL, 'Matricula', 50.00, 0, 50.00,
    @FechaMatricula1, 0, 50.00, 0, 0,
    36.5, 50.00, 0, 0, 0,
    'Efectivo', NULL, 'Pago de matrícula', 'Completado'
)

-- Pagar solo 1 de las 4 mensualidades que deberían estar pagadas
DECLARE @MateriaId1 INT
SELECT TOP 1 @MateriaId1 = MateriaId FROM materia WHERE ModuloId = @ModuloId AND Activo = 1 ORDER BY Orden

INSERT INTO pago (
    Codigo, MatriculaId, MateriaId, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PAG-2025-0002', @MatriculaId1, @MateriaId1, 'Mensualidad', 25.00, 0, 25.00,
    DATEADD(MONTH, -4, GETDATE()), 0, 25.00, 0, 0,
    36.5, 25.00, 0, 0, 0,
    'Efectivo', NULL, 'Mensualidad 1', 'Completado'
)

PRINT '  ✓ Estudiante con mora grave creado (debe 3 materias)'
PRINT ''

-- =============================================
-- ESCENARIO 2: Estudiante con MORA MODERADA (curso especializado)
-- Matriculado hace 4 meses, ha pagado 1 mensualidad
-- =============================================
PRINT '2. Creando estudiante con MORA MODERADA (curso especializado)...'

DECLARE @EstudianteId2 INT

INSERT INTO estudiante (
    Codigo, NombreCompleto, Cedula, CorreoElectronico, Celular, Ciudad,
    TipoEstudiante, EsInterno, EsBecado, PorcentajeBeca, RedId, Activo
)
VALUES (
    '2026-IVN-0002', 'María González Mora Moderada', '001-020202-0002P',
    'maria.gonzalez@test.com', '8888-2222', 'Managua',
    'Regular', 1, 0, 0, @RedId, 1
)

SET @EstudianteId2 = SCOPE_IDENTITY()

-- Asignar cargo
INSERT INTO estudiante_cargo (EstudianteId, CargoId)
VALUES (@EstudianteId2, @CargoDiscipuloId)

-- Crear matrícula de curso (hace 4 meses - diciembre 2025)
DECLARE @MatriculaCursoId2 INT
DECLARE @FechaMatriculaCurso2 DATE = DATEADD(MONTH, -4, GETDATE())

INSERT INTO MatriculasCurso (
    Codigo, EstudianteId, CursoEspecializadoId, ModalidadId, CategoriaEstudianteId,
    FechaMatricula, MontoMatricula, DescuentoAplicado, MontoFinal, Estado, Aprobado, Observaciones
)
VALUES (
    'MCURSO-2025-0001', @EstudianteId2, @CursoEspecializadoId, @ModalidadPresencialId, @CategoriaInternoId,
    @FechaMatriculaCurso2, 15.00, 0, 15.00, 'Activa', 0, NULL
)

SET @MatriculaCursoId2 = SCOPE_IDENTITY()

-- Pago de matrícula curso (hace 4 meses)
INSERT INTO PagosCurso (
    Codigo, MatriculaCursoId, NumeroMensualidad, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PCURSO-2025-0001', @MatriculaCursoId2, NULL, 'Matricula', 15.00, 0, 15.00,
    @FechaMatriculaCurso2, 0, 15.00, 0, 0,
    36.5, 15.00, 0, 0, 0,
    'Efectivo', NULL, 'Pago de matrícula curso', 'Completado'
)

-- Pagar solo 1 de las 3 mensualidades que deberían estar pagadas
INSERT INTO PagosCurso (
    Codigo, MatriculaCursoId, NumeroMensualidad, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PCURSO-2025-0002', @MatriculaCursoId2, 1, 'Mensualidad', 12.00, 0, 12.00,
    DATEADD(MONTH, -3, GETDATE()), 0, 12.00, 0, 0,
    36.5, 12.00, 0, 0, 0,
    'Efectivo', NULL, 'Mensualidad 1', 'Completado'
)

PRINT '  ✓ Estudiante con mora moderada creado (debe 2 mensualidades curso)'
PRINT ''

-- =============================================
-- ESCENARIO 3: Estudiante con MORA LEVE (académico)
-- Matriculado hace 3 meses, ha pagado 1 mensualidad
-- =============================================
PRINT '3. Creando estudiante con MORA LEVE (académico)...'

DECLARE @EstudianteId3 INT

INSERT INTO estudiante (
    Codigo, NombreCompleto, Cedula, CorreoElectronico, Celular, Ciudad,
    TipoEstudiante, EsInterno, EsBecado, PorcentajeBeca, RedId, Activo
)
VALUES (
    '2026-IVN-0003', 'Carlos Martínez Mora Leve', '001-030303-0003M',
    'carlos.martinez@test.com', '8888-3333', 'Managua',
    'Regular', 1, 0, 0, @RedId, 1
)

SET @EstudianteId3 = SCOPE_IDENTITY()

-- Asignar cargo
INSERT INTO estudiante_cargo (EstudianteId, CargoId)
VALUES (@EstudianteId3, @CargoMatrimonioId)

-- Crear matrícula (hace 3 meses - enero 2026)
DECLARE @MatriculaId3 INT
DECLARE @FechaMatricula3 DATE = DATEADD(MONTH, -3, GETDATE())

INSERT INTO matricula (
    Codigo, EstudianteId, ModuloId, ModalidadId, CategoriaEstudianteId,
    FechaMatricula, MontoMatricula, DescuentoAplicado, MontoFinal, Estado, Observaciones
)
VALUES (
    'MAT-2026-0001', @EstudianteId3, @ModuloId, @ModalidadPresencialId, @CategoriaInternoId,
    @FechaMatricula3, 50.00, 0, 50.00, 'Activa', NULL
)

SET @MatriculaId3 = SCOPE_IDENTITY()

-- Pago de matrícula (hace 3 meses)
INSERT INTO pago (
    Codigo, MatriculaId, MateriaId, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PAG-2026-0001', @MatriculaId3, NULL, 'Matricula', 50.00, 0, 50.00,
    @FechaMatricula3, 0, 50.00, 0, 0,
    36.5, 50.00, 0, 0, 0,
    'Efectivo', NULL, 'Pago de matrícula', 'Completado'
)

-- Pagar 1 de las 2 mensualidades que deberían estar pagadas
INSERT INTO pago (
    Codigo, MatriculaId, MateriaId, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PAG-2026-0002', @MatriculaId3, @MateriaId1, 'Mensualidad', 25.00, 0, 25.00,
    DATEADD(MONTH, -2, GETDATE()), 0, 25.00, 0, 0,
    36.5, 25.00, 0, 0, 0,
    'Efectivo', NULL, 'Mensualidad 1', 'Completado'
)

PRINT '  ✓ Estudiante con mora leve creado (debe 1 materia)'
PRINT ''

-- =============================================
-- ESCENARIO 4: Estudiante AL DÍA (curso especializado)
-- Matriculado hace 3 meses, ha pagado todas las mensualidades
-- =============================================
PRINT '4. Creando estudiante AL DÍA (curso especializado)...'

DECLARE @EstudianteId4 INT

INSERT INTO estudiante (
    Codigo, NombreCompleto, Cedula, CorreoElectronico, Celular, Ciudad,
    TipoEstudiante, EsInterno, EsBecado, PorcentajeBeca, RedId, Activo
)
VALUES (
    '2026-IVN-0004', 'Ana López Sin Mora', '001-040404-0004F',
    'ana.lopez@test.com', '8888-4444', 'Managua',
    'Regular', 1, 0, 0, @RedId, 1
)

SET @EstudianteId4 = SCOPE_IDENTITY()

-- Asignar cargo
INSERT INTO estudiante_cargo (EstudianteId, CargoId)
VALUES (@EstudianteId4, @CargoDiscipuloId)

-- Crear matrícula de curso (hace 3 meses)
DECLARE @MatriculaCursoId4 INT
DECLARE @FechaMatriculaCurso4 DATE = DATEADD(MONTH, -3, GETDATE())

INSERT INTO MatriculasCurso (
    Codigo, EstudianteId, CursoEspecializadoId, ModalidadId, CategoriaEstudianteId,
    FechaMatricula, MontoMatricula, DescuentoAplicado, MontoFinal, Estado, Aprobado, Observaciones
)
VALUES (
    'MCURSO-2026-0001', @EstudianteId4, @CursoEspecializadoId, @ModalidadPresencialId, @CategoriaInternoId,
    @FechaMatriculaCurso4, 15.00, 0, 15.00, 'Activa', 0, NULL
)

SET @MatriculaCursoId4 = SCOPE_IDENTITY()

-- Pago de matrícula curso
INSERT INTO PagosCurso (
    Codigo, MatriculaCursoId, NumeroMensualidad, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PCURSO-2026-0001', @MatriculaCursoId4, NULL, 'Matricula', 15.00, 0, 15.00,
    @FechaMatriculaCurso4, 0, 15.00, 0, 0,
    36.5, 15.00, 0, 0, 0,
    'Efectivo', NULL, 'Pago de matrícula curso', 'Completado'
)

-- Pagar las 2 mensualidades que deberían estar pagadas
INSERT INTO PagosCurso (
    Codigo, MatriculaCursoId, NumeroMensualidad, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES
(
    'PCURSO-2026-0002', @MatriculaCursoId4, 1, 'Mensualidad', 12.00, 0, 12.00,
    DATEADD(MONTH, -2, GETDATE()), 0, 12.00, 0, 0,
    36.5, 12.00, 0, 0, 0,
    'Efectivo', NULL, 'Mensualidad 1', 'Completado'
),
(
    'PCURSO-2026-0003', @MatriculaCursoId4, 2, 'Mensualidad', 12.00, 0, 12.00,
    DATEADD(MONTH, -1, GETDATE()), 0, 12.00, 0, 0,
    36.5, 12.00, 0, 0, 0,
    'Efectivo', NULL, 'Mensualidad 2', 'Completado'
)

PRINT '  ✓ Estudiante al día creado (sin mora)'
PRINT ''

-- =============================================
-- ESCENARIO 5: Estudiante BECADO 100% (no debe aparecer en morosidad)
-- =============================================
PRINT '5. Creando estudiante BECADO 100%...'

DECLARE @EstudianteId5 INT

INSERT INTO estudiante (
    Codigo, NombreCompleto, Cedula, CorreoElectronico, Celular, Ciudad,
    TipoEstudiante, EsInterno, EsBecado, PorcentajeBeca, RedId, Activo
)
VALUES (
    '2026-IVN-0005', 'Pedro Ramírez Becado 100%', '001-050505-0005N',
    'pedro.ramirez@test.com', '8888-5555', 'Managua',
    'Regular', 1, 1, 100, @RedId, 1
)

SET @EstudianteId5 = SCOPE_IDENTITY()

-- Asignar cargo
INSERT INTO estudiante_cargo (EstudianteId, CargoId)
VALUES (@EstudianteId5, @CargoMatrimonioId)

-- Crear matrícula (automáticamente activa por becado 100%)
INSERT INTO matricula (
    Codigo, EstudianteId, ModuloId, ModalidadId, CategoriaEstudianteId,
    FechaMatricula, MontoMatricula, DescuentoAplicado, MontoFinal, Estado, Observaciones
)
VALUES (
    'MAT-2026-0002', @EstudianteId5, @ModuloId, @ModalidadPresencialId, @CategoriaInternoId,
    DATEADD(MONTH, -2, GETDATE()), 50.00, 50.00, 0, 'Activa', 'Becado 100%'
)

PRINT '  ✓ Estudiante becado 100% creado (no aparece en morosidad)'
PRINT ''

-- =============================================
-- ESCENARIO 6: Estudiante con AMBOS tipos (académico + curso)
-- Con mora en ambos sistemas
-- =============================================
PRINT '6. Creando estudiante con AMBOS sistemas (mora mixta)...'

DECLARE @EstudianteId6 INT

INSERT INTO estudiante (
    Codigo, NombreCompleto, Cedula, CorreoElectronico, Celular, Ciudad,
    TipoEstudiante, EsInterno, EsBecado, PorcentajeBeca, RedId, Activo
)
VALUES (
    '2026-IVN-0006', 'Laura Torres Mora Mixta', '001-060606-0006P',
    'laura.torres@test.com', '8888-6666', 'Managua',
    'Regular', 1, 0, 0, @RedId, 1
)

SET @EstudianteId6 = SCOPE_IDENTITY()

-- Asignar cargo
INSERT INTO estudiante_cargo (EstudianteId, CargoId)
VALUES (@EstudianteId6, @CargoDiscipuloId)

-- Matrícula académica (hace 4 meses)
DECLARE @MatriculaId6 INT
DECLARE @FechaMatricula6 DATE = DATEADD(MONTH, -4, GETDATE())

INSERT INTO matricula (
    Codigo, EstudianteId, ModuloId, ModalidadId, CategoriaEstudianteId,
    FechaMatricula, MontoMatricula, DescuentoAplicado, MontoFinal, Estado, Observaciones
)
VALUES (
    'MAT-2026-0003', @EstudianteId6, @ModuloId, @ModalidadPresencialId, @CategoriaInternoId,
    @FechaMatricula6, 50.00, 0, 50.00, 'Activa', NULL
)

SET @MatriculaId6 = SCOPE_IDENTITY()

-- Pago matrícula académica
INSERT INTO pago (
    Codigo, MatriculaId, MateriaId, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PAG-2026-0003', @MatriculaId6, NULL, 'Matricula', 50.00, 0, 50.00,
    @FechaMatricula6, 0, 50.00, 0, 0,
    36.5, 50.00, 0, 0, 0,
    'Efectivo', NULL, 'Pago de matrícula', 'Completado'
)

-- Pagar solo 1 mensualidad académica (debe 2)
INSERT INTO pago (
    Codigo, MatriculaId, MateriaId, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PAG-2026-0004', @MatriculaId6, @MateriaId1, 'Mensualidad', 25.00, 0, 25.00,
    DATEADD(MONTH, -3, GETDATE()), 0, 25.00, 0, 0,
    36.5, 25.00, 0, 0, 0,
    'Efectivo', NULL, 'Mensualidad 1', 'Completado'
)

-- Matrícula curso especializado (hace 3 meses)
DECLARE @MatriculaCursoId6 INT
DECLARE @FechaMatriculaCurso6 DATE = DATEADD(MONTH, -3, GETDATE())

INSERT INTO MatriculasCurso (
    Codigo, EstudianteId, CursoEspecializadoId, ModalidadId, CategoriaEstudianteId,
    FechaMatricula, MontoMatricula, DescuentoAplicado, MontoFinal, Estado, Aprobado, Observaciones
)
VALUES (
    'MCURSO-2026-0002', @EstudianteId6, @CursoEspecializadoId, @ModalidadPresencialId, @CategoriaInternoId,
    @FechaMatriculaCurso6, 15.00, 0, 15.00, 'Activa', 0, NULL
)

SET @MatriculaCursoId6 = SCOPE_IDENTITY()

-- Pago matrícula curso
INSERT INTO PagosCurso (
    Codigo, MatriculaCursoId, NumeroMensualidad, TipoPago, Monto, Descuento, MontoFinal,
    FechaPago, EfectivoCordobas, EfectivoDolares, TarjetaCordobas, TarjetaDolares,
    TipoCambio, TotalPagadoUSD, Vuelto, VueltoCordobas, VueltoDolares,
    MetodoPago, NumeroComprobante, Observaciones, Estado
)
VALUES (
    'PCURSO-2026-0004', @MatriculaCursoId6, NULL, 'Matricula', 15.00, 0, 15.00,
    @FechaMatriculaCurso6, 0, 15.00, 0, 0,
    36.5, 15.00, 0, 0, 0,
    'Efectivo', NULL, 'Pago de matrícula curso', 'Completado'
)

-- NO pagar ninguna mensualidad de curso (debe 2)

PRINT '  ✓ Estudiante con mora mixta creado (debe en ambos sistemas)'
PRINT ''

-- Resumen final
PRINT '========================================'
PRINT 'RESUMEN DE DATOS GENERADOS'
PRINT '========================================'
PRINT ''
PRINT 'Estudiantes creados: 6'
PRINT ''
PRINT 'ESCENARIOS DE MOROSIDAD:'
PRINT '  1. Juan Pérez - MORA GRAVE (académico): Debe 3 materias'
PRINT '  2. María González - MORA MODERADA (curso): Debe 2 mensualidades'
PRINT '  3. Carlos Martínez - MORA LEVE (académico): Debe 1 materia'
PRINT '  4. Ana López - AL DÍA (curso): Sin mora'
PRINT '  5. Pedro Ramírez - BECADO 100%: No aparece en morosidad'
PRINT '  6. Laura Torres - MORA MIXTA: Debe en ambos sistemas'
PRINT ''
PRINT 'Matrículas académicas: 4'
PRINT 'Matrículas cursos: 3'
PRINT 'Pagos académicos: 7'
PRINT 'Pagos cursos: 6'
PRINT ''
PRINT '✓ Datos de prueba generados exitosamente'
PRINT 'Puedes probar el reporte de morosidad en:'
PRINT '  GET /api/Reportes/Morosidad'
PRINT ''
PRINT 'Script completado'
GO
