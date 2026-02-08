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
    public class TiposPagoController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public TiposPagoController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/TiposPago/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var tipos = await _context.TiposPago
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return Ok(tipos);
        }

        // GET: api/TiposPago/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var tipos = await _context.TiposPago
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .Select(t => new SelectViewModel
                {
                    Id = t.TipoPagoId,
                    Nombre = t.Nombre
                })
                .ToListAsync();

            return Ok(tipos);
        }

        // GET: api/TiposPago/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var tipo = await _context.TiposPago.FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            return Ok(tipo);
        }

        // POST: api/TiposPago/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] TipoPago model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipo = new TipoPago
            {
                Nombre = model.Nombre,
                Activo = true
            };

            _context.TiposPago.Add(tipo);
            await _context.SaveChangesAsync();

            return Ok(tipo);
        }

        // PUT: api/TiposPago/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] TipoPago model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipo = await _context.TiposPago.FindAsync(model.TipoPagoId);

            if (tipo == null)
            {
                return NotFound();
            }

            tipo.Nombre = model.Nombre;
            await _context.SaveChangesAsync();

            return Ok(tipo);
        }

        // PUT: api/TiposPago/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var tipo = await _context.TiposPago.FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            tipo.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/TiposPago/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var tipo = await _context.TiposPago.FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            tipo.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
