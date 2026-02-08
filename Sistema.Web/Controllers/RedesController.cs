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
    public class RedesController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public RedesController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Redes/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var redes = await _context.Redes
                .OrderBy(r => r.Nombre)
                .ToListAsync();

            return Ok(redes);
        }

        // GET: api/Redes/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var redes = await _context.Redes
                .Where(r => r.Activo)
                .OrderBy(r => r.Nombre)
                .Select(r => new SelectViewModel
                {
                    Id = r.RedId,
                    Nombre = r.Nombre
                })
                .ToListAsync();

            return Ok(redes);
        }

        // GET: api/Redes/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var red = await _context.Redes.FindAsync(id);

            if (red == null)
            {
                return NotFound();
            }

            return Ok(red);
        }

        // POST: api/Redes/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] Red model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var red = new Red
            {
                Nombre = model.Nombre,
                Color = model.Color,
                Activo = true
            };

            _context.Redes.Add(red);
            await _context.SaveChangesAsync();

            return Ok(red);
        }

        // PUT: api/Redes/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] Red model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var red = await _context.Redes.FindAsync(model.RedId);

            if (red == null)
            {
                return NotFound();
            }

            red.Nombre = model.Nombre;
            red.Color = model.Color;
            await _context.SaveChangesAsync();

            return Ok(red);
        }

        // PUT: api/Redes/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var red = await _context.Redes.FindAsync(id);

            if (red == null)
            {
                return NotFound();
            }

            red.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Redes/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var red = await _context.Redes.FindAsync(id);

            if (red == null)
            {
                return NotFound();
            }

            red.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
