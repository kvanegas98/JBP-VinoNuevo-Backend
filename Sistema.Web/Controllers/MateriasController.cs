using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Catalogos;
using Sistema.Web.Models.Catalogos;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MateriasController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public MateriasController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Materias/Listar?moduloId=1
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar([FromQuery] int? moduloId)
        {
            var query = _context.Materias
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .AsQueryable();

            // Filtrar por mÃƒÂ³dulo si se proporciona
            if (moduloId.HasValue)
            {
                query = query.Where(m => m.ModuloId == moduloId.Value);
            }

            var materias = await query
                .OrderBy(m => m.Modulo.AnioLectivoId)
                .ThenBy(m => m.Modulo.Numero)
                .ThenBy(m => m.Orden)
                .Select(m => new
                {
                    m.MateriaId,
                    m.Nombre,
                    m.ModuloId,
                    ModuloNombre = m.Modulo.Nombre,
                    AnioLectivoId = m.Modulo.AnioLectivoId,
                    AnioLectivoNombre = m.Modulo.AnioLectivo.Nombre,
                    m.Orden,
                    m.Activo
                })
                .ToListAsync();

            return Ok(materias);
        }

        // GET: api/Materias/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var materias = await _context.Materias
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .Where(m => m.Activo)
                .OrderBy(m => m.Modulo.AnioLectivoId)
                .ThenBy(m => m.Modulo.Numero)
                .ThenBy(m => m.Nombre)
                .Select(m => new SelectViewModel
                {
                    Id = m.MateriaId,
                    Nombre = $"{m.Nombre} ({m.Modulo.Nombre} - {m.Modulo.AnioLectivo.Nombre})"
                })
                .ToListAsync();

            return Ok(materias);
        }

        // GET: api/Materias/SelectPorModulo/{moduloId}
        [HttpGet("[action]/{moduloId}")]
        public async Task<IActionResult> SelectPorModulo([FromRoute] int moduloId)
        {
            var materias = await _context.Materias
                .Where(m => m.Activo && m.ModuloId == moduloId)
                .OrderBy(m => m.Orden)
                .Select(m => new SelectViewModel
                {
                    Id = m.MateriaId,
                    Nombre = m.Nombre
                })
                .ToListAsync();

            return Ok(materias);
        }

        // GET: api/Materias/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var materia = await _context.Materias
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .Where(m => m.MateriaId == id)
                .Select(m => new
                {
                    m.MateriaId,
                    m.Nombre,
                    m.ModuloId,
                    ModuloNombre = m.Modulo.Nombre,
                    AnioLectivoId = m.Modulo.AnioLectivoId,
                    AnioLectivoNombre = m.Modulo.AnioLectivo.Nombre,
                    m.Orden,
                    m.Activo
                })
                .FirstOrDefaultAsync();

            if (materia == null)
            {
                return NotFound();
            }

            return Ok(materia);
        }

        // POST: api/Materias/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] Materia model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Si no se especifica orden, asignar el siguiente disponible en el mÃƒÂ³dulo
            int orden = model.Orden;
            if (orden <= 0)
            {
                var maxOrden = await _context.Materias
                    .Where(m => m.ModuloId == model.ModuloId)
                    .MaxAsync(m => (int?)m.Orden) ?? 0;
                orden = maxOrden + 1;
            }

            var materia = new Materia
            {
                Nombre = model.Nombre,
                ModuloId = model.ModuloId,
                Orden = orden,
                Activo = true
            };

            _context.Materias.Add(materia);
            await _context.SaveChangesAsync();

            return Ok(materia);
        }

        // PUT: api/Materias/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] Materia model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var materia = await _context.Materias.FindAsync(model.MateriaId);

            if (materia == null)
            {
                return NotFound();
            }

            materia.Nombre = model.Nombre;
            materia.ModuloId = model.ModuloId;
            materia.Orden = model.Orden > 0 ? model.Orden : materia.Orden;
            await _context.SaveChangesAsync();

            return Ok(materia);
        }

        // PUT: api/Materias/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var materia = await _context.Materias.FindAsync(id);

            if (materia == null)
            {
                return NotFound();
            }

            materia.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Materias/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var materia = await _context.Materias.FindAsync(id);

            if (materia == null)
            {
                return NotFound();
            }

            materia.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
