using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Usuarios;
using Sistema.Web.Models.Usuarios;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public UsuariosController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Usuarios/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] string buscar = null,
            [FromQuery] int? rolId = null,
            [FromQuery] bool? activo = null)
        {
            try
            {
                var query = _context.Usuarios
                    .Include(u => u.Rol)
                    .AsQueryable();

                // Filtro: BÃƒÂºsqueda por texto
                if (!string.IsNullOrEmpty(buscar))
                {
                    buscar = buscar.ToLower();
                    query = query.Where(u =>
                        u.Nombre.ToLower().Contains(buscar) ||
                        u.Email.ToLower().Contains(buscar));
                }

                // Filtro: Por rol
                if (rolId.HasValue)
                {
                    query = query.Where(u => u.RolId == rolId.Value);
                }

                // Filtro: Activo/Inactivo
                if (activo.HasValue)
                {
                    query = query.Where(u => u.Activo == activo.Value);
                }

                var totalRegistros = await query.CountAsync();

                var usuarios = await query
                    .OrderBy(u => u.Nombre)
                    .Skip((pagina - 1) * porPagina)
                    .Take(porPagina)
                    .Select(u => new UsuarioViewModel
                    {
                        UsuarioId = u.UsuarioId,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        RolId = u.RolId,
                        RolNombre = u.Rol.Nombre,
                        Activo = u.Activo
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalRegistros = totalRegistros,
                    pagina = pagina,
                    porPagina = porPagina,
                    totalPaginas = (int)Math.Ceiling((double)totalRegistros / porPagina),
                    datos = usuarios
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al listar usuarios",
                    error = ex.Message
                });
            }
        }

        // GET: api/Usuarios/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.UsuarioId == id)
                .Select(u => new UsuarioViewModel
                {
                    UsuarioId = u.UsuarioId,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    RolId = u.RolId,
                    RolNombre = u.Rol.Nombre,
                    Activo = u.Activo
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        // POST: api/Usuarios/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar que el email no exista
            var existe = await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (existe)
            {
                return BadRequest(new { message = "Ya existe un usuario con ese email" });
            }

            // Verificar que el rol existe
            var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == model.RolId && r.Activo);
            if (!rolExiste)
            {
                return BadRequest(new { message = "El rol seleccionado no existe o estÃƒÂ¡ inactivo" });
            }

            // Generar hash de la contraseÃƒÂ±a
            CrearPasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Email = model.Email,
                RolId = model.RolId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Activo = true
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                usuarioId = usuario.UsuarioId,
                message = "Usuario creado exitosamente"
            });
        }

        // PUT: api/Usuarios/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar que el email no exista (excepto el mismo usuario)
            var existe = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.UsuarioId != model.UsuarioId);
            if (existe)
            {
                return BadRequest(new { message = "Ya existe un usuario con ese email" });
            }

            // Verificar que el rol existe
            var rolExiste = await _context.Roles.AnyAsync(r => r.RolId == model.RolId && r.Activo);
            if (!rolExiste)
            {
                return BadRequest(new { message = "El rol seleccionado no existe o estÃƒÂ¡ inactivo" });
            }

            usuario.Nombre = model.Nombre;
            usuario.Email = model.Email;
            usuario.RolId = model.RolId;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario actualizado exitosamente" });
        }

        // PUT: api/Usuarios/CambiarPassword
        [HttpPut("[action]")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar contraseÃƒÂ±a actual
            if (!VerificarPasswordHash(model.PasswordActual, usuario.PasswordHash, usuario.PasswordSalt))
            {
                return BadRequest(new { message = "La contraseÃƒÂ±a actual es incorrecta" });
            }

            // Generar hash de la nueva contraseÃƒÂ±a
            CrearPasswordHash(model.PasswordNueva, out byte[] passwordHash, out byte[] passwordSalt);

            usuario.PasswordHash = passwordHash;
            usuario.PasswordSalt = passwordSalt;
            await _context.SaveChangesAsync();

            return Ok(new { message = "ContraseÃƒÂ±a cambiada exitosamente" });
        }

        // PUT: api/Usuarios/ResetPassword
        // Para que un administrador pueda resetear la contraseÃƒÂ±a de un usuario
        [HttpPut("[action]")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            // Generar hash de la nueva contraseÃƒÂ±a
            CrearPasswordHash(model.PasswordNueva, out byte[] passwordHash, out byte[] passwordSalt);

            usuario.PasswordHash = passwordHash;
            usuario.PasswordSalt = passwordSalt;
            await _context.SaveChangesAsync();

            return Ok(new { message = "ContraseÃƒÂ±a reseteada exitosamente" });
        }

        // PUT: api/Usuarios/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario activado exitosamente" });
        }

        // PUT: api/Usuarios/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario desactivado exitosamente" });
        }

        // MÃƒÂ©todos auxiliares para manejo de contraseÃƒÂ±as
        private void CrearPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerificarPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
