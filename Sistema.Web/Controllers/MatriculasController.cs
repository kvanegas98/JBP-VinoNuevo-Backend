using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Instituto;
using Sistema.Web.Models.Matriculas;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatriculasController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public MatriculasController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Matriculas/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var matriculas = await _context.Matriculas
                .Include(m => m.Estudiante)
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .OrderByDescending(m => m.FechaMatricula)
                .Select(m => new MatriculaViewModel
                {
                    MatriculaId = m.MatriculaId,
                    Codigo = m.Codigo,
                    EstudianteId = m.EstudianteId,
                    EstudianteCodigo = m.Estudiante.Codigo,
                    EstudianteNombre = m.Estudiante.NombreCompleto,
                    ModuloId = m.ModuloId,
                    ModuloNombre = m.Modulo.Nombre,
                    AnioLectivoId = m.Modulo.AnioLectivoId,
                    AnioLectivoNombre = m.Modulo.AnioLectivo.Nombre,
                    ModalidadId = m.ModalidadId,
                    ModalidadNombre = m.Modalidad.Nombre,
                    CategoriaEstudianteId = m.CategoriaEstudianteId,
                    CategoriaEstudianteNombre = m.CategoriaEstudiante.Nombre,
                    FechaMatricula = m.FechaMatricula,
                    MontoMatricula = m.MontoMatricula,
                    DescuentoAplicado = m.DescuentoAplicado,
                    MontoFinal = m.MontoFinal,
                    Estado = m.Estado
                })
                .ToListAsync();

            return Ok(matriculas);
        }

        // POST: api/Matriculas/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearMatriculaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
            // Verificar que el estudiante existe
            var estudiante = await _context.Estudiantes
                .Include(e => e.EstudianteCargos)
                    .ThenInclude(ec => ec.Cargo)
                .FirstOrDefaultAsync(e => e.EstudianteId == model.EstudianteId);

            if (estudiante == null)
            {
                return BadRequest(new { message = "Estudiante no encontrado" });
            }

            // Verificar que no exista una matrícula activa o pendiente del mismo estudiante en el mismo módulo
            var matriculaExistente = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.EstudianteId == model.EstudianteId
                                       && m.ModuloId == model.ModuloId
                                       && m.Estado != "Anulada");

            if (matriculaExistente != null)
            {
                return BadRequest(new {
                    message = $"El estudiante ya tiene una matrícula {matriculaExistente.Estado.ToLower()} en este módulo",
                    matriculaId = matriculaExistente.MatriculaId,
                    codigo = matriculaExistente.Codigo,
                    estado = matriculaExistente.Estado
                });
            }

            // Obtener el cargo del estudiante (si es interno)
            int? cargoId = null;
            string cargoNombre = null;

            if (estudiante.EsInterno && estudiante.EstudianteCargos != null && estudiante.EstudianteCargos.Any(ec => ec.Cargo != null))
            {
                var cargo = estudiante.EstudianteCargos.First(ec => ec.Cargo != null);
                cargoId = cargo.CargoId;
                cargoNombre = cargo.Cargo.Nombre;
            }

            // Obtener precio de matrícula según categoría + cargo
            var precioMatricula = await _context.PreciosMatricula
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio específico para el cargo, buscar sin cargo (precio base)
            if (precioMatricula == null && cargoId.HasValue)
            {
                precioMatricula = await _context.PreciosMatricula
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == model.CategoriaEstudianteId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            decimal montoMatricula = precioMatricula?.Precio ?? model.MontoMatricula;
            decimal descuento = 0;
            string tipoDescuento = cargoNombre ?? "Sin cargo";

            // Aplicar beca si el estudiante está becado
            if (estudiante.EsBecado && estudiante.PorcentajeBeca > 0)
            {
                descuento = montoMatricula * (estudiante.PorcentajeBeca / 100);
                tipoDescuento = $"Beca {estudiante.PorcentajeBeca}%";
            }

            // Generar código automático MAT-2025-0001, MAT-2025-0002, etc.
            var anioActual = DateTime.Now.Year;
            var prefijoAnio = $"MAT-{anioActual}-";
            var ultimoCodigo = await _context.Matriculas
                .Where(m => m.Codigo != null && m.Codigo.StartsWith(prefijoAnio))
                .OrderByDescending(m => m.Codigo)
                .Select(m => m.Codigo)
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

            decimal montoFinal = montoMatricula - descuento;

            // Si el monto es $0 (becado 100%), activar automáticamente sin necesidad de pago
            string estadoInicial = montoFinal == 0 ? "Activa" : "Pendiente";

            // Si está becado 100% y no hay observaciones, agregar observación automática
            string observaciones = model.Observaciones;
            if (montoFinal == 0 && string.IsNullOrEmpty(observaciones))
            {
                observaciones = "Becado 100%";
            }

            var matricula = new Matricula
            {
                Codigo = $"{prefijoAnio}{siguienteNumero:D4}",
                EstudianteId = model.EstudianteId,
                ModuloId = model.ModuloId,
                ModalidadId = model.ModalidadId,
                CategoriaEstudianteId = model.CategoriaEstudianteId,
                FechaMatricula = DateTime.Now,
                MontoMatricula = montoMatricula,
                DescuentoAplicado = descuento,
                MontoFinal = montoFinal,
                Estado = estadoInicial,
                Observaciones = observaciones
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            return Ok(new {
                matriculaId = matricula.MatriculaId,
                codigo = matricula.Codigo,
                montoMatricula = matricula.MontoMatricula,
                descuentoAplicado = matricula.DescuentoAplicado,
                montoFinal = matricula.MontoFinal,
                tipoDescuento = tipoDescuento,
                estado = matricula.Estado,
                activadaAutomaticamente = montoFinal == 0
            });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {
                    message = "Error al crear matrícula",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/Matriculas/CalcularPrecio/{estudianteId}/{categoriaId}
        [HttpGet("[action]/{estudianteId}/{categoriaId}")]
        public async Task<IActionResult> CalcularPrecio([FromRoute] int estudianteId, [FromRoute] int categoriaId)
        {
            var estudiante = await _context.Estudiantes
                .Include(e => e.EstudianteCargos)
                    .ThenInclude(ec => ec.Cargo)
                .FirstOrDefaultAsync(e => e.EstudianteId == estudianteId);

            if (estudiante == null)
            {
                return NotFound(new { message = "Estudiante no encontrado" });
            }

            // Obtener el cargo del estudiante (si es interno)
            int? cargoId = null;
            string cargoNombre = null;

            if (estudiante.EsInterno && estudiante.EstudianteCargos != null && estudiante.EstudianteCargos.Any(ec => ec.Cargo != null))
            {
                var cargo = estudiante.EstudianteCargos.First(ec => ec.Cargo != null);
                cargoId = cargo.CargoId;
                cargoNombre = cargo.Cargo.Nombre;
            }

            // Obtener precio de matrícula según categoría + cargo
            var precioMatricula = await _context.PreciosMatricula
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio específico para el cargo, buscar sin cargo
            if (precioMatricula == null && cargoId.HasValue)
            {
                precioMatricula = await _context.PreciosMatricula
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            // Obtener precio de mensualidad según categoría + cargo
            var precioMensualidad = await _context.PreciosMensualidad
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio específico para el cargo, buscar sin cargo
            if (precioMensualidad == null && cargoId.HasValue)
            {
                precioMensualidad = await _context.PreciosMensualidad
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == categoriaId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            decimal montoMatricula = precioMatricula?.Precio ?? 0;
            decimal montoMensualidad = precioMensualidad?.Precio ?? 0;

            // Calcular descuento por beca
            decimal descuentoMatricula = 0;
            decimal descuentoMensualidad = 0;
            if (estudiante.EsBecado && estudiante.PorcentajeBeca > 0)
            {
                descuentoMatricula = montoMatricula * (estudiante.PorcentajeBeca / 100);
                descuentoMensualidad = montoMensualidad * (estudiante.PorcentajeBeca / 100);
            }

            return Ok(new
            {
                estudianteId = estudiante.EstudianteId,
                estudianteNombre = estudiante.NombreCompleto,
                esInterno = estudiante.EsInterno,
                cargoNombre = cargoNombre,
                esBecado = estudiante.EsBecado,
                porcentajeBeca = estudiante.PorcentajeBeca,
                matricula = new
                {
                    precio = montoMatricula,
                    descuento = descuentoMatricula,
                    precioFinal = montoMatricula - descuentoMatricula
                },
                mensualidad = new
                {
                    precio = montoMensualidad,
                    descuento = descuentoMensualidad,
                    precioFinal = montoMensualidad - descuentoMensualidad
                }
            });
        }

        // PUT: api/Matriculas/Activar/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Activar([FromRoute] int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Estudiante)
                .FirstOrDefaultAsync(m => m.MatriculaId == id);

            if (matricula == null)
            {
                return NotFound(new { message = "Matrícula no encontrada" });
            }

            if (matricula.Estado == "Anulada")
            {
                return BadRequest(new { message = "No se puede activar una matrícula anulada" });
            }

            if (matricula.Estado == "Activa")
            {
                return BadRequest(new { message = "La matrícula ya está activa" });
            }

            matricula.Estado = "Activa";
            await _context.SaveChangesAsync();

            return Ok(new {
                matriculaId = matricula.MatriculaId,
                codigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                estado = matricula.Estado,
                message = "Matrícula activada exitosamente"
            });
        }

        // PUT: api/Matriculas/Anular/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Anular([FromRoute] int id, [FromBody] AnularMatriculaViewModel model)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Estudiante)
                .FirstOrDefaultAsync(m => m.MatriculaId == id);

            if (matricula == null)
            {
                return NotFound(new { message = "Matrícula no encontrada" });
            }

            if (matricula.Estado == "Anulada")
            {
                return BadRequest(new { message = "La matrícula ya está anulada" });
            }

            matricula.Estado = "Anulada";
            await _context.SaveChangesAsync();

            return Ok(new {
                matriculaId = matricula.MatriculaId,
                codigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                estado = matricula.Estado,
                motivo = model?.Motivo,
                message = "Matrícula anulada exitosamente"
            });
        }

        // GET: api/Matriculas/ListarPorEstado/{estado}
        [HttpGet("[action]/{estado}")]
        public async Task<IActionResult> ListarPorEstado([FromRoute] string estado)
        {
            var matriculas = await _context.Matriculas
                .Include(m => m.Estudiante)
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .Where(m => m.Estado == estado)
                .OrderByDescending(m => m.FechaMatricula)
                .Select(m => new MatriculaViewModel
                {
                    MatriculaId = m.MatriculaId,
                    Codigo = m.Codigo,
                    EstudianteId = m.EstudianteId,
                    EstudianteCodigo = m.Estudiante.Codigo,
                    EstudianteNombre = m.Estudiante.NombreCompleto,
                    ModuloId = m.ModuloId,
                    ModuloNombre = m.Modulo.Nombre,
                    AnioLectivoId = m.Modulo.AnioLectivoId,
                    AnioLectivoNombre = m.Modulo.AnioLectivo.Nombre,
                    ModalidadId = m.ModalidadId,
                    ModalidadNombre = m.Modalidad.Nombre,
                    CategoriaEstudianteId = m.CategoriaEstudianteId,
                    CategoriaEstudianteNombre = m.CategoriaEstudiante.Nombre,
                    FechaMatricula = m.FechaMatricula,
                    MontoMatricula = m.MontoMatricula,
                    DescuentoAplicado = m.DescuentoAplicado,
                    MontoFinal = m.MontoFinal,
                    Estado = m.Estado
                })
                .ToListAsync();

            return Ok(matriculas);
        }
    }
}
