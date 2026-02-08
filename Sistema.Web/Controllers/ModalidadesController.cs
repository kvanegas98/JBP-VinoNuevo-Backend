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
    public class ModalidadesController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public ModalidadesController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Modalidades/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var modalidades = await _context.Modalidades
                .OrderBy(m => m.Nombre)
                .ToListAsync();

            return Ok(modalidades);
        }

        // GET: api/Modalidades/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var modalidades = await _context.Modalidades
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new SelectViewModel
                {
                    Id = m.ModalidadId,
                    Nombre = m.Nombre
                })
                .ToListAsync();

            return Ok(modalidades);
        }

        // GET: api/Modalidades/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var modalidad = await _context.Modalidades.FindAsync(id);

            if (modalidad == null)
            {
                return NotFound();
            }

            return Ok(modalidad);
        }

        // POST: api/Modalidades/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] Modalidad model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var modalidad = new Modalidad
            {
                Nombre = model.Nombre,
                Activo = true
            };

            _context.Modalidades.Add(modalidad);
            await _context.SaveChangesAsync();

            return Ok(modalidad);
        }

        // PUT: api/Modalidades/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] Modalidad model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var modalidad = await _context.Modalidades.FindAsync(model.ModalidadId);

            if (modalidad == null)
            {
                return NotFound();
            }

            modalidad.Nombre = model.Nombre;
            await _context.SaveChangesAsync();

            return Ok(modalidad);
        }

        // PUT: api/Modalidades/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var modalidad = await _context.Modalidades.FindAsync(id);

            if (modalidad == null)
            {
                return NotFound();
            }

            modalidad.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Modalidades/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var modalidad = await _context.Modalidades.FindAsync(id);

            if (modalidad == null)
            {
                return NotFound();
            }

            modalidad.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
