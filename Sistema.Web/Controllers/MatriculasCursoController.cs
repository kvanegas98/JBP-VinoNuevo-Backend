using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Instituto;
using Sistema.Web.Models.MatriculasCurso;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MatriculasCursoController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public MatriculasCursoController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/MatriculasCurso/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var matriculas = await _context.MatriculasCurso
                .Include(m => m.Estudiante)
                .Include(m => m.CursoEspecializado)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .OrderByDescending(m => m.FechaMatricula)
                .Select(m => new MatriculaCursoViewModel
                {
                    MatriculaCursoId = m.MatriculaCursoId,
                    Codigo = m.Codigo,
                    EstudianteId = m.EstudianteId,
                    EstudianteCodigo = m.Estudiante.Codigo,
                    EstudianteNombre = m.Estudiante.NombreCompleto,
                    CursoEspecializadoId = m.CursoEspecializadoId,
                    CursoNombre = m.CursoEspecializado.Nombre,
                    ModalidadId = m.ModalidadId,
                    ModalidadNombre = m.Modalidad.Nombre,
                    CategoriaEstudianteId = m.CategoriaEstudianteId,
                    CategoriaEstudianteNombre = m.CategoriaEstudiante.Nombre,
                    FechaMatricula = m.FechaMatricula,
                    MontoMatricula = m.MontoMatricula,
                    DescuentoAplicado = m.DescuentoAplicado,
                    MontoFinal = m.MontoFinal,
                    Estado = m.Estado,
                    Aprobado = m.Aprobado
                })
                .ToListAsync();

            return Ok(matriculas);
        }

        // GET: api/MatriculasCurso/ListarPorEstudiante/{estudianteId}
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> ListarPorEstudiante([FromRoute] int estudianteId)
        {
            var matriculas = await _context.MatriculasCurso
                .Include(m => m.CursoEspecializado)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .Where(m => m.EstudianteId == estudianteId)
                .OrderByDescending(m => m.FechaMatricula)
                .Select(m => new
                {
                    m.MatriculaCursoId,
                    m.Codigo,
                    m.CursoEspecializadoId,
                    CursoNombre = m.CursoEspecializado.Nombre,
                    CursoFechaInicio = m.CursoEspecializado.FechaInicio,
                    CursoFechaFin = m.CursoEspecializado.FechaFin,
                    ModalidadNombre = m.Modalidad.Nombre,
                    CategoriaEstudianteNombre = m.CategoriaEstudiante.Nombre,
                    m.FechaMatricula,
                    m.MontoMatricula,
                    m.DescuentoAplicado,
                    m.MontoFinal,
                    m.Estado,
                    m.Aprobado
                })
                .ToListAsync();

            return Ok(matriculas);
        }

        // POST: api/MatriculasCurso/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearMatriculaCursoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verificar que el estudiante existe
                var estudiante = await _context.Estudiantes
                    .Include(e => e.EstudianteCargos)
                        .ThenInclude(ec => ec.Cargo)
                    .FirstOrDefaultAsync(e => e.EstudianteId == model.EstudianteId);

                if (estudiante == null)
                {
                    return BadRequest(new { message = "Estudiante no encontrado" });
                }

                // Verificar que el curso existe y estÃƒÂ¡ activo
                var curso = await _context.CursosEspecializados
                    .FirstOrDefaultAsync(c => c.CursoEspecializadoId == model.CursoEspecializadoId);

                if (curso == null)
                {
                    return BadRequest(new { message = "Curso no encontrado" });
                }

                if (!curso.Activo)
                {
                    return BadRequest(new { message = "El curso no estÃƒÂ¡ activo" });
                }

                // Verificar que no tenga matrÃƒÂ­cula activa o pendiente en el mismo curso
                var matriculaExistente = await _context.MatriculasCurso
                    .FirstOrDefaultAsync(m => m.EstudianteId == model.EstudianteId
                                           && m.CursoEspecializadoId == model.CursoEspecializadoId
                                           && (m.Estado == "Activa" || m.Estado == "Pendiente"));

                if (matriculaExistente != null)
                {
                    return BadRequest(new
                    {
                        message = $"El estudiante ya tiene una matrÃƒÂ­cula {matriculaExistente.Estado.ToLower()} en este curso",
                        matriculaCursoId = matriculaExistente.MatriculaCursoId,
                        codigo = matriculaExistente.Codigo,
                        estado = matriculaExistente.Estado
                    });
                }

                // Verificar que no haya aprobado el curso anteriormente
                var haAprobado = await _context.MatriculasCurso
                    .AnyAsync(m => m.EstudianteId == model.EstudianteId
                               && m.CursoEspecializadoId == model.CursoEspecializadoId
                               && m.Aprobado);

                if (haAprobado)
                {
                    return BadRequest(new { message = "El estudiante ya aprobÃƒÂ³ este curso y no puede volver a matricularse" });
                }

                // Obtener el cargo del estudiante (si es interno)
                int? cargoId = null;
                if (estudiante.EsInterno && estudiante.EstudianteCargos != null && estudiante.EstudianteCargos.Any(ec => ec.Cargo != null))
                {
                    var cargo = estudiante.EstudianteCargos.First(ec => ec.Cargo != null);
                    cargoId = cargo.CargoId;
                }

                // Obtener precio de matrÃƒÂ­cula segÃƒÂºn categorÃƒÂ­a + cargo
                var precioMatricula = await _context.PreciosMatriculaCurso
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                                           && p.CargoId == cargoId
                                           && p.Activo);

                // Si no encuentra precio especÃƒÂ­fico para el cargo, buscar sin cargo
                if (precioMatricula == null && cargoId.HasValue)
                {
                    precioMatricula = await _context.PreciosMatriculaCurso
                        .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                                               && p.CargoId == null
                                               && p.Activo);
                }

                if (precioMatricula == null)
                {
                    return BadRequest(new
                    {
                        message = "No se encontrÃƒÂ³ precio configurado para cursos especializados",
                        categoriaEstudianteId = model.CategoriaEstudianteId,
                        cargoId = cargoId,
                        esInterno = estudiante.EsInterno,
                        detalle = $"Buscando precio para categorÃƒÂ­a {model.CategoriaEstudianteId} y cargo {(cargoId.HasValue ? cargoId.Value.ToString() : "NULL")}"
                    });
                }

                decimal montoMatricula = precioMatricula.Precio;
                decimal descuento = 0;

                // Aplicar beca si el estudiante estÃƒÂ¡ becado
                if (estudiante.EsBecado && estudiante.PorcentajeBeca > 0)
                {
                    descuento = montoMatricula * (estudiante.PorcentajeBeca / 100);
                }

                // Generar cÃƒÂ³digo automÃƒÂ¡tico MCURSO-2026-0001, MCURSO-2026-0002, etc.
                var anioActual = DateTime.Now.Year;
                var prefijoAnio = $"MCURSO-{anioActual}-";
                var ultimoCodigo = await _context.MatriculasCurso
                    .Where(m => m.Codigo != null && m.Codigo.StartsWith(prefijoAnio))
                    .OrderByDescending(m => m.Codigo)
                    .Select(m => m.Codigo)
                    .FirstOrDefaultAsync();

                int siguienteNumero = 1;
                if (!string.IsNullOrEmpty(ultimoCodigo))
                {
                    var partes = ultimoCodigo.Split('-');
                    if (partes.Length == 3)
                    {
                        int.TryParse(partes[2], out siguienteNumero);
                        siguienteNumero++;
                    }
                }

                decimal montoFinal = montoMatricula - descuento;

                // Si el monto es $0 (becado 100%), activar automÃƒÂ¡ticamente sin necesidad de pago
                string estadoInicial = montoFinal == 0 ? "Activa" : "Pendiente";

                // Si estÃƒÂ¡ becado 100%, agregar observaciÃƒÂ³n automÃƒÂ¡tica
                string observaciones = montoFinal == 0 ? "Becado 100%" : null;

                var matricula = new MatriculaCurso
                {
                    Codigo = $"{prefijoAnio}{siguienteNumero:D4}",
                    EstudianteId = model.EstudianteId,
                    CursoEspecializadoId = model.CursoEspecializadoId,
                    ModalidadId = model.ModalidadId,
                    CategoriaEstudianteId = model.CategoriaEstudianteId,
                    FechaMatricula = DateTime.Now,
                    MontoMatricula = montoMatricula,
                    DescuentoAplicado = descuento,
                    MontoFinal = montoFinal,
                    Estado = estadoInicial,
                    Aprobado = false,
                    Observaciones = observaciones
                };

                _context.MatriculasCurso.Add(matricula);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    matriculaCursoId = matricula.MatriculaCursoId,
                    codigo = matricula.Codigo,
                    montoMatricula = matricula.MontoMatricula,
                    descuentoAplicado = matricula.DescuentoAplicado,
                    montoFinal = matricula.MontoFinal,
                    estado = matricula.Estado,
                    activadaAutomaticamente = montoFinal == 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear matrÃƒÂ­cula del curso",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/MatriculasCurso/CalcularPrecio/{estudianteId}/{cursoId}/{modalidadId}/{categoriaId}
        [HttpGet("[action]/{estudianteId}/{cursoId}/{modalidadId}/{categoriaId}")]
        public async Task<IActionResult> CalcularPrecio([FromRoute] int estudianteId, [FromRoute] int cursoId, [FromRoute] int modalidadId, [FromRoute] int categoriaId)
        {
            var estudiante = await _context.Estudiantes
                .Include(e => e.EstudianteCargos)
                    .ThenInclude(ec => ec.Cargo)
                .FirstOrDefaultAsync(e => e.EstudianteId == estudianteId);

            if (estudiante == null)
            {
                return NotFound(new { message = "Estudiante no encontrado" });
            }

            // Obtener el cargo del estudiante (si es interno)
            int? cargoId = null;
            if (estudiante.EsInterno && estudiante.EstudianteCargos != null && estudiante.EstudianteCargos.Any(ec => ec.Cargo != null))
            {
                var cargo = estudiante.EstudianteCargos.First(ec => ec.Cargo != null);
                cargoId = cargo.CargoId;
            }

            // Obtener precio de matrÃƒÂ­cula segÃƒÂºn categorÃƒÂ­a + cargo
            var precioMatricula = await _context.PreciosMatriculaCurso
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio especÃƒÂ­fico para el cargo, buscar sin cargo
            if (precioMatricula == null && cargoId.HasValue)
            {
                precioMatricula = await _context.PreciosMatriculaCurso
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            // Obtener precio de mensualidad segÃƒÂºn categorÃƒÂ­a + cargo
            var precioMensualidad = await _context.PreciosMensualidadCurso
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio especÃƒÂ­fico para el cargo, buscar sin cargo
            if (precioMensualidad == null && cargoId.HasValue)
            {
                precioMensualidad = await _context.PreciosMensualidadCurso
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            decimal montoMatricula = precioMatricula?.Precio ?? 0;
            decimal montoMensualidad = precioMensualidad?.Precio ?? 0;

            // Calcular descuento por beca
            decimal descuentoMatricula = 0;
            decimal descuentoMensualidad = 0;
            if (estudiante.EsBecado && estudiante.PorcentajeBeca > 0)
            {
                descuentoMatricula = montoMatricula * (estudiante.PorcentajeBeca / 100);
                descuentoMensualidad = montoMensualidad * (estudiante.PorcentajeBeca / 100);
            }

            return Ok(new
            {
                estudianteId = estudiante.EstudianteId,
                estudianteNombre = estudiante.NombreCompleto,
                esBecado = estudiante.EsBecado,
                porcentajeBeca = estudiante.PorcentajeBeca,
                matricula = new
                {
                    precio = montoMatricula,
                    descuento = descuentoMatricula,
                    precioFinal = montoMatricula - descuentoMatricula
                },
                mensualidad = new
                {
                    precio = montoMensualidad,
                    descuento = descuentoMensualidad,
                    precioFinal = montoMensualidad - descuentoMensualidad
                }
            });
        }

        // PUT: api/MatriculasCurso/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var matricula = await _context.MatriculasCurso
                .Include(m => m.Estudiante)
                .FirstOrDefaultAsync(m => m.MatriculaCursoId == id);

            if (matricula == null)
            {
                return NotFound(new { message = "MatrÃƒÂ­cula no encontrada" });
            }

            if (matricula.Estado == "Anulada")
            {
                return BadRequest(new { message = "No se puede activar una matrÃƒÂ­cula anulada" });
            }

            if (matricula.Estado == "Activa")
            {
                return BadRequest(new { message = "La matrÃƒÂ­cula ya estÃƒÂ¡ activa" });
            }

            matricula.Estado = "Activa";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                matriculaCursoId = matricula.MatriculaCursoId,
                codigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                estado = matricula.Estado,
                message = "MatrÃƒÂ­cula activada exitosamente"
            });
        }

        // PUT: api/MatriculasCurso/Anular/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Anular([FromRoute] int id)
        {
            var matricula = await _context.MatriculasCurso
                .Include(m => m.Estudiante)
                .FirstOrDefaultAsync(m => m.MatriculaCursoId == id);

            if (matricula == null)
            {
                return NotFound(new { message = "MatrÃƒÂ­cula no encontrada" });
            }

            if (matricula.Estado == "Anulada")
            {
                return BadRequest(new { message = "La matrÃƒÂ­cula ya estÃƒÂ¡ anulada" });
            }

            matricula.Estado = "Anulada";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                matriculaCursoId = matricula.MatriculaCursoId,
                codigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                estado = matricula.Estado,
                message = "MatrÃƒÂ­cula anulada exitosamente"
            });
        }

        // PUT: api/MatriculasCurso/MarcarComoAprobado/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> MarcarComoAprobado([FromRoute] int id)
        {
            var matricula = await _context.MatriculasCurso
                .Include(m => m.Estudiante)
                .FirstOrDefaultAsync(m => m.MatriculaCursoId == id);

            if (matricula == null)
            {
                return NotFound(new { message = "MatrÃƒÂ­cula no encontrada" });
            }

            if (matricula.Estado != "Activa" && matricula.Estado != "Completada")
            {
                return BadRequest(new { message = "Solo se pueden marcar como aprobadas las matrÃƒÂ­culas activas o completadas" });
            }

            matricula.Aprobado = true;
            matricula.Estado = "Completada";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                matriculaCursoId = matricula.MatriculaCursoId,
                codigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                estado = matricula.Estado,
                aprobado = matricula.Aprobado,
                message = "MatrÃƒÂ­cula marcada como aprobada"
            });
        }

        // PUT: api/MatriculasCurso/MarcarComoNoAprobado/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> MarcarComoNoAprobado([FromRoute] int id)
        {
            var matricula = await _context.MatriculasCurso
                .Include(m => m.Estudiante)
                .FirstOrDefaultAsync(m => m.MatriculaCursoId == id);

            if (matricula == null)
            {
                return NotFound(new { message = "MatrÃƒÂ­cula no encontrada" });
            }

            if (matricula.Estado != "Activa" && matricula.Estado != "Completada")
            {
                return BadRequest(new { message = "Solo se pueden marcar como no aprobadas las matrÃƒÂ­culas activas o completadas" });
            }

            matricula.Aprobado = false;
            matricula.Estado = "Completada";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                matriculaCursoId = matricula.MatriculaCursoId,
                codigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                estado = matricula.Estado,
                aprobado = matricula.Aprobado,
                message = "MatrÃƒÂ­cula marcada como no aprobada"
            });
        }
    }

    public class CrearMatriculaCursoViewModel
    {
        public int EstudianteId { get; set; }
        public int CursoEspecializadoId { get; set; }
        public int ModalidadId { get; set; }
        public int CategoriaEstudianteId { get; set; }
    }
}
