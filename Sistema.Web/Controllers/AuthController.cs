using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sistema.Datos;
using Sistema.Web.Models.Auth;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbContextSistema _context;
        private readonly IConfiguration _configuration;

        public AuthController(DbContextSistema context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/Login
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Datos inválidos",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u => u.Email.ToLower() == model.Email.ToLower())
                    .FirstOrDefaultAsync();

                // Validar que el usuario existe
                if (usuario == null)
                {
                    return StatusCode(401, new { message = "Email o contraseña incorrectos" });
                }

                // Validar que el usuario está activo
                if (!usuario.Activo)
                {
                    return StatusCode(401, new { message = "Usuario inactivo. Contacte al administrador" });
                }

                // Validar que el rol está activo
                if (!usuario.Rol.Activo)
                {
                    return StatusCode(401, new { message = "Rol inactivo. Contacte al administrador" });
                }

                // Verificar la contraseña
                if (!VerificarPasswordHash(model.Password, usuario.PasswordHash, usuario.PasswordSalt))
                {
                    return StatusCode(401, new { message = "Email o contraseña incorrectos" });
                }

                // Generar token JWT
                var token = GenerarToken(usuario);

                // Retornar respuesta con información del usuario y token
                return Ok(new LoginResponseViewModel
                {
                    UsuarioId = usuario.UsuarioId,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    RolId = usuario.RolId,
                    RolNombre = usuario.Rol.Nombre,
                    Token = token,
                    Expiracion = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al procesar el login",
                    error = ex.Message
                });
            }
        }

        // POST: api/Auth/ValidarToken
        // Endpoint para validar si un token sigue siendo válido
        [HttpPost("[action]")]
        public async Task<IActionResult> ValidarToken([FromBody] dynamic model)
        {
            try
            {
                string token = model.token;

                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new { message = "Token no proporcionado" });
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = _configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = _configuration["Jwt:Issuer"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var usuarioId = int.Parse(jwtToken.Claims.First(x => x.Type == "usuarioId").Value);

                    // Verificar que el usuario sigue activo
                    var usuario = await _context.Usuarios
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

                    if (usuario == null || !usuario.Activo || !usuario.Rol.Activo)
                    {
                        return StatusCode(401, new { message = "Token inválido o usuario inactivo" });
                    }

                    return Ok(new
                    {
                        valido = true,
                        usuario = new
                        {
                            usuarioId = usuario.UsuarioId,
                            nombre = usuario.Nombre,
                            email = usuario.Email,
                            rolId = usuario.RolId,
                            rolNombre = usuario.Rol.Nombre
                        }
                    });
                }
                catch
                {
                    return StatusCode(401, new { message = "Token inválido o expirado" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al validar el token",
                    error = ex.Message
                });
            }
        }

        // Métodos auxiliares
        private string GenerarToken(Sistema.Entidades.Usuarios.Usuario usuario)
        {
            var claims = new[]
            {
                new Claim("usuarioId", usuario.UsuarioId.ToString()),
                new Claim("email", usuario.Email),
                new Claim("nombre", usuario.Nombre),
                new Claim("rolId", usuario.RolId.ToString()),
                new Claim("rolNombre", usuario.Rol.Nombre),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
