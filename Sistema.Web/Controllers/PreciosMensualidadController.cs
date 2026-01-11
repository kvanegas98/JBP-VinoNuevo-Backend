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
    public class PreciosMensualidadController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public PreciosMensualidadController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/PreciosMensualidad/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var precios = await _context.PreciosMensualidad
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .OrderBy(p => p.CategoriaEstudiante.Nombre)
                .ThenBy(p => p.CargoId)
                .Select(p => new
                {
                    p.PrecioMensualidadId,
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

        // GET: api/PreciosMensualidad/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var precio = await _context.PreciosMensualidad
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .Where(p => p.PrecioMensualidadId == id)
                .Select(p => new
                {
                    p.PrecioMensualidadId,
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

        // GET: api/PreciosMensualidad/ObtenerPrecio/{categoriaId}/{cargoId?}
        [HttpGet("[action]/{categoriaId}/{cargoId?}")]
        public async Task<IActionResult> ObtenerPrecio([FromRoute] int categoriaId, [FromRoute] int? cargoId = null)
        {
            var precio = await _context.PreciosMensualidad
                .Include(p => p.Cargo)
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio específico para el cargo, buscar sin cargo
            if (precio == null && cargoId.HasValue)
            {
                precio = await _context.PreciosMensualidad
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
                precio.PrecioMensualidadId,
                precio.CategoriaEstudianteId,
                precio.CargoId,
                CargoNombre = precio.Cargo?.Nombre,
                precio.Precio
            });
        }

        // POST: api/PreciosMensualidad/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] PrecioMensualidad model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe un precio para esa categoría + cargo
            var existe = await _context.PreciosMensualidad
                .AnyAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                            && p.CargoId == model.CargoId);

            if (existe)
            {
                return BadRequest(new { message = "Ya existe un precio para esta combinación de categoría y cargo" });
            }

            var precio = new PrecioMensualidad
            {
                CategoriaEstudianteId = model.CategoriaEstudianteId,
                CargoId = model.CargoId,
                Precio = model.Precio,
                Activo = true
            };

            _context.PreciosMensualidad.Add(precio);
            await _context.SaveChangesAsync();

            return Ok(precio);
        }

        // PUT: api/PreciosMensualidad/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] PrecioMensualidad model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var precio = await _context.PreciosMensualidad.FindAsync(model.PrecioMensualidadId);

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

        // DELETE: api/PreciosMensualidad/Eliminar/{id}
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var precio = await _context.PreciosMensualidad.FindAsync(id);

            if (precio == null)
            {
                return NotFound();
            }

            _context.PreciosMensualidad.Remove(precio);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
