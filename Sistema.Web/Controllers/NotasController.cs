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
    [Route("api/[controller]")]
    [ApiController]
    public class NotasController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public NotasController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Notas/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var notas = await _context.Notas
                .Include(n => n.Matricula)
                    .ThenInclude(m => m.Estudiante)
                .Include(n => n.Matricula)
                    .ThenInclude(m => m.Modulo)
                        .ThenInclude(mod => mod.AnioLectivo)
                .Include(n => n.Materia)
                .OrderByDescending(n => n.FechaRegistro)
                .Select(n => new NotaViewModel
                {
                    NotaId = n.NotaId,
                    MatriculaId = n.MatriculaId,
                    EstudianteNombre = n.Matricula.Estudiante.NombreCompleto,
                    MateriaId = n.MateriaId,
                    MateriaNombre = n.Materia.Nombre,
                    Calificacion = n.Calificacion,
                    FechaRegistro = n.FechaRegistro,
                    Observaciones = n.Observaciones,
                    AnioLectivoId = n.Matricula.Modulo.AnioLectivoId,
                    AnioLectivoNombre = n.Matricula.Modulo.AnioLectivo.Nombre
                })
                .ToListAsync();

            return Ok(notas);
        }

        // POST: api/Notas/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearNotaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que la matrícula existe y está activa
            var matricula = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.MatriculaId == model.MatriculaId && m.Estado == "Activa");

            if (matricula == null)
            {
                return BadRequest(new { message = "Matrícula no encontrada o no está activa" });
            }

            var nota = new Nota
            {
                MatriculaId = model.MatriculaId,
                MateriaId = model.MateriaId,
                Calificacion = model.Calificacion,
                FechaRegistro = DateTime.Now,
                Observaciones = model.Observaciones
            };

            _context.Notas.Add(nota);
            await _context.SaveChangesAsync();

            return Ok(new { notaId = nota.NotaId });
        }

        // PUT: api/Notas/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarNotaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nota = await _context.Notas.FindAsync(model.NotaId);

            if (nota == null)
            {
                return NotFound();
            }

            nota.MatriculaId = model.MatriculaId;
            nota.MateriaId = model.MateriaId;
            nota.Calificacion = model.Calificacion;
            nota.Observaciones = model.Observaciones;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Notas/Eliminar/{id}
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var nota = await _context.Notas.FindAsync(id);

            if (nota == null)
            {
                return NotFound();
            }

            _context.Notas.Remove(nota);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
