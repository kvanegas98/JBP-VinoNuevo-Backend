using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Instituto;
using Sistema.Web.Models.Notas;

namespace Sistema.Web.Controllers
{
    /// <summary>
    /// Controller para el nuevo sistema de evaluación flexible
    /// Maneja componentes de evaluación configurables (Examen1, Examen2, Proyecto, etc.)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluacionesController : ControllerBase
    {
        private readonly DbContextSistema _context;
        private const decimal NOTA_MINIMA_APROBATORIA = 70;

        public EvaluacionesController(DbContextSistema context)
        {
            _context = context;
        }

        // ===================================================================
        // POST: api/Evaluaciones - Registrar nota individual
        // ===================================================================
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarNotaViewModel model)
        {
            try
            {
                // 1. Validar que se especifique MatriculaId O MatriculaCursoId (no ambos, no ninguno)
                if (!model.MatriculaId.HasValue && !model.MatriculaCursoId.HasValue)
                {
                    return BadRequest(new { message = "Debe especificar MatriculaId o MatriculaCursoId" });
                }

                if (model.MatriculaId.HasValue && model.MatriculaCursoId.HasValue)
                {
                    return BadRequest(new { message = "Solo puede especificar MatriculaId o MatriculaCursoId, no ambos" });
                }

                // 2. Obtener el tipo de evaluación del curso/materia
                int tipoEvaluacionId;
                string programaNombre;

                if (model.MatriculaId.HasValue)
                {
                    var matricula = await _context.Matriculas
                        .Include(m => m.Modulo)
                        .FirstOrDefaultAsync(m => m.MatriculaId == model.MatriculaId.Value);

                    if (matricula == null)
                        return NotFound(new { message = "Matrícula no encontrada" });

                    // Para el sistema flexible, usar tipo REGULAR (1) por defecto para matrículas académicas
                    tipoEvaluacionId = 1; // REGULAR
                    programaNombre = matricula.Modulo.Nombre;
                }
                else
                {
                    var matriculaCurso = await _context.MatriculasCurso
                        .Include(m => m.CursoEspecializado)
                            .ThenInclude(c => c.TipoEvaluacion)
                        .FirstOrDefaultAsync(m => m.MatriculaCursoId == model.MatriculaCursoId.Value);

                    if (matriculaCurso == null)
                        return NotFound(new { message = "Matrícula de curso no encontrada" });

                    tipoEvaluacionId = matriculaCurso.CursoEspecializado.TipoEvaluacionId;
                    programaNombre = matriculaCurso.CursoEspecializado.Nombre;
                }

                // 3. Validar que el componente pertenezca al tipo correcto
                var componente = await _context.ComponenteEvaluacion
                    .Include(c => c.TipoEvaluacion)
                    .FirstOrDefaultAsync(c => c.ComponenteEvaluacionId == model.ComponenteEvaluacionId);

                if (componente == null)
                    return NotFound(new { message = "Componente de evaluación no encontrado" });

                if (componente.TipoEvaluacionId != tipoEvaluacionId)
                {
                    return BadRequest(new
                    {
                        message = "El componente de evaluación no corresponde al tipo de curso",
                        componenteTipo = componente.TipoEvaluacion.Nombre,
                        programaTipo = await _context.TiposEvaluacion
                            .Where(t => t.TipoEvaluacionId == tipoEvaluacionId)
                            .Select(t => t.Nombre)
                            .FirstOrDefaultAsync()
                    });
                }

                // 4. Validar pagos o beca (solo para matrículas académicas)
                if (model.MatriculaId.HasValue)
                {
                    var matriculaConEstudiante = await _context.Matriculas
                        .Include(m => m.Estudiante)
                        .FirstOrDefaultAsync(m => m.MatriculaId == model.MatriculaId.Value);

                    // Si el estudiante NO es becado al 100%, verificar que tenga al menos un pago completado
                    if (!matriculaConEstudiante.Estudiante.EsBecado ||
                        matriculaConEstudiante.Estudiante.PorcentajeBeca < 100)
                    {
                        var tienePagos = await _context.Pagos
                            .AnyAsync(p => p.MatriculaId == model.MatriculaId.Value &&
                                           p.Estado == "Completado");

                        if (!tienePagos)
                        {
                            return BadRequest(new
                            {
                                message = "No se puede registrar la nota. El estudiante no ha realizado pagos para esta matrícula",
                                requierePago = true
                            });
                        }
                    }
                }

                // 5. Validar pagos para cursos especializados
                if (model.MatriculaCursoId.HasValue)
                {
                    var matriculaCursoConEstudiante = await _context.MatriculasCurso
                        .Include(m => m.Estudiante)
                        .Include(m => m.CursoEspecializado)
                        .FirstOrDefaultAsync(m => m.MatriculaCursoId == model.MatriculaCursoId.Value);

                    // Si el estudiante NO es becado al 100%, verificar que haya pagado
                    if (!matriculaCursoConEstudiante.Estudiante.EsBecado ||
                        matriculaCursoConEstudiante.Estudiante.PorcentajeBeca < 100)
                    {
                        var pagoCursoPendiente = await _context.PagosCurso
                            .AnyAsync(p => p.MatriculaCursoId == model.MatriculaCursoId.Value &&
                                           p.TipoPago == "Mensualidad" &&
                                           p.Estado == "Completado");

                        if (!pagoCursoPendiente)
                        {
                            return BadRequest(new
                            {
                                message = $"No se puede registrar la nota. El estudiante no ha pagado el curso '{matriculaCursoConEstudiante.CursoEspecializado.Nombre}'",
                                requierePago = true
                            });
                        }
                    }
                }

                // 6. Verificar que no exista ya una nota para este componente y materia
                var notaExistente = await _context.Notas
                    .FirstOrDefaultAsync(n =>
                        ((model.MatriculaId.HasValue && n.MatriculaId == model.MatriculaId.Value) ||
                         (model.MatriculaCursoId.HasValue && n.MatriculaCursoId == model.MatriculaCursoId.Value)) &&
                        n.ComponenteEvaluacionId == model.ComponenteEvaluacionId &&
                        ((!model.MateriaId.HasValue && !n.MateriaId.HasValue) ||
                         (model.MateriaId.HasValue && n.MateriaId == model.MateriaId.Value)));

                if (notaExistente != null)
                {
                    return BadRequest(new
                    {
                        message = "Ya existe una nota registrada para este componente",
                        notaId = notaExistente.NotaId,
                        notaActual = notaExistente.NotaValor,
                        componente = componente.Nombre
                    });
                }

                // 7. Obtener el usuario del token JWT
                int? usuarioId = null;
                if (User?.Claims != null)
                {
                    var usuarioIdClaim = User.Claims.FirstOrDefault(x => x.Type == "usuarioId");
                    if (usuarioIdClaim != null)
                    {
                        usuarioId = int.Parse(usuarioIdClaim.Value);
                    }
                }

                // 8. Crear y guardar la nota
                var nota = new Nota
                {
                    MatriculaId = model.MatriculaId,
                    MatriculaCursoId = model.MatriculaCursoId,
                    MateriaId = model.MateriaId,
                    ComponenteEvaluacionId = model.ComponenteEvaluacionId,
                    NotaValor = model.Nota,
                    Observaciones = model.Observaciones,
                    FechaRegistro = DateTime.Now,
                    UsuarioRegistroId = usuarioId
                };

                _context.Notas.Add(nota);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Nota registrada exitosamente",
                    notaId = nota.NotaId,
                    programa = programaNombre,
                    componente = componente.Nombre,
                    nota = nota.NotaValor
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al registrar la nota", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/Evaluaciones/Matricula/{matriculaId} - Obtener notas por matrícula académica
        // ===================================================================
        [HttpGet("Matricula/{matriculaId}")]
        public async Task<IActionResult> ObtenerPorMatricula(int matriculaId)
        {
            try
            {
                var notas = await _context.Notas
                    .Include(n => n.ComponenteEvaluacion)
                    .Include(n => n.Matricula).ThenInclude(m => m.Modulo)
                    .Include(n => n.Materia)
                    .Include(n => n.Usuario)
                    .Where(n => n.MatriculaId == matriculaId && n.ComponenteEvaluacionId.HasValue)
                    .OrderBy(n => n.ComponenteEvaluacion.Orden)
                    .Select(n => new
                    {
                        notaId = n.NotaId,
                        matriculaId = n.MatriculaId,
                        matriculaCursoId = n.MatriculaCursoId,
                        materiaId = n.MateriaId,
                        materiaNombre = n.Materia != null ? n.Materia.Nombre : null,
                        tipoMatricula = "Académica",
                        matriculaCodigo = n.Matricula.Codigo,
                        programaNombre = n.Matricula.Modulo.Nombre,
                        componenteEvaluacionId = n.ComponenteEvaluacionId.Value,
                        componenteNombre = n.ComponenteEvaluacion.Nombre,
                        componenteOrden = n.ComponenteEvaluacion.Orden,
                        componentePeso = n.ComponenteEvaluacion.PorcentajePeso,
                        nota = n.NotaValor.Value,
                        fechaRegistro = n.FechaRegistro,
                        observaciones = n.Observaciones,
                        usuarioRegistro = n.Usuario != null ? n.Usuario.Nombre : null
                    })
                    .ToListAsync();

                return Ok(notas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las notas", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/Evaluaciones/MatriculaCurso/{id} - Obtener notas por matrícula de curso
        // ===================================================================
        [HttpGet("MatriculaCurso/{matriculaCursoId}")]
        public async Task<IActionResult> ObtenerPorMatriculaCurso(int matriculaCursoId)
        {
            try
            {
                var notas = await _context.Notas
                    .Include(n => n.ComponenteEvaluacion)
                    .Include(n => n.MatriculaCurso).ThenInclude(m => m.CursoEspecializado)
                    .Include(n => n.Materia)
                    .Include(n => n.Usuario)
                    .Where(n => n.MatriculaCursoId == matriculaCursoId && n.ComponenteEvaluacionId.HasValue)
                    .OrderBy(n => n.ComponenteEvaluacion.Orden)
                    .Select(n => new
                    {
                        notaId = n.NotaId,
                        matriculaId = n.MatriculaId,
                        matriculaCursoId = n.MatriculaCursoId,
                        materiaId = n.MateriaId,
                        materiaNombre = n.Materia != null ? n.Materia.Nombre : null,
                        tipoMatricula = "Curso Especializado",
                        matriculaCodigo = n.MatriculaCurso.Codigo,
                        programaNombre = n.MatriculaCurso.CursoEspecializado.Nombre,
                        componenteEvaluacionId = n.ComponenteEvaluacionId.Value,
                        componenteNombre = n.ComponenteEvaluacion.Nombre,
                        componenteOrden = n.ComponenteEvaluacion.Orden,
                        componentePeso = n.ComponenteEvaluacion.PorcentajePeso,
                        nota = n.NotaValor.Value,
                        fechaRegistro = n.FechaRegistro,
                        observaciones = n.Observaciones,
                        usuarioRegistro = n.Usuario != null ? n.Usuario.Nombre : null
                    })
                    .ToListAsync();

                return Ok(notas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener las notas", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/Evaluaciones/CalcularPromedio - Calcular promedio final
        // Query params: ?matriculaId=X o ?matriculaCursoId=Y
        // ===================================================================
        [HttpGet("CalcularPromedio")]
        public async Task<IActionResult> CalcularPromedio([FromQuery] int? matriculaId, [FromQuery] int? matriculaCursoId)
        {
            try
            {
                // 1. Validar que solo uno esté presente
                if (!matriculaId.HasValue && !matriculaCursoId.HasValue)
                    return BadRequest(new { message = "Debe especificar matriculaId o matriculaCursoId" });

                if (matriculaId.HasValue && matriculaCursoId.HasValue)
                    return BadRequest(new { message = "Solo puede especificar matriculaId o matriculaCursoId, no ambos" });

                // 2. Obtener tipo de evaluación
                int tipoEvaluacionId;
                string tipoEvaluacionNombre;

                if (matriculaId.HasValue)
                {
                    var matricula = await _context.Matriculas
                        .FirstOrDefaultAsync(m => m.MatriculaId == matriculaId.Value);

                    if (matricula == null) return NotFound(new { message = "Matrícula no encontrada" });

                    // Para el sistema flexible, usar tipo REGULAR (1) por defecto
                    tipoEvaluacionId = 1; // REGULAR
                    tipoEvaluacionNombre = "Evaluación Regular (3 componentes)";
                }
                else
                {
                    var curso = await _context.MatriculasCurso
                        .Include(m => m.CursoEspecializado).ThenInclude(c => c.TipoEvaluacion)
                        .FirstOrDefaultAsync(m => m.MatriculaCursoId == matriculaCursoId.Value);

                    if (curso == null) return NotFound(new { message = "Matrícula de curso no encontrada" });

                    tipoEvaluacionId = curso.CursoEspecializado.TipoEvaluacionId;
                    tipoEvaluacionNombre = curso.CursoEspecializado.TipoEvaluacion.Nombre;
                }

                // 3. Componentes requeridos
                var componentesRequeridos = await _context.ComponenteEvaluacion
                    .Where(c => c.TipoEvaluacionId == tipoEvaluacionId && c.EsObligatorio && c.Activo)
                    .CountAsync();

                // 4. Notas registradas
                var notasConComponentes = await _context.Notas
                    .Include(n => n.ComponenteEvaluacion)
                    .Where(n => (matriculaId.HasValue && n.MatriculaId == matriculaId.Value) ||
                               (matriculaCursoId.HasValue && n.MatriculaCursoId == matriculaCursoId.Value))
                    .Where(n => n.ComponenteEvaluacionId.HasValue)
                    .OrderBy(n => n.ComponenteEvaluacion.Orden)
                    .ToListAsync();

                bool notasCompletas = notasConComponentes.Count >= componentesRequeridos;

                // Si no están todas las notas
                if (!notasCompletas)
                {
                    return Ok(new PromedioViewModel
                    {
                        NotaFinal = 0,
                        Estado = "Incompleto",
                        TipoEvaluacion = tipoEvaluacionNombre,
                        Detalles = notasConComponentes.Select(n => new DetalleComponenteNotaViewModel
                        {
                            ComponenteEvaluacionId = n.ComponenteEvaluacionId.Value,
                            Componente = n.ComponenteEvaluacion.Nombre,
                            Nota = n.NotaValor.Value,
                            Peso = n.ComponenteEvaluacion.PorcentajePeso,
                            Aporte = Math.Round(n.NotaValor.Value * (n.ComponenteEvaluacion.PorcentajePeso / 100m), 2),
                            Orden = n.ComponenteEvaluacion.Orden
                        }).ToList(),
                        NotasCompletas = false,
                        ComponentesRequeridos = componentesRequeridos,
                        ComponentesRegistrados = notasConComponentes.Count
                    });
                }

                // 5. Calcular promedio ponderado
                decimal notaFinal = 0;
                var detalles = notasConComponentes.Select(n =>
                {
                    decimal pesoDecimal = n.ComponenteEvaluacion.PorcentajePeso / 100m;
                    decimal aporte = n.NotaValor.Value * pesoDecimal;
                    notaFinal += aporte;

                    return new DetalleComponenteNotaViewModel
                    {
                        ComponenteEvaluacionId = n.ComponenteEvaluacionId.Value,
                        Componente = n.ComponenteEvaluacion.Nombre,
                        Nota = n.NotaValor.Value,
                        Peso = n.ComponenteEvaluacion.PorcentajePeso,
                        Aporte = Math.Round(aporte, 2),
                        Orden = n.ComponenteEvaluacion.Orden
                    };
                }).ToList();

                string estado = notaFinal >= NOTA_MINIMA_APROBATORIA ? "Aprobado" : "Reprobado";

                return Ok(new PromedioViewModel
                {
                    NotaFinal = (int)Math.Round(notaFinal, 0),
                    Estado = estado,
                    TipoEvaluacion = tipoEvaluacionNombre,
                    Detalles = detalles,
                    NotasCompletas = true,
                    ComponentesRequeridos = componentesRequeridos,
                    ComponentesRegistrados = notasConComponentes.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al calcular el promedio", error = ex.Message });
            }
        }

        // ===================================================================
        // PUT: api/Evaluaciones/{id} - Actualizar nota
        // ===================================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarNotaViewModel model)
        {
            try
            {
                var nota = await _context.Notas
                    .Include(n => n.ComponenteEvaluacion)
                    .FirstOrDefaultAsync(n => n.NotaId == id && n.ComponenteEvaluacionId.HasValue);

                if (nota == null)
                    return NotFound(new { message = "Nota no encontrada" });

                // Obtener el usuario del token JWT
                int? usuarioId = null;
                if (User?.Claims != null)
                {
                    var usuarioIdClaim = User.Claims.FirstOrDefault(x => x.Type == "usuarioId");
                    if (usuarioIdClaim != null)
                    {
                        usuarioId = int.Parse(usuarioIdClaim.Value);
                    }
                }

                nota.NotaValor = model.Nota;
                nota.Observaciones = model.Observaciones;
                nota.FechaRegistro = DateTime.Now;
                nota.UsuarioRegistroId = usuarioId;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Nota actualizada exitosamente",
                    notaId = nota.NotaId,
                    componente = nota.ComponenteEvaluacion.Nombre,
                    notaNueva = nota.NotaValor
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la nota", error = ex.Message });
            }
        }

        // ===================================================================
        // DELETE: api/Evaluaciones/{id} - Eliminar nota
        // ===================================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var nota = await _context.Notas
                    .FirstOrDefaultAsync(n => n.NotaId == id && n.ComponenteEvaluacionId.HasValue);

                if (nota == null)
                    return NotFound(new { message = "Nota no encontrada" });

                _context.Notas.Remove(nota);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Nota eliminada exitosamente", notaId = id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la nota", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/Evaluaciones/ListarConProgreso - Listado con progreso
        // ===================================================================
        [HttpGet("[action]")]
        public async Task<IActionResult> ListarConProgreso(
            [FromQuery] string tipoPrograma = "academico", // "academico" o "especializado"
            [FromQuery] int? anioLectivoId = null,
            [FromQuery] int? moduloId = null,
            [FromQuery] string busqueda = null,
            [FromQuery] string estado = null, // "pendiente", "en_progreso", "completado"
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            try
            {
                if (tipoPrograma == "academico")
                {
                    // Obtener todas las matrículas académicas con sus relaciones
                    var query = _context.Matriculas
                        .Include(m => m.Estudiante)
                        .Include(m => m.Modulo)
                            .ThenInclude(mod => mod.AnioLectivo)
                        .Include(m => m.Notas)
                            .ThenInclude(n => n.ComponenteEvaluacion)
                        .AsQueryable();

                    // Filtros
                    if (anioLectivoId.HasValue)
                        query = query.Where(m => m.Modulo.AnioLectivoId == anioLectivoId.Value);

                    if (moduloId.HasValue)
                        query = query.Where(m => m.ModuloId == moduloId.Value);

                    if (!string.IsNullOrWhiteSpace(busqueda))
                    {
                        busqueda = busqueda.ToLower();
                        query = query.Where(m =>
                            m.Codigo.ToLower().Contains(busqueda) ||
                            m.Estudiante.NombreCompleto.ToLower().Contains(busqueda));
                    }

                    // Total registros
                    var total = await query.CountAsync();

                    // Obtener matrículas paginadas
                    var matriculas = await query
                        .OrderByDescending(m => m.FechaMatricula)
                        .Skip((pagina - 1) * registrosPorPagina)
                        .Take(registrosPorPagina)
                        .ToListAsync();

                    // Proyectar a ViewModel con progreso
                    var resultado = matriculas.Select(m =>
                    {
                        const int COMPONENTES_REQUERIDOS = 3; // REGULAR
                        var notasRegistradas = m.Notas.Count(n => n.ComponenteEvaluacionId.HasValue);
                        var progreso = notasRegistradas > 0 ? Math.Round((decimal)notasRegistradas / COMPONENTES_REQUERIDOS * 100, 2) : 0;

                        string estadoEvaluacion;
                        if (notasRegistradas == 0)
                            estadoEvaluacion = "Pendiente";
                        else if (notasRegistradas < COMPONENTES_REQUERIDOS)
                            estadoEvaluacion = "En Progreso";
                        else
                            estadoEvaluacion = "Completado";

                        // Calcular nota final si está completo
                        int? notaFinal = null;
                        if (notasRegistradas == COMPONENTES_REQUERIDOS)
                        {
                            decimal suma = 0;
                            foreach (var nota in m.Notas.Where(n => n.ComponenteEvaluacionId.HasValue))
                            {
                                suma += nota.NotaValor.Value * (nota.ComponenteEvaluacion.PorcentajePeso / 100m);
                            }
                            notaFinal = (int)Math.Round(suma, 0);
                        }

                        return new
                        {
                            matriculaId = m.MatriculaId,
                            codigo = m.Codigo,
                            estudiante = new
                            {
                                estudianteId = m.Estudiante.EstudianteId,
                                nombreCompleto = m.Estudiante.NombreCompleto,
                                codigo = m.Estudiante.Codigo
                            },
                            programa = new
                            {
                                moduloId = m.ModuloId,
                                nombre = m.Modulo.Nombre,
                                anioLectivo = m.Modulo.AnioLectivo.Nombre
                            },
                            tipoEvaluacion = "Evaluación Regular (3 componentes)",
                            progreso = new
                            {
                                componentesRegistrados = notasRegistradas,
                                componentesRequeridos = COMPONENTES_REQUERIDOS,
                                porcentaje = progreso
                            },
                            notaFinal,
                            estadoEvaluacion,
                            estadoAprobacion = notaFinal.HasValue ? (notaFinal.Value >= 70 ? "Aprobado" : "Reprobado") : null
                        };
                    }).AsQueryable();

                    // Filtro adicional por estado de evaluación
                    if (!string.IsNullOrWhiteSpace(estado))
                    {
                        resultado = resultado.Where(r =>
                            r.estadoEvaluacion.Replace(" ", "_").ToLower() == estado.ToLower());
                    }

                    return Ok(new
                    {
                        data = resultado.ToList(),
                        paginacion = new
                        {
                            paginaActual = pagina,
                            registrosPorPagina,
                            totalRegistros = total,
                            totalPaginas = (int)Math.Ceiling(total / (double)registrosPorPagina)
                        }
                    });
                }
                else // especializado
                {
                    // Similar lógica para cursos especializados
                    var query = _context.MatriculasCurso
                        .Include(m => m.Estudiante)
                        .Include(m => m.CursoEspecializado)
                            .ThenInclude(c => c.TipoEvaluacion)
                        .Include(m => m.Notas)
                            .ThenInclude(n => n.ComponenteEvaluacion)
                        .AsQueryable();

                    if (!string.IsNullOrWhiteSpace(busqueda))
                    {
                        busqueda = busqueda.ToLower();
                        query = query.Where(m =>
                            m.Codigo.ToLower().Contains(busqueda) ||
                            m.Estudiante.NombreCompleto.ToLower().Contains(busqueda));
                    }

                    var total = await query.CountAsync();

                    var matriculasCurso = await query
                        .OrderByDescending(m => m.FechaMatricula)
                        .Skip((pagina - 1) * registrosPorPagina)
                        .Take(registrosPorPagina)
                        .ToListAsync();

                    var resultado = matriculasCurso.Select(m =>
                    {
                        var COMPONENTES_REQUERIDOS = m.CursoEspecializado.TipoEvaluacion.CantidadComponentes;
                        var notasRegistradas = m.Notas.Count(n => n.ComponenteEvaluacionId.HasValue);
                        var progreso = notasRegistradas > 0 ? Math.Round((decimal)notasRegistradas / COMPONENTES_REQUERIDOS * 100, 2) : 0;

                        string estadoEvaluacion;
                        if (notasRegistradas == 0)
                            estadoEvaluacion = "Pendiente";
                        else if (notasRegistradas < COMPONENTES_REQUERIDOS)
                            estadoEvaluacion = "En Progreso";
                        else
                            estadoEvaluacion = "Completado";

                        int? notaFinal = null;
                        if (notasRegistradas == COMPONENTES_REQUERIDOS)
                        {
                            decimal suma = 0;
                            foreach (var nota in m.Notas.Where(n => n.ComponenteEvaluacionId.HasValue))
                            {
                                suma += nota.NotaValor.Value * (nota.ComponenteEvaluacion.PorcentajePeso / 100m);
                            }
                            notaFinal = (int)Math.Round(suma, 0);
                        }

                        return new
                        {
                            matriculaCursoId = m.MatriculaCursoId,
                            codigo = m.Codigo,
                            estudiante = new
                            {
                                estudianteId = m.Estudiante.EstudianteId,
                                nombreCompleto = m.Estudiante.NombreCompleto,
                                codigo = m.Estudiante.Codigo
                            },
                            programa = new
                            {
                                cursoEspecializadoId = m.CursoEspecializadoId,
                                nombre = m.CursoEspecializado.Nombre
                            },
                            tipoEvaluacion = m.CursoEspecializado.TipoEvaluacion.Nombre,
                            progreso = new
                            {
                                componentesRegistrados = notasRegistradas,
                                componentesRequeridos = COMPONENTES_REQUERIDOS,
                                porcentaje = progreso
                            },
                            notaFinal,
                            estadoEvaluacion,
                            estadoAprobacion = notaFinal.HasValue ? (notaFinal.Value >= 70 ? "Aprobado" : "Reprobado") : null
                        };
                    }).AsQueryable();

                    if (!string.IsNullOrWhiteSpace(estado))
                    {
                        resultado = resultado.Where(r =>
                            r.estadoEvaluacion.Replace(" ", "_").ToLower() == estado.ToLower());
                    }

                    return Ok(new
                    {
                        data = resultado.ToList(),
                        paginacion = new
                        {
                            paginaActual = pagina,
                            registrosPorPagina,
                            totalRegistros = total,
                            totalPaginas = (int)Math.Ceiling(total / (double)registrosPorPagina)
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el listado", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/Evaluaciones/HistorialEstudiante/{id} - Historial completo
        // ===================================================================
        [HttpGet("HistorialEstudiante/{estudianteId}")]
        public async Task<IActionResult> HistorialEstudiante(int estudianteId)
        {
            try
            {
                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.EstudianteId == estudianteId);

                if (estudiante == null)
                    return NotFound(new { message = "Estudiante no encontrado" });

                // Notas académicas (nuevo sistema)
                var notasAcademicas = await _context.Notas
                    .Include(n => n.Matricula)
                        .ThenInclude(m => m.Modulo)
                            .ThenInclude(mod => mod.AnioLectivo)
                    .Include(n => n.Materia)
                    .Include(n => n.ComponenteEvaluacion)
                        .ThenInclude(c => c.TipoEvaluacion)
                    .Include(n => n.Usuario)
                    .Where(n => n.Matricula.EstudianteId == estudianteId && n.ComponenteEvaluacionId.HasValue)
                    .OrderByDescending(n => n.FechaRegistro)
                    .Select(n => new
                    {
                        notaId = n.NotaId,
                        matriculaId = n.MatriculaId,
                        tipoPrograma = "Académico",
                        programa = n.Matricula.Modulo.Nombre,
                        materiaId = n.MateriaId,
                        materia = n.Materia != null ? n.Materia.Nombre : n.Matricula.Modulo.Nombre,
                        anioLectivo = n.Matricula.Modulo.AnioLectivo.Nombre,
                        tipoEvaluacion = n.ComponenteEvaluacion.TipoEvaluacion.Nombre,
                        componente = n.ComponenteEvaluacion.Nombre,
                        peso = n.ComponenteEvaluacion.PorcentajePeso,
                        nota = n.NotaValor,
                        fechaRegistro = n.FechaRegistro,
                        usuarioRegistro = n.Usuario != null ? n.Usuario.Nombre : null,
                        observaciones = n.Observaciones
                    })
                    .ToListAsync();

                // Notas de cursos especializados
                var notasEspecializadas = await _context.Notas
                    .Include(n => n.MatriculaCurso)
                        .ThenInclude(m => m.CursoEspecializado)
                    .Include(n => n.ComponenteEvaluacion)
                        .ThenInclude(c => c.TipoEvaluacion)
                    .Include(n => n.Usuario)
                    .Where(n => n.MatriculaCurso.EstudianteId == estudianteId && n.ComponenteEvaluacionId.HasValue)
                    .OrderByDescending(n => n.FechaRegistro)
                    .Select(n => new
                    {
                        notaId = n.NotaId,
                        matriculaId = n.MatriculaCursoId,
                        tipoPrograma = "Curso Especializado",
                        programa = n.MatriculaCurso.CursoEspecializado.Nombre,
                        materiaId = (int?)null,
                        materia = n.MatriculaCurso.CursoEspecializado.Nombre,
                        anioLectivo = (string)null,
                        tipoEvaluacion = n.ComponenteEvaluacion.TipoEvaluacion.Nombre,
                        componente = n.ComponenteEvaluacion.Nombre,
                        peso = n.ComponenteEvaluacion.PorcentajePeso,
                        nota = n.NotaValor,
                        fechaRegistro = n.FechaRegistro,
                        usuarioRegistro = n.Usuario != null ? n.Usuario.Nombre : null,
                        observaciones = n.Observaciones
                    })
                    .ToListAsync();

                // Calcular estadísticas
                var todasLasNotas = notasAcademicas.Concat(notasEspecializadas).ToList();
                var totalNotas = todasLasNotas.Count;
                var promedioGeneral = totalNotas > 0 ? Math.Round(todasLasNotas.Average(n => n.nota.Value), 2) : 0;
                var notaMasAlta = totalNotas > 0 ? todasLasNotas.Max(n => n.nota.Value) : 0;
                var notaMasBaja = totalNotas > 0 ? todasLasNotas.Min(n => n.nota.Value) : 0;

                return Ok(new
                {
                    estudiante = new
                    {
                        estudianteId = estudiante.EstudianteId,
                        codigo = estudiante.Codigo,
                        nombreCompleto = estudiante.NombreCompleto,
                        email = estudiante.CorreoElectronico,
                        telefono = estudiante.Celular
                    },
                    estadisticas = new
                    {
                        totalNotas,
                        promedioGeneral,
                        notaMasAlta,
                        notaMasBaja,
                        notasAcademicas = notasAcademicas.Count,
                        notasEspecializadas = notasEspecializadas.Count
                    },
                    notas = new
                    {
                        academicas = notasAcademicas,
                        especializadas = notasEspecializadas
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el historial", error = ex.Message });
            }
        }
    }
}
