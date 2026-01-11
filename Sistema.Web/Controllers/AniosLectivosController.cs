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
    public class AniosLectivosController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public AniosLectivosController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/AniosLectivos/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var anios = await _context.AniosLectivos
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return Ok(anios);
        }

        // GET: api/AniosLectivos/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var anios = await _context.AniosLectivos
                .Where(a => a.Activo)
                .OrderBy(a => a.Nombre)
                .Select(a => new SelectViewModel
                {
                    Id = a.AnioLectivoId,
                    Nombre = a.Nombre
                })
                .ToListAsync();

            return Ok(anios);
        }

        // GET: api/AniosLectivos/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var anio = await _context.AniosLectivos.FindAsync(id);

            if (anio == null)
            {
                return NotFound();
            }

            return Ok(anio);
        }

        // POST: api/AniosLectivos/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] AnioLectivo model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var anio = new AnioLectivo
            {
                Nombre = model.Nombre,
                Activo = true
            };

            _context.AniosLectivos.Add(anio);
            await _context.SaveChangesAsync();

            return Ok(anio);
        }

        // PUT: api/AniosLectivos/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] AnioLectivo model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var anio = await _context.AniosLectivos.FindAsync(model.AnioLectivoId);

            if (anio == null)
            {
                return NotFound();
            }

            anio.Nombre = model.Nombre;
            await _context.SaveChangesAsync();

            return Ok(anio);
        }

        // PUT: api/AniosLectivos/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var anio = await _context.AniosLectivos.FindAsync(id);

            if (anio == null)
            {
                return NotFound();
            }

            anio.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/AniosLectivos/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var anio = await _context.AniosLectivos.FindAsync(id);

            if (anio == null)
            {
                return NotFound();
            }

            anio.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
