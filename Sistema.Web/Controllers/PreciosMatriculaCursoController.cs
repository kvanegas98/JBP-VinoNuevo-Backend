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
    public class PreciosMatriculaCursoController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public PreciosMatriculaCursoController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/PreciosMatriculaCurso/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var precios = await _context.PreciosMatriculaCurso
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .OrderBy(p => p.CategoriaEstudiante.Nombre)
                .ThenBy(p => p.CargoId)
                .Select(p => new
                {
                    p.PrecioMatriculaCursoId,
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

        // GET: api/PreciosMatriculaCurso/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var precio = await _context.PreciosMatriculaCurso
                .Include(p => p.CategoriaEstudiante)
                .Include(p => p.Cargo)
                .Where(p => p.PrecioMatriculaCursoId == id)
                .Select(p => new
                {
                    p.PrecioMatriculaCursoId,
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

        // GET: api/PreciosMatriculaCurso/ObtenerPrecio/{categoriaId}/{cargoId?}
        [HttpGet("[action]/{categoriaId}/{cargoId?}")]
        public async Task<IActionResult> ObtenerPrecio([FromRoute] int categoriaId, [FromRoute] int? cargoId = null)
        {
            var precio = await _context.PreciosMatriculaCurso
                .Include(p => p.Cargo)
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio específico para el cargo, buscar sin cargo
            if (precio == null && cargoId.HasValue)
            {
                precio = await _context.PreciosMatriculaCurso
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
                precio.PrecioMatriculaCursoId,
                precio.CategoriaEstudianteId,
                precio.CargoId,
                CargoNombre = precio.Cargo?.Nombre,
                precio.Precio
            });
        }

        // POST: api/PreciosMatriculaCurso/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] PrecioMatriculaCurso model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe un precio para esa categoría + cargo
            var existe = await _context.PreciosMatriculaCurso
                .AnyAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                            && p.CargoId == model.CargoId);

            if (existe)
            {
                return BadRequest(new { message = "Ya existe un precio para esta combinación de categoría y cargo" });
            }

            var precio = new PrecioMatriculaCurso
            {
                CategoriaEstudianteId = model.CategoriaEstudianteId,
                CargoId = model.CargoId,
                Precio = model.Precio,
                Activo = true
            };

            _context.PreciosMatriculaCurso.Add(precio);
            await _context.SaveChangesAsync();

            return Ok(precio);
        }

        // PUT: api/PreciosMatriculaCurso/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] PrecioMatriculaCurso model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var precio = await _context.PreciosMatriculaCurso.FindAsync(model.PrecioMatriculaCursoId);

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

        // DELETE: api/PreciosMatriculaCurso/Eliminar/{id}
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var precio = await _context.PreciosMatriculaCurso.FindAsync(id);

            if (precio == null)
            {
                return NotFound();
            }

            _context.PreciosMatriculaCurso.Remove(precio);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
