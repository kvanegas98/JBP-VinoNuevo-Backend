using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Usuarios;
using Sistema.Web.Models.Usuarios;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public RolesController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Roles/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var roles = await _context.Roles
                .Include(r => r.Usuarios)
                .OrderBy(r => r.Nombre)
                .Select(r => new RolViewModel
                {
                    RolId = r.RolId,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    Activo = r.Activo,
                    TotalUsuarios = r.Usuarios.Count(u => u.Activo)
                })
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/Roles/Select
        [HttpGet("[action]")]
        public async Task<IActionResult> Select()
        {
            var roles = await _context.Roles
                .Where(r => r.Activo)
                .OrderBy(r => r.Nombre)
                .Select(r => new
                {
                    id = r.RolId,
                    nombre = r.Nombre
                })
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/Roles/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var rol = await _context.Roles
                .Where(r => r.RolId == id)
                .Select(r => new RolViewModel
                {
                    RolId = r.RolId,
                    Nombre = r.Nombre,
                    Descripcion = r.Descripcion,
                    Activo = r.Activo,
                    TotalUsuarios = r.Usuarios.Count(u => u.Activo)
                })
                .FirstOrDefaultAsync();

            if (rol == null)
            {
                return NotFound();
            }

            return Ok(rol);
        }

        // POST: api/Roles/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que no exista otro rol con el mismo nombre
            var existe = await _context.Roles.AnyAsync(r => r.Nombre.ToLower() == model.Nombre.ToLower());
            if (existe)
            {
                return BadRequest(new { message = "Ya existe un rol con ese nombre" });
            }

            var rol = new Rol
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Activo = true
            };

            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();

            return Ok(new { rolId = rol.RolId, message = "Rol creado exitosamente" });
        }

        // PUT: api/Roles/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarRolViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var rol = await _context.Roles.FindAsync(model.RolId);
            if (rol == null)
            {
                return NotFound();
            }

            // Verificar que no exista otro rol con el mismo nombre
            var existe = await _context.Roles
                .AnyAsync(r => r.Nombre.ToLower() == model.Nombre.ToLower() && r.RolId != model.RolId);
            if (existe)
            {
                return BadRequest(new { message = "Ya existe un rol con ese nombre" });
            }

            rol.Nombre = model.Nombre;
            rol.Descripcion = model.Descripcion;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rol actualizado exitosamente" });
        }

        // PUT: api/Roles/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            rol.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rol activado exitosamente" });
        }

        // PUT: api/Roles/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null)
            {
                return NotFound();
            }

            // Verificar que no tenga usuarios activos asociados
            var tieneUsuariosActivos = await _context.Usuarios
                .AnyAsync(u => u.RolId == id && u.Activo);

            if (tieneUsuariosActivos)
            {
                return BadRequest(new { message = "No se puede desactivar un rol que tiene usuarios activos asociados" });
            }

            rol.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Rol desactivado exitosamente" });
        }
    }
}
