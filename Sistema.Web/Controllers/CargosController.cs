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
    public class CargosController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public CargosController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Cargos/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var cargos = await _context.Cargos
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return Ok(cargos);
        }

        // GET: api/Cargos/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var cargos = await _context.Cargos
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new CargoSelectViewModel
                {
                    CargoId = c.CargoId,
                    Nombre = c.Nombre,
                    PorcentajeDescuento = c.PorcentajeDescuento
                })
                .ToListAsync();

            return Ok(cargos);
        }

        // GET: api/Cargos/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var cargo = await _context.Cargos.FindAsync(id);

            if (cargo == null)
            {
                return NotFound();
            }

            return Ok(cargo);
        }

        // POST: api/Cargos/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] Cargo model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cargo = new Cargo
            {
                Nombre = model.Nombre,
                PorcentajeDescuento = model.PorcentajeDescuento,
                Activo = true
            };

            _context.Cargos.Add(cargo);
            await _context.SaveChangesAsync();

            return Ok(cargo);
        }

        // PUT: api/Cargos/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] Cargo model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cargo = await _context.Cargos.FindAsync(model.CargoId);

            if (cargo == null)
            {
                return NotFound();
            }

            cargo.Nombre = model.Nombre;
            cargo.PorcentajeDescuento = model.PorcentajeDescuento;
            await _context.SaveChangesAsync();

            return Ok(cargo);
        }

        // PUT: api/Cargos/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var cargo = await _context.Cargos.FindAsync(id);

            if (cargo == null)
            {
                return NotFound();
            }

            cargo.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Cargos/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var cargo = await _context.Cargos.FindAsync(id);

            if (cargo == null)
            {
                return NotFound();
            }

            cargo.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
