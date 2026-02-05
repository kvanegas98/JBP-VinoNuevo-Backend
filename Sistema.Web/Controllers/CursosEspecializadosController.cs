using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Catalogos;
using Sistema.Web.Models.Catalogos;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CursosEspecializadosController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public CursosEspecializadosController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/CursosEspecializados/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar([FromQuery] bool? soloActivos)
        {
            var query = _context.CursosEspecializados.AsQueryable();

            if (soloActivos.HasValue && soloActivos.Value)
            {
                query = query.Where(c => c.Activo);
            }

            var cursos = await query
                .OrderByDescending(c => c.FechaCreacion)
                .Select(c => new
                {
                    c.CursoEspecializadoId,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.FechaInicio,
                    c.FechaFin,
                    c.Activo,
                    c.FechaCreacion,
                    TotalMatriculas = c.Matriculas.Count(m => m.Estado == "Activa" || m.Estado == "Completada")
                })
                .ToListAsync();

            return Ok(cursos);
        }

        // GET: api/CursosEspecializados/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var cursos = await _context.CursosEspecializados
                .Where(c => c.Activo && c.FechaFin >= DateTime.Now)
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectViewModel
                {
                    Id = c.CursoEspecializadoId,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(cursos);
        }

        // GET: api/CursosEspecializados/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var curso = await _context.CursosEspecializados
                .Where(c => c.CursoEspecializadoId == id)
                .Select(c => new
                {
                    c.CursoEspecializadoId,
                    c.Codigo,
                    c.Nombre,
                    c.Descripcion,
                    c.FechaInicio,
                    c.FechaFin,
                    c.Activo,
                    c.FechaCreacion
                })
                .FirstOrDefaultAsync();

            if (curso == null)
            {
                return NotFound();
            }

            return Ok(curso);
        }

        // POST: api/CursosEspecializados/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearCursoEspecializadoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar fechas
            if (model.FechaInicio >= model.FechaFin)
            {
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
            }

            // Generar código único
            var ultimoCurso = await _context.CursosEspecializados
                .OrderByDescending(c => c.CursoEspecializadoId)
                .FirstOrDefaultAsync();

            var anio = DateTime.Now.Year;
            var numeroSecuencial = 1;

            if (ultimoCurso != null && ultimoCurso.Codigo.StartsWith($"CURSO-{anio}"))
            {
                var partes = ultimoCurso.Codigo.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int numero))
                {
                    numeroSecuencial = numero + 1;
                }
            }

            var curso = new CursoEspecializado
            {
                Codigo = $"CURSO-{anio}-{numeroSecuencial:D3}",
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.CursosEspecializados.Add(curso);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                curso.CursoEspecializadoId,
                curso.Codigo,
                curso.Nombre,
                curso.Descripcion,
                curso.FechaInicio,
                curso.FechaFin,
                curso.Activo,
                curso.FechaCreacion
            });
        }

        // PUT: api/CursosEspecializados/Actualizar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] ActualizarCursoEspecializadoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var curso = await _context.CursosEspecializados.FindAsync(id);

            if (curso == null)
            {
                return NotFound();
            }

            // Validar fechas
            if (model.FechaInicio >= model.FechaFin)
            {
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
            }

            curso.Nombre = model.Nombre;
            curso.Descripcion = model.Descripcion;
            curso.FechaInicio = model.FechaInicio;
            curso.FechaFin = model.FechaFin;
            curso.Activo = model.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                curso.CursoEspecializadoId,
                curso.Codigo,
                curso.Nombre,
                curso.Descripcion,
                curso.FechaInicio,
                curso.FechaFin,
                curso.Activo,
                curso.FechaCreacion
            });
        }

        // PUT: api/CursosEspecializados/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var curso = await _context.CursosEspecializados.FindAsync(id);

            if (curso == null)
            {
                return NotFound();
            }

            curso.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/CursosEspecializados/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var curso = await _context.CursosEspecializados.FindAsync(id);

            if (curso == null)
            {
                return NotFound();
            }

            // Verificar que no tenga matrículas activas
            var tieneMatriculasActivas = await _context.MatriculasCurso
                .AnyAsync(m => m.CursoEspecializadoId == id && m.Estado == "Activa");

            if (tieneMatriculasActivas)
            {
                return BadRequest("No se puede desactivar un curso con matrículas activas.");
            }

            curso.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
