using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Catalogos;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PreciosMensualidadCursoController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public PreciosMensualidadCursoController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/PreciosMensualidadCurso/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var precios = await _context.PreciosMensualidadCurso
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .OrderBy(p => p.CategoriaEstudiante.Nombre)
                .ThenBy(p => p.CargoId)
                .Select(p => new
                {
                    p.PrecioMensualidadCursoId,
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

        // GET: api/PreciosMensualidadCurso/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var precio = await _context.PreciosMensualidadCurso
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .Where(p => p.PrecioMensualidadCursoId == id)
                .Select(p => new
                {
                    p.PrecioMensualidadCursoId,
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

        // GET: api/PreciosMensualidadCurso/ObtenerPrecio/{categoriaId}/{cargoId?}
        [HttpGet("[action]/{categoriaId}/{cargoId?}")]
        public async Task<IActionResult> ObtenerPrecio([FromRoute] int categoriaId, [FromRoute] int? cargoId = null)
        {
            var precio = await _context.PreciosMensualidadCurso
                .Include(p => p.Cargo)
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio especÃƒÂ­fico para el cargo, buscar sin cargo
            if (precio == null && cargoId.HasValue)
            {
                precio = await _context.PreciosMensualidadCurso
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            if (precio == null)
            {
                return NotFound(new { message = "Precio no encontrado para la categorÃƒÂ­a especificada" });
            }

            return Ok(new
            {
                precio.PrecioMensualidadCursoId,
                precio.CategoriaEstudianteId,
                precio.CargoId,
                CargoNombre = precio.Cargo?.Nombre,
                precio.Precio
            });
        }

        // POST: api/PreciosMensualidadCurso/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] PrecioMensualidadCurso model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe un precio para esa categorÃƒÂ­a + cargo
            var existe = await _context.PreciosMensualidadCurso
                .AnyAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                            && p.CargoId == model.CargoId);

            if (existe)
            {
                return BadRequest(new { message = "Ya existe un precio para esta combinaciÃƒÂ³n de categorÃƒÂ­a y cargo" });
            }

            var precio = new PrecioMensualidadCurso
            {
                CategoriaEstudianteId = model.CategoriaEstudianteId,
                CargoId = model.CargoId,
                Precio = model.Precio,
                Activo = true
            };

            _context.PreciosMensualidadCurso.Add(precio);
            await _context.SaveChangesAsync();

            return Ok(precio);
        }

        // PUT: api/PreciosMensualidadCurso/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] PrecioMensualidadCurso model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var precio = await _context.PreciosMensualidadCurso.FindAsync(model.PrecioMensualidadCursoId);

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

        // DELETE: api/PreciosMensualidadCurso/Eliminar/{id}
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var precio = await _context.PreciosMensualidadCurso.FindAsync(id);

            if (precio == null)
            {
                return NotFound();
            }

            _context.PreciosMensualidadCurso.Remove(precio);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
