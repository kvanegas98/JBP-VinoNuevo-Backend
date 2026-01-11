using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Configuracion;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoCambioController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public TipoCambioController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/TipoCambio/Actual
        // Obtiene el tipo de cambio vigente
        [HttpGet("[action]")]
        public async Task<IActionResult> Actual()
        {
            var tipoCambio = await _context.TiposCambio
                .Include(t => t.Usuario)
                .Where(t => t.VigenteHasta == null)
                .Select(t => new
                {
                    t.TipoCambioId,
                    t.TasaCompra,
                    t.TasaVenta,
                    t.VigenteDesde,
                    t.UsuarioId,
                    UsuarioNombre = t.Usuario.Nombre,
                    t.FechaRegistro,
                    t.Observaciones
                })
                .FirstOrDefaultAsync();

            if (tipoCambio == null)
            {
                return NotFound(new { message = "No hay tipo de cambio configurado. Debe registrar uno." });
            }

            return Ok(tipoCambio);
        }

        // POST: api/TipoCambio/Registrar
        // Registra un nuevo tipo de cambio (cierra el anterior automáticamente)
        [HttpPost("[action]")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarTipoCambioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.TasaCompra <= 0 || model.TasaVenta <= 0)
            {
                return BadRequest(new { message = "Las tasas deben ser mayores a 0" });
            }

            if (model.TasaVenta < model.TasaCompra)
            {
                return BadRequest(new { message = "La tasa de venta no puede ser menor que la tasa de compra" });
            }

            // Verificar que el usuario existe
            var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
            if (usuario == null)
            {
                return BadRequest(new { message = "Usuario no encontrado" });
            }

            try
            {
                var ahora = DateTime.Now;

                // Cerrar el tipo de cambio vigente actual (si existe)
                var tipoCambioActual = await _context.TiposCambio
                    .FirstOrDefaultAsync(t => t.VigenteHasta == null);

                if (tipoCambioActual != null)
                {
                    tipoCambioActual.VigenteHasta = ahora;
                }

                // Crear el nuevo tipo de cambio
                var nuevoTipoCambio = new TipoCambio
                {
                    TasaCompra = model.TasaCompra,
                    TasaVenta = model.TasaVenta,
                    VigenteDesde = ahora,
                    VigenteHasta = null, // NULL = vigente
                    UsuarioId = model.UsuarioId,
                    FechaRegistro = ahora,
                    Observaciones = model.Observaciones
                };

                _context.TiposCambio.Add(nuevoTipoCambio);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    tipoCambioId = nuevoTipoCambio.TipoCambioId,
                    tasaCompra = nuevoTipoCambio.TasaCompra,
                    tasaVenta = nuevoTipoCambio.TasaVenta,
                    vigenteDesde = nuevoTipoCambio.VigenteDesde,
                    usuarioNombre = usuario.Nombre,
                    message = "Tipo de cambio registrado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al registrar el tipo de cambio",
                    error = ex.Message
                });
            }
        }

        // GET: api/TipoCambio/Historial
        // Lista el historial de tipos de cambio
        [HttpGet("[action]")]
        public async Task<IActionResult> Historial([FromQuery] int pagina = 1, [FromQuery] int porPagina = 20)
        {
            var query = _context.TiposCambio
                .Include(t => t.Usuario)
                .OrderByDescending(t => t.VigenteDesde);

            var total = await query.CountAsync();

            var historial = await query
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(t => new
                {
                    t.TipoCambioId,
                    t.TasaCompra,
                    t.TasaVenta,
                    t.VigenteDesde,
                    t.VigenteHasta,
                    EsVigente = t.VigenteHasta == null,
                    t.UsuarioId,
                    UsuarioNombre = t.Usuario.Nombre,
                    t.FechaRegistro,
                    t.Observaciones
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                pagina,
                porPagina,
                totalPaginas = (int)Math.Ceiling((double)total / porPagina),
                datos = historial
            });
        }
    }

    // ViewModel para registrar tipo de cambio
    public class RegistrarTipoCambioViewModel
    {
        public decimal TasaCompra { get; set; }
        public decimal TasaVenta { get; set; }
        public int UsuarioId { get; set; }
        public string Observaciones { get; set; }
    }
}
