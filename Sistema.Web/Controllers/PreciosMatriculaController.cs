using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Catalogos;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreciosMatriculaController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public PreciosMatriculaController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/PreciosMatricula/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var precios = await _context.PreciosMatricula
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .OrderBy(p => p.CategoriaEstudiante.Nombre)
                .ThenBy(p => p.CargoId)
                .Select(p => new
                {
                    p.PrecioMatriculaId,
                    p.CategoriaEstudianteId,
                    CategoriaEstudianteNombre = p.CategoriaEstudiante.Nombre,
                    p.CargoId,
                    CargoNombre = p.Cargo != null ? p.Cargo.Nombre : "Sin cargo (Externo)",
                    p.Precio,
                    p.Activo
                })
                .ToListAsync();

            return Ok(precios);
        }

        // GET: api/PreciosMatricula/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var precio = await _context.PreciosMatricula
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .Where(p => p.PrecioMatriculaId == id)
                .Select(p => new
                {
                    p.PrecioMatriculaId,
                    p.CategoriaEstudianteId,
                    CategoriaEstudianteNombre = p.CategoriaEstudiante.Nombre,
                    p.CargoId,
                    CargoNombre = p.Cargo != null ? p.Cargo.Nombre : "Sin cargo (Externo)",
                    p.Precio,
                    p.Activo
                })
                .FirstOrDefaultAsync();

            if (precio == null)
            {
                return NotFound();
            }

            return Ok(precio);
        }

        // GET: api/PreciosMatricula/ObtenerPrecio/{categoriaId}/{cargoId?}
        [HttpGet("[action]/{categoriaId}/{cargoId?}")]
        public async Task<IActionResult> ObtenerPrecio([FromRoute] int categoriaId, [FromRoute] int? cargoId = null)
        {
            var precio = await _context.PreciosMatricula
                .Include(p => p.Cargo)
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio específico para el cargo, buscar sin cargo
            if (precio == null && cargoId.HasValue)
            {
                precio = await _context.PreciosMatricula
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            if (precio == null)
            {
                return NotFound(new { message = "Precio no encontrado para la categoría especificada" });
            }

            return Ok(new
            {
                precio.PrecioMatriculaId,
                precio.CategoriaEstudianteId,
                precio.CargoId,
                CargoNombre = precio.Cargo?.Nombre,
                precio.Precio
            });
        }

        // POST: api/PreciosMatricula/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] PrecioMatricula model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe un precio para esa categoría + cargo
            var existe = await _context.PreciosMatricula
                .AnyAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                            && p.CargoId == model.CargoId);

            if (existe)
            {
                return BadRequest(new { message = "Ya existe un precio para esta combinación de categoría y cargo" });
            }

            var precio = new PrecioMatricula
            {
                CategoriaEstudianteId = model.CategoriaEstudianteId,
                CargoId = model.CargoId,
                Precio = model.Precio,
                Activo = true
            };

            _context.PreciosMatricula.Add(precio);
            await _context.SaveChangesAsync();

            return Ok(precio);
        }

        // PUT: api/PreciosMatricula/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] PrecioMatricula model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var precio = await _context.PreciosMatricula.FindAsync(model.PrecioMatriculaId);

            if (precio == null)
            {
                return NotFound();
            }

            precio.CategoriaEstudianteId = model.CategoriaEstudianteId;
            precio.CargoId = model.CargoId;
            precio.Precio = model.Precio;
            await _context.SaveChangesAsync();

            return Ok(precio);
        }

        // DELETE: api/PreciosMatricula/Eliminar/{id}
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var precio = await _context.PreciosMatricula.FindAsync(id);

            if (precio == null)
            {
                return NotFound();
            }

            _context.PreciosMatricula.Remove(precio);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
