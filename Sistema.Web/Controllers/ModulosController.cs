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
    public class ModulosController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public ModulosController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Modulos/Listar?anioLectivoId=1
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar([FromQuery] int? anioLectivoId)
        {
            var query = _context.Modulos
                .Include(m => m.AnioLectivo)
                .AsQueryable();

            if (anioLectivoId.HasValue)
            {
                query = query.Where(m => m.AnioLectivoId == anioLectivoId.Value);
            }

            var modulos = await query
                .OrderBy(m => m.AnioLectivoId)
                .ThenBy(m => m.Numero)
                .Select(m => new
                {
                    m.ModuloId,
                    m.Numero,
                    m.Nombre,
                    m.AnioLectivoId,
                    AnioLectivoNombre = m.AnioLectivo.Nombre,
                    m.Activo
                })
                .ToListAsync();

            return Ok(modulos);
        }

        // GET: api/Modulos/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var modulos = await _context.Modulos
                .Include(m => m.AnioLectivo)
                .Where(m => m.Activo)
                .OrderBy(m => m.AnioLectivoId)
                .ThenBy(m => m.Numero)
                .Select(m => new SelectViewModel
                {
                    Id = m.ModuloId,
                    Nombre = $"{m.Nombre} ({m.AnioLectivo.Nombre})"
                })
                .ToListAsync();

            return Ok(modulos);
        }

        // GET: api/Modulos/SelectPorAnio/{anioLectivoId}
        [HttpGet("[action]/{anioLectivoId}")]
        public async Task<IActionResult> SelectPorAnio([FromRoute] int anioLectivoId)
        {
            var modulos = await _context.Modulos
                .Where(m => m.Activo && m.AnioLectivoId == anioLectivoId)
                .OrderBy(m => m.Numero)
                .Select(m => new SelectViewModel
                {
                    Id = m.ModuloId,
                    Nombre = m.Nombre
                })
                .ToListAsync();

            return Ok(modulos);
        }

        // GET: api/Modulos/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var modulo = await _context.Modulos
                .Include(m => m.AnioLectivo)
                .Where(m => m.ModuloId == id)
                .Select(m => new
                {
                    m.ModuloId,
                    m.Numero,
                    m.Nombre,
                    m.AnioLectivoId,
                    AnioLectivoNombre = m.AnioLectivo.Nombre,
                    m.Activo
                })
                .FirstOrDefaultAsync();

            if (modulo == null)
            {
                return NotFound();
            }

            return Ok(modulo);
        }

        // POST: api/Modulos/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] Modulo model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que no exista otro módulo con el mismo número en el mismo año
            var existe = await _context.Modulos
                .AnyAsync(m => m.AnioLectivoId == model.AnioLectivoId && m.Numero == model.Numero);

            if (existe)
            {
                return BadRequest("Ya existe un módulo con ese número en el año lectivo seleccionado.");
            }

            var modulo = new Modulo
            {
                AnioLectivoId = model.AnioLectivoId,
                Numero = model.Numero,
                Nombre = model.Nombre,
                Activo = true
            };

            _context.Modulos.Add(modulo);
            await _context.SaveChangesAsync();

            return Ok(modulo);
        }

        // PUT: api/Modulos/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] Modulo model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var modulo = await _context.Modulos.FindAsync(model.ModuloId);

            if (modulo == null)
            {
                return NotFound();
            }

            // Verificar que no exista otro módulo con el mismo número en el mismo año
            var existe = await _context.Modulos
                .AnyAsync(m => m.AnioLectivoId == model.AnioLectivoId
                    && m.Numero == model.Numero
                    && m.ModuloId != model.ModuloId);

            if (existe)
            {
                return BadRequest("Ya existe un módulo con ese número en el año lectivo seleccionado.");
            }

            modulo.AnioLectivoId = model.AnioLectivoId;
            modulo.Numero = model.Numero;
            modulo.Nombre = model.Nombre;
            await _context.SaveChangesAsync();

            return Ok(modulo);
        }

        // PUT: api/Modulos/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var modulo = await _context.Modulos.FindAsync(id);

            if (modulo == null)
            {
                return NotFound();
            }

            modulo.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Modulos/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var modulo = await _context.Modulos.FindAsync(id);

            if (modulo == null)
            {
                return NotFound();
            }

            modulo.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
