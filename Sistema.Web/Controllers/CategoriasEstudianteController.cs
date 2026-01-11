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
    public class CategoriasEstudianteController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public CategoriasEstudianteController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/CategoriasEstudiante/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var categorias = await _context.CategoriasEstudiante
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/CategoriasEstudiante/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var categorias = await _context.CategoriasEstudiante
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectViewModel
                {
                    Id = c.CategoriaEstudianteId,
                    Nombre = c.Nombre
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/CategoriasEstudiante/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var categoria = await _context.CategoriasEstudiante.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }

        // POST: api/CategoriasEstudiante/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CategoriaEstudiante model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoria = new CategoriaEstudiante
            {
                Nombre = model.Nombre,
                Activo = true
            };

            _context.CategoriasEstudiante.Add(categoria);
            await _context.SaveChangesAsync();

            return Ok(categoria);
        }

        // PUT: api/CategoriasEstudiante/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] CategoriaEstudiante model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoria = await _context.CategoriasEstudiante.FindAsync(model.CategoriaEstudianteId);

            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Nombre = model.Nombre;
            await _context.SaveChangesAsync();

            return Ok(categoria);
        }

        // PUT: api/CategoriasEstudiante/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var categoria = await _context.CategoriasEstudiante.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/CategoriasEstudiante/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var categoria = await _context.CategoriasEstudiante.FindAsync(id);

            if (categoria == null)
            {
                return NotFound();
            }

            categoria.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
