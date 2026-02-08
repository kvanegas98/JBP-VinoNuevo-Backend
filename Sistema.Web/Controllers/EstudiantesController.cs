using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Instituto;
using Sistema.Web.Models.Estudiantes;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EstudiantesController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public EstudiantesController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Estudiantes/Listar
        // Listar estudiantes con filtros avanzados y paginaciÃƒÂ³n
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] string buscar = null,
            [FromQuery] bool? esInterno = null,
            [FromQuery] bool? esBecado = null,
            [FromQuery] int? redId = null,
            [FromQuery] int? cargoId = null, // Filtro por un solo cargo (mantiene compatibilidad)
            [FromQuery] int[] cargosIds = null, // Filtro por mÃƒÂºltiples cargos
            [FromQuery] string tipoEstudiante = null, // "Presencial" o "En Linea"
            [FromQuery] bool? activo = null,
            [FromQuery] string ordenarPor = "nombre") // "nombre", "codigo", "ciudad"
        {
            try
            {
                var query = _context.Estudiantes
                    .Include(e => e.Red)
                    .Include(e => e.EstudianteCargos)
                        .ThenInclude(ec => ec.Cargo)
                    .AsQueryable();

                // Filtro: BÃƒÂºsqueda por texto (nombre, cÃƒÂ³digo, cÃƒÂ©dula, email)
                if (!string.IsNullOrEmpty(buscar))
                {
                    buscar = buscar.ToLower();
                    query = query.Where(e =>
                        e.NombreCompleto.ToLower().Contains(buscar) ||
                        e.Codigo.ToLower().Contains(buscar) ||
                        (e.Cedula != null && e.Cedula.Contains(buscar)) ||
                        (e.CorreoElectronico != null && e.CorreoElectronico.ToLower().Contains(buscar)));
                }

                // Filtro: Interno/Externo
                if (esInterno.HasValue)
                {
                    query = query.Where(e => e.EsInterno == esInterno.Value);
                }

                // Filtro: Becado/No becado
                if (esBecado.HasValue)
                {
                    query = query.Where(e => e.EsBecado == esBecado.Value);
                }

                // Filtro: Por red
                if (redId.HasValue)
                {
                    query = query.Where(e => e.RedId == redId.Value);
                }

                // Filtro: Por cargo(s)
                // Si se proporciona cargosIds (mÃƒÂºltiples), tiene prioridad sobre cargoId (uno solo)
                if (cargosIds != null && cargosIds.Length > 0)
                {
                    // Filtrar estudiantes que tengan AL MENOS UNO de los cargos especificados
                    query = query.Where(e => e.EstudianteCargos.Any(ec => cargosIds.Contains(ec.CargoId)));
                }
                else if (cargoId.HasValue)
                {
                    // Mantener compatibilidad con filtro por un solo cargo
                    query = query.Where(e => e.EstudianteCargos.Any(ec => ec.CargoId == cargoId.Value));
                }

                // Filtro: Tipo de estudiante (Presencial / En Linea)
                if (!string.IsNullOrEmpty(tipoEstudiante))
                {
                    query = query.Where(e => e.TipoEstudiante == tipoEstudiante);
                }

                // Filtro: Activo/Inactivo
                if (activo.HasValue)
                {
                    query = query.Where(e => e.Activo == activo.Value);
                }

                // Contar total ANTES de paginar
                var totalRegistros = await query.CountAsync();

                // Ordenamiento
                switch (ordenarPor.ToLower())
                {
                    case "codigo":
                        query = query.OrderBy(e => e.Codigo);
                        break;
                    case "ciudad":
                        query = query.OrderBy(e => e.Ciudad).ThenBy(e => e.NombreCompleto);
                        break;
                    default: // "nombre"
                        query = query.OrderBy(e => e.NombreCompleto);
                        break;
                }

                // PaginaciÃƒÂ³n
                var estudiantes = await query
                    .Skip((pagina - 1) * porPagina)
                    .Take(porPagina)
                    .Select(e => new EstudianteViewModel
                    {
                        EstudianteId = e.EstudianteId,
                        Codigo = e.Codigo,
                        NombreCompleto = e.NombreCompleto,
                        Cedula = e.Cedula,
                        CorreoElectronico = e.CorreoElectronico,
                        Celular = e.Celular,
                        Ciudad = e.Ciudad,
                        TipoEstudiante = e.TipoEstudiante,
                        EsInterno = e.EsInterno,
                        EsBecado = e.EsBecado,
                        PorcentajeBeca = e.PorcentajeBeca,
                        RedId = e.RedId,
                        RedNombre = e.Red != null ? e.Red.Nombre : null,
                        RedColor = e.Red != null ? e.Red.Color : null,
                        CargosIds = e.EstudianteCargos.Select(ec => ec.CargoId).ToList(),
                        Cargos = e.EstudianteCargos.Select(ec => new CargoViewModel
                        {
                            CargoId = ec.Cargo.CargoId,
                            Nombre = ec.Cargo.Nombre,
                            PorcentajeDescuento = ec.Cargo.PorcentajeDescuento
                        }).ToList(),
                        IglesiaOrigen = e.IglesiaOrigen,
                        PastorOrigen = e.PastorOrigen,
                        DireccionIglesiaOrigen = e.DireccionIglesiaOrigen,
                        TelefonoIglesiaOrigen = e.TelefonoIglesiaOrigen,
                        Activo = e.Activo
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalRegistros = totalRegistros,
                    pagina = pagina,
                    porPagina = porPagina,
                    totalPaginas = (int)System.Math.Ceiling((double)totalRegistros / porPagina),
                    datos = estudiantes
                });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al listar estudiantes",
                    error = ex.Message
                });
            }
        }

        // GET: api/Estudiantes/Buscar/{texto}
        [HttpGet("[action]/{texto}")]
        public async Task<IActionResult> Buscar([FromRoute] string texto)
        {
            var estudiantes = await _context.Estudiantes
                .Include(e => e.Red)
                .Include(e => e.EstudianteCargos)
                    .ThenInclude(ec => ec.Cargo)
                .Where(e => e.NombreCompleto.Contains(texto))
                .OrderBy(e => e.NombreCompleto)
                .Select(e => new EstudianteViewModel
                {
                    EstudianteId = e.EstudianteId,
                    Codigo = e.Codigo,
                    NombreCompleto = e.NombreCompleto,
                    Cedula = e.Cedula,
                    CorreoElectronico = e.CorreoElectronico,
                    Celular = e.Celular,
                    Ciudad = e.Ciudad,
                    TipoEstudiante = e.TipoEstudiante,
                    EsInterno = e.EsInterno,
                    EsBecado = e.EsBecado,
                    PorcentajeBeca = e.PorcentajeBeca,
                    RedId = e.RedId,
                    RedNombre = e.Red != null ? e.Red.Nombre : null,
                    RedColor = e.Red != null ? e.Red.Color : null,
                    CargosIds = e.EstudianteCargos.Select(ec => ec.CargoId).ToList(),
                    Cargos = e.EstudianteCargos.Select(ec => new CargoViewModel
                    {
                        CargoId = ec.Cargo.CargoId,
                        Nombre = ec.Cargo.Nombre,
                        PorcentajeDescuento = ec.Cargo.PorcentajeDescuento
                    }).ToList(),
                    IglesiaOrigen = e.IglesiaOrigen,
                    PastorOrigen = e.PastorOrigen,
                    DireccionIglesiaOrigen = e.DireccionIglesiaOrigen,
                    TelefonoIglesiaOrigen = e.TelefonoIglesiaOrigen,
                    Activo = e.Activo
                })
                .ToListAsync();

            return Ok(estudiantes);
        }

        // GET: api/Estudiantes/Obtener/{id}
        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Obtener([FromRoute] int id)
        {
            var estudiante = await _context.Estudiantes
                .Include(e => e.Red)
                .Include(e => e.EstudianteCargos)
                    .ThenInclude(ec => ec.Cargo)
                .Where(e => e.EstudianteId == id)
                .Select(e => new EstudianteViewModel
                {
                    EstudianteId = e.EstudianteId,
                    Codigo = e.Codigo,
                    NombreCompleto = e.NombreCompleto,
                    Cedula = e.Cedula,
                    CorreoElectronico = e.CorreoElectronico,
                    Celular = e.Celular,
                    Ciudad = e.Ciudad,
                    TipoEstudiante = e.TipoEstudiante,
                    EsInterno = e.EsInterno,
                    EsBecado = e.EsBecado,
                    PorcentajeBeca = e.PorcentajeBeca,
                    RedId = e.RedId,
                    RedNombre = e.Red != null ? e.Red.Nombre : null,
                    RedColor = e.Red != null ? e.Red.Color : null,
                    CargosIds = e.EstudianteCargos.Select(ec => ec.CargoId).ToList(),
                    Cargos = e.EstudianteCargos.Select(ec => new CargoViewModel
                    {
                        CargoId = ec.Cargo.CargoId,
                        Nombre = ec.Cargo.Nombre,
                        PorcentajeDescuento = ec.Cargo.PorcentajeDescuento
                    }).ToList(),
                    IglesiaOrigen = e.IglesiaOrigen,
                    PastorOrigen = e.PastorOrigen,
                    DireccionIglesiaOrigen = e.DireccionIglesiaOrigen,
                    TelefonoIglesiaOrigen = e.TelefonoIglesiaOrigen,
                    Activo = e.Activo
                })
                .FirstOrDefaultAsync();

            if (estudiante == null)
            {
                return NotFound();
            }

            return Ok(estudiante);
        }

        // POST: api/Estudiantes/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearEstudianteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Generar cÃƒÂ³digo automÃƒÂ¡tico AÃƒÂ±o-IVN-Consecutivo (ej: 2026-IVN-0001)
            var anioActual = DateTime.Now.Year;
            var prefijo = $"{anioActual}-IVN-";

            var ultimoCodigo = await _context.Estudiantes
                .Where(e => e.Codigo != null && e.Codigo.StartsWith(prefijo))
                .OrderByDescending(e => e.Codigo)
                .Select(e => e.Codigo)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (!string.IsNullOrEmpty(ultimoCodigo))
            {
                var partes = ultimoCodigo.Split('-');
                if (partes.Length == 3)
                {
                    int.TryParse(partes[2], out siguienteNumero);
                    siguienteNumero++;
                }
            }

            var estudiante = new Estudiante
            {
                Codigo = $"{prefijo}{siguienteNumero:D4}",
                NombreCompleto = model.NombreCompleto,
                Cedula = model.Cedula,
                CorreoElectronico = model.CorreoElectronico,
                Celular = model.Celular,
                Ciudad = model.Ciudad,
                TipoEstudiante = model.TipoEstudiante,
                EsInterno = model.EsInterno,
                EsBecado = model.EsBecado,
                PorcentajeBeca = model.EsBecado ? model.PorcentajeBeca : 0,
                RedId = model.EsInterno ? model.RedId : null,
                IglesiaOrigen = !model.EsInterno ? model.IglesiaOrigen : null,
                PastorOrigen = !model.EsInterno ? model.PastorOrigen : null,
                DireccionIglesiaOrigen = !model.EsInterno ? model.DireccionIglesiaOrigen : null,
                TelefonoIglesiaOrigen = !model.EsInterno ? model.TelefonoIglesiaOrigen : null,
                Activo = true
            };

            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();

            // Agregar cargos si es interno
            if (model.EsInterno && model.CargosIds != null && model.CargosIds.Any())
            {
                foreach (var cargoId in model.CargosIds)
                {
                    _context.EstudianteCargos.Add(new EstudianteCargo
                    {
                        EstudianteId = estudiante.EstudianteId,
                        CargoId = cargoId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { estudianteId = estudiante.EstudianteId, codigo = estudiante.Codigo });
        }

        // PUT: api/Estudiantes/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarEstudianteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estudiante = await _context.Estudiantes
                .Include(e => e.EstudianteCargos)
                .FirstOrDefaultAsync(e => e.EstudianteId == model.EstudianteId);

            if (estudiante == null)
            {
                return NotFound();
            }

            estudiante.NombreCompleto = model.NombreCompleto;
            estudiante.Cedula = model.Cedula;
            estudiante.CorreoElectronico = model.CorreoElectronico;
            estudiante.Celular = model.Celular;
            estudiante.Ciudad = model.Ciudad;
            estudiante.TipoEstudiante = model.TipoEstudiante;
            estudiante.EsInterno = model.EsInterno;
            estudiante.RedId = model.EsInterno ? model.RedId : null;
            estudiante.IglesiaOrigen = !model.EsInterno ? model.IglesiaOrigen : null;
            estudiante.PastorOrigen = !model.EsInterno ? model.PastorOrigen : null;
            estudiante.DireccionIglesiaOrigen = !model.EsInterno ? model.DireccionIglesiaOrigen : null;
            estudiante.TelefonoIglesiaOrigen = !model.EsInterno ? model.TelefonoIglesiaOrigen : null;

            // Actualizar cargos
            _context.EstudianteCargos.RemoveRange(estudiante.EstudianteCargos);

            if (model.EsInterno && model.CargosIds != null && model.CargosIds.Any())
            {
                foreach (var cargoId in model.CargosIds)
                {
                    _context.EstudianteCargos.Add(new EstudianteCargo
                    {
                        EstudianteId = estudiante.EstudianteId,
                        CargoId = cargoId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Estudiantes/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);

            if (estudiante == null)
            {
                return NotFound();
            }

            estudiante.Activo = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Estudiantes/Desactivar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Desactivar([FromRoute] int id)
        {
            var estudiante = await _context.Estudiantes.FindAsync(id);

            if (estudiante == null)
            {
                return NotFound();
            }

            estudiante.Activo = false;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
