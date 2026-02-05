using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;

namespace Sistema.Web.Controllers
{
    /// <summary>
    /// Controller para gestionar tipos de evaluación y sus componentes
    /// Endpoints de solo lectura para configuración del sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TiposEvaluacionController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public TiposEvaluacionController(DbContextSistema context)
        {
            _context = context;
        }

        // ===================================================================
        // GET: api/TiposEvaluacion - Listar todos los tipos de evaluación
        // ===================================================================
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                var tipos = await _context.TiposEvaluacion
                    .Where(t => t.Activo)
                    .OrderBy(t => t.TipoEvaluacionId)
                    .Select(t => new
                    {
                        t.TipoEvaluacionId,
                        t.Codigo,
                        t.Nombre,
                        t.Descripcion,
                        t.CantidadComponentes,
                        t.Activo,
                        t.FechaCreacion
                    })
                    .ToListAsync();

                return Ok(tipos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tipos de evaluación", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/TiposEvaluacion/{id} - Obtener tipo de evaluación por ID
        // ===================================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var tipo = await _context.TiposEvaluacion
                    .Where(t => t.TipoEvaluacionId == id)
                    .Select(t => new
                    {
                        t.TipoEvaluacionId,
                        t.Codigo,
                        t.Nombre,
                        t.Descripcion,
                        t.CantidadComponentes,
                        t.Activo,
                        t.FechaCreacion
                    })
                    .FirstOrDefaultAsync();

                if (tipo == null)
                    return NotFound(new { message = "Tipo de evaluación no encontrado" });

                return Ok(tipo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener tipo de evaluación", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/TiposEvaluacion/{id}/Componentes - Obtener componentes de un tipo
        // ===================================================================
        [HttpGet("{id}/Componentes")]
        public async Task<IActionResult> ObtenerComponentes(int id)
        {
            try
            {
                // Verificar que el tipo exista
                var tipoExiste = await _context.TiposEvaluacion
                    .AnyAsync(t => t.TipoEvaluacionId == id);

                if (!tipoExiste)
                    return NotFound(new { message = "Tipo de evaluación no encontrado" });

                // Obtener componentes
                var componentes = await _context.ComponenteEvaluacion
                    .Where(c => c.TipoEvaluacionId == id && c.Activo)
                    .OrderBy(c => c.Orden)
                    .Select(c => new
                    {
                        c.ComponenteEvaluacionId,
                        c.TipoEvaluacionId,
                        c.Nombre,
                        c.PorcentajePeso,
                        c.Orden,
                        c.NotaMinima,
                        c.EsObligatorio,
                        c.Activo
                    })
                    .ToListAsync();

                // Validar que los pesos sumen 100%
                var totalPeso = componentes.Sum(c => c.PorcentajePeso);
                bool pesosValidos = totalPeso == 100;

                return Ok(new
                {
                    tipoEvaluacionId = id,
                    componentes,
                    totalPorcentaje = totalPeso,
                    pesosValidos,
                    mensaje = pesosValidos ? "Configuración válida" : "¡ADVERTENCIA! Los porcentajes no suman 100%"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener componentes", error = ex.Message });
            }
        }

        // ===================================================================
        // GET: api/TiposEvaluacion/Validar/{id} - Validar configuración de un tipo
        // ===================================================================
        [HttpGet("Validar/{id}")]
        public async Task<IActionResult> ValidarConfiguracion(int id)
        {
            try
            {
                var tipo = await _context.TiposEvaluacion
                    .FirstOrDefaultAsync(t => t.TipoEvaluacionId == id);

                if (tipo == null)
                    return NotFound(new { message = "Tipo de evaluación no encontrado" });

                var componentes = await _context.ComponenteEvaluacion
                    .Where(c => c.TipoEvaluacionId == id && c.Activo)
                    .ToListAsync();

                // Validaciones
                var errores = new System.Collections.Generic.List<string>();
                var advertencias = new System.Collections.Generic.List<string>();

                // 1. Validar que existan componentes
                if (!componentes.Any())
                {
                    errores.Add("No hay componentes de evaluación configurados");
                }

                // 2. Validar cantidad de componentes
                if (componentes.Count != tipo.CantidadComponentes)
                {
                    advertencias.Add($"La cantidad de componentes ({componentes.Count}) no coincide con la configurada ({tipo.CantidadComponentes})");
                }

                // 3. Validar que los pesos sumen 100%
                var totalPeso = componentes.Sum(c => c.PorcentajePeso);
                if (totalPeso != 100)
                {
                    errores.Add($"Los porcentajes no suman 100% (actual: {totalPeso}%)");
                }

                // 4. Validar que haya al menos un componente obligatorio
                if (!componentes.Any(c => c.EsObligatorio))
                {
                    advertencias.Add("No hay componentes obligatorios configurados");
                }

                // 5. Validar orden secuencial
                var ordenesEsperados = Enumerable.Range(1, componentes.Count).ToList();
                var ordenesActuales = componentes.Select(c => c.Orden).OrderBy(o => o).ToList();
                if (!ordenesEsperados.SequenceEqual(ordenesActuales))
                {
                    advertencias.Add("El orden de los componentes no es secuencial (1, 2, 3, ...)");
                }

                bool esValido = !errores.Any();

                return Ok(new
                {
                    tipoEvaluacionId = id,
                    tipoNombre = tipo.Nombre,
                    esValido,
                    errores,
                    advertencias,
                    resumen = new
                    {
                        cantidadComponentes = componentes.Count,
                        cantidadEsperada = tipo.CantidadComponentes,
                        totalPorcentaje = totalPeso,
                        componentesObligatorios = componentes.Count(c => c.EsObligatorio),
                        componentesOpcionales = componentes.Count(c => !c.EsObligatorio)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al validar configuración", error = ex.Message });
            }
        }
    }
}
