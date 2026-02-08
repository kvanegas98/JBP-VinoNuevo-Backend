using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Instituto;
using Sistema.Web.Models.Pagos;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagosCursoController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public PagosCursoController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/PagosCurso/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar([FromQuery] int? matriculaCursoId)
        {
            var query = _context.PagosCurso
                .Include(p => p.MatriculaCurso)
                    .ThenInclude(m => m.Estudiante)
                .Include(p => p.MatriculaCurso)
                    .ThenInclude(m => m.CursoEspecializado)
                .AsQueryable();

            if (matriculaCursoId.HasValue)
            {
                query = query.Where(p => p.MatriculaCursoId == matriculaCursoId.Value);
            }

            var pagos = await query
                .OrderByDescending(p => p.FechaPago)
                .Select(p => new
                {
                    p.PagoCursoId,
                    p.Codigo,
                    p.MatriculaCursoId,
                    MatriculaCursoCodigo = p.MatriculaCurso.Codigo,
                    EstudianteId = p.MatriculaCurso.EstudianteId,
                    EstudianteCodigo = p.MatriculaCurso.Estudiante.Codigo,
                    EstudianteNombre = p.MatriculaCurso.Estudiante.NombreCompleto,
                    p.NumeroMensualidad,
                    CursoEspecializadoId = p.MatriculaCurso.CursoEspecializadoId,
                    CursoNombre = p.MatriculaCurso.CursoEspecializado.Nombre,
                    p.TipoPago,
                    p.Monto,
                    p.Descuento,
                    p.MontoFinal,
                    p.FechaPago,
                    p.EfectivoCordobas,
                    p.EfectivoDolares,
                    p.TarjetaCordobas,
                    p.TarjetaDolares,
                    p.TipoCambio,
                    p.TotalPagadoUSD,
                    p.MetodoPago,
                    p.NumeroComprobante,
                    p.Observaciones,
                    p.Estado
                })
                .ToListAsync();

            return Ok(pagos);
        }

        // GET: api/PagosCurso/MatriculasCursoPendientes/{estudianteId}
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> MatriculasCursoPendientes([FromRoute] int estudianteId)
        {
            var matriculas = await _context.MatriculasCurso
                .Include(m => m.CursoEspecializado)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .Where(m => m.EstudianteId == estudianteId && m.Estado == "Pendiente")
                .Select(m => new
                {
                    m.MatriculaCursoId,
                    m.Codigo,
                    m.CursoEspecializadoId,
                    CursoNombre = m.CursoEspecializado.Nombre,
                    ModalidadNombre = m.Modalidad.Nombre,
                    CategoriaNombre = m.CategoriaEstudiante.Nombre,
                    m.FechaMatricula,
                    m.MontoMatricula,
                    m.DescuentoAplicado,
                    m.MontoFinal,
                    m.Estado
                })
                .ToListAsync();

            return Ok(matriculas);
        }

        // GET: api/PagosCurso/MatriculasCursoActivas/{estudianteId}
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> MatriculasCursoActivas([FromRoute] int estudianteId)
        {
            var matriculas = await _context.MatriculasCurso
                .Include(m => m.CursoEspecializado)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .Where(m => m.EstudianteId == estudianteId && m.Estado == "Activa")
                .Select(m => new
                {
                    m.MatriculaCursoId,
                    m.Codigo,
                    m.CursoEspecializadoId,
                    CursoNombre = m.CursoEspecializado.Nombre,
                    ModalidadNombre = m.Modalidad.Nombre,
                    CategoriaNombre = m.CategoriaEstudiante.Nombre,
                    m.FechaMatricula,
                    m.MontoMatricula,
                    m.DescuentoAplicado,
                    m.MontoFinal,
                    m.Estado
                })
                .ToListAsync();

            return Ok(matriculas);
        }

        // GET: api/PagosCurso/MensualidadesParaPago/{matriculaCursoId}
        [HttpGet("[action]/{matriculaCursoId}")]
        public async Task<IActionResult> MensualidadesParaPago([FromRoute] int matriculaCursoId)
        {
            var matricula = await _context.MatriculasCurso
                .Include(m => m.Estudiante)
                    .ThenInclude(e => e.EstudianteCargos)
                        .ThenInclude(ec => ec.Cargo)
                .Include(m => m.CursoEspecializado)
                .Include(m => m.CategoriaEstudiante)
                .Include(m => m.Modalidad)
                .FirstOrDefaultAsync(m => m.MatriculaCursoId == matriculaCursoId);

            if (matricula == null)
            {
                return NotFound(new { message = "MatrÃƒÂ­cula de curso no encontrada" });
            }

            if (matricula.Estado != "Activa" && matricula.Estado != "Completada")
            {
                return BadRequest(new { message = "La matrÃƒÂ­cula debe estar activa o completada para consultar mensualidades" });
            }

            bool soloLectura = matricula.Estado == "Completada";

            // Calcular duraciÃƒÂ³n del curso en meses
            var fechaInicio = matricula.CursoEspecializado.FechaInicio;
            var fechaFin = matricula.CursoEspecializado.FechaFin;
            var totalMeses = ((fechaFin.Year - fechaInicio.Year) * 12) + fechaFin.Month - fechaInicio.Month + 1;

            // Obtener pagos de mensualidad existentes para esta matrÃƒÂ­cula
            var pagosExistentes = await _context.PagosCurso
                .Where(p => p.MatriculaCursoId == matriculaCursoId &&
                           p.TipoPago == "Mensualidad" &&
                           p.Estado == "Completado")
                .Select(p => p.NumeroMensualidad)
                .ToListAsync();

            // Obtener el cargo del estudiante (si es interno)
            int? cargoId = null;
            if (matricula.Estudiante.EsInterno && matricula.Estudiante.EstudianteCargos != null && matricula.Estudiante.EstudianteCargos.Any(ec => ec.Cargo != null))
            {
                var cargo = matricula.Estudiante.EstudianteCargos.First(ec => ec.Cargo != null);
                cargoId = cargo.CargoId;
            }

            // Obtener precio de mensualidad segÃƒÂºn categorÃƒÂ­a + cargo
            var precioMensualidad = await _context.PreciosMensualidadCurso
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio especÃƒÂ­fico para el cargo, buscar sin cargo
            if (precioMensualidad == null && cargoId.HasValue)
            {
                precioMensualidad = await _context.PreciosMensualidadCurso
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            decimal montoBase = precioMensualidad?.Precio ?? 0;
            decimal descuento = 0;
            decimal montoFinal = montoBase;

            // Verificar si es becado y aplicar descuento
            bool esBecado = matricula.Estudiante.EsBecado;
            decimal porcentajeBeca = matricula.Estudiante.PorcentajeBeca;

            if (esBecado && porcentajeBeca > 0)
            {
                descuento = montoBase * (porcentajeBeca / 100);
                montoFinal = montoBase - descuento;
            }

            bool esBecado100 = esBecado && porcentajeBeca >= 100;

            // Generar lista de mensualidades (1, 2, 3, ..., totalMeses)
            var mensualidades = Enumerable.Range(1, totalMeses).Select(numero => new
            {
                NumeroMensualidad = numero,
                Nombre = $"Mensualidad {numero}",
                Pagado = esBecado100 || pagosExistentes.Contains(numero),
                PagadoAutomaticamente = esBecado100,
                MontoBase = montoBase,
                Descuento = descuento,
                MontoFinal = montoFinal
            }).ToList();

            int mensualidadesPagadas = esBecado100 ? totalMeses : pagosExistentes.Count;
            int mensualidadesPendientes = esBecado100 ? 0 : totalMeses - pagosExistentes.Count;

            return Ok(new
            {
                matriculaCursoId = matricula.MatriculaCursoId,
                matriculaCursoCodigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                cursoNombre = matricula.CursoEspecializado.Nombre,
                categoriaNombre = matricula.CategoriaEstudiante.Nombre,
                esBecado = esBecado,
                porcentajeBeca = porcentajeBeca,
                esBecado100 = esBecado100,
                estadoMatricula = matricula.Estado,
                soloLectura = soloLectura,
                mensualidades = mensualidades,
                resumen = new
                {
                    totalMensualidades = totalMeses,
                    mensualidadesPagadas = mensualidadesPagadas,
                    mensualidadesPendientes = mensualidadesPendientes,
                    montoPendiente = mensualidadesPendientes * montoFinal
                }
            });
        }

        // POST: api/PagosCurso/PagarMatriculaCurso
        [HttpPost("[action]")]
        public async Task<IActionResult> PagarMatriculaCurso([FromBody] PagarMatriculaCursoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var matricula = await _context.MatriculasCurso
                    .Include(m => m.Estudiante)
                    .Include(m => m.CursoEspecializado)
                    .FirstOrDefaultAsync(m => m.MatriculaCursoId == model.MatriculaCursoId);

                if (matricula == null)
                {
                    return NotFound(new { message = "MatrÃƒÂ­cula de curso no encontrada" });
                }

                if (matricula.Estado == "Activa")
                {
                    return BadRequest(new { message = "La matrÃƒÂ­cula ya estÃƒÂ¡ pagada y activa" });
                }

                if (matricula.Estado == "Anulada")
                {
                    return BadRequest(new { message = "No se puede pagar una matrÃƒÂ­cula anulada" });
                }

                // Verificar que no exista pago de matrÃƒÂ­cula previo
                var pagoExistente = await _context.PagosCurso
                    .FirstOrDefaultAsync(p => p.MatriculaCursoId == model.MatriculaCursoId &&
                                             p.TipoPago == "Matricula" &&
                                             p.Estado == "Completado");

                if (pagoExistente != null)
                {
                    return BadRequest(new { message = "Ya existe un pago de matrÃƒÂ­cula para esta inscripciÃƒÂ³n" });
                }

                // Si el monto es $0 (becado 100%), no se requiere pago
                if (matricula.MontoFinal == 0)
                {
                    return BadRequest(new
                    {
                        message = "No se requiere pago para estudiantes becados al 100%. La matrÃƒÂ­cula ya fue activada automÃƒÂ¡ticamente.",
                        esBecado = true,
                        montoFinal = 0
                    });
                }

                // Validar detalle de pago
                if (model.DetallePago == null)
                {
                    return BadRequest(new { message = "Debe proporcionar el detalle del pago" });
                }

                if (model.DetallePago.TipoCambio <= 0)
                {
                    return BadRequest(new { message = "El tipo de cambio debe ser mayor a 0" });
                }

                // Calcular total pagado en USD
                var detalle = model.DetallePago;
                decimal totalCordobas = detalle.EfectivoCordobas + detalle.TarjetaCordobas;
                decimal totalDolares = detalle.EfectivoDolares + detalle.TarjetaDolares;
                decimal cordobasEnUSD = Math.Round(totalCordobas / detalle.TipoCambio, 2);
                decimal totalPagadoUSD = Math.Round(totalDolares + cordobasEnUSD, 2);

                // Validar que el monto pagado sea suficiente
                if (totalPagadoUSD < matricula.MontoFinal)
                {
                    return BadRequest(new
                    {
                        message = "El monto pagado es insuficiente",
                        montoRequerido = matricula.MontoFinal,
                        montoPagado = totalPagadoUSD,
                        diferencia = matricula.MontoFinal - totalPagadoUSD
                    });
                }

                // Calcular vuelto
                decimal vuelto = Math.Round(totalPagadoUSD - matricula.MontoFinal, 2);

                // Determinar mÃƒÂ©todo de pago
                string metodoPago = DeterminarMetodoPago(detalle);

                // Generar cÃƒÂ³digo de pago
                var anioActual = DateTime.Now.Year;
                var prefijo = $"PCURSO-{anioActual}-";
                var ultimoCodigo = await _context.PagosCurso
                    .Where(p => p.Codigo != null && p.Codigo.StartsWith(prefijo))
                    .OrderByDescending(p => p.Codigo)
                    .Select(p => p.Codigo)
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

                var pago = new PagoCurso
                {
                    Codigo = $"{prefijo}{siguienteNumero:D4}",
                    MatriculaCursoId = model.MatriculaCursoId,
                    NumeroMensualidad = null,
                    TipoPago = "Matricula",
                    Monto = matricula.MontoMatricula,
                    Descuento = matricula.DescuentoAplicado,
                    MontoFinal = matricula.MontoFinal,
                    FechaPago = DateTime.Now,
                    EfectivoCordobas = detalle.EfectivoCordobas,
                    EfectivoDolares = detalle.EfectivoDolares,
                    TarjetaCordobas = detalle.TarjetaCordobas,
                    TarjetaDolares = detalle.TarjetaDolares,
                    TipoCambio = detalle.TipoCambio,
                    TotalPagadoUSD = totalPagadoUSD,
                    Vuelto = vuelto,
                    VueltoCordobas = detalle.VueltoCordobas,
                    VueltoDolares = detalle.VueltoDolares,
                    MetodoPago = metodoPago,
                    NumeroComprobante = detalle.NumeroComprobante,
                    Observaciones = model.Observaciones,
                    Estado = "Completado"
                };

                _context.PagosCurso.Add(pago);

                // Activar la matrÃƒÂ­cula
                matricula.Estado = "Activa";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    pagoCursoId = pago.PagoCursoId,
                    codigo = pago.Codigo,
                    matriculaCursoId = matricula.MatriculaCursoId,
                    matriculaCursoCodigo = matricula.Codigo,
                    estudianteNombre = matricula.Estudiante.NombreCompleto,
                    cursoNombre = matricula.CursoEspecializado.Nombre,
                    montoFinal = pago.MontoFinal,
                    detallePago = new
                    {
                        efectivoCordobas = pago.EfectivoCordobas,
                        efectivoDolares = pago.EfectivoDolares,
                        tarjetaCordobas = pago.TarjetaCordobas,
                        tarjetaDolares = pago.TarjetaDolares,
                        tipoCambio = pago.TipoCambio,
                        totalPagadoUSD = pago.TotalPagadoUSD
                    },
                    vuelto = new
                    {
                        totalUSD = pago.Vuelto,
                        cordobas = pago.VueltoCordobas,
                        dolares = pago.VueltoDolares
                    },
                    metodoPago = pago.MetodoPago,
                    estadoMatricula = matricula.Estado,
                    message = pago.Vuelto > 0
                        ? $"Pago registrado. Vuelto: C${pago.VueltoCordobas:F2} + ${pago.VueltoDolares:F2}"
                        : "Pago de matrÃƒÂ­cula registrado y matrÃƒÂ­cula activada"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al procesar el pago",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // POST: api/PagosCurso/PagarMensualidadCurso
        [HttpPost("[action]")]
        public async Task<IActionResult> PagarMensualidadCurso([FromBody] PagarMensualidadCursoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var matricula = await _context.MatriculasCurso
                    .Include(m => m.Estudiante)
                        .ThenInclude(e => e.EstudianteCargos)
                            .ThenInclude(ec => ec.Cargo)
                    .Include(m => m.CursoEspecializado)
                    .Include(m => m.CategoriaEstudiante)
                    .FirstOrDefaultAsync(m => m.MatriculaCursoId == model.MatriculaCursoId);

                if (matricula == null)
                {
                    return NotFound(new { message = "MatrÃƒÂ­cula de curso no encontrada" });
                }

                if (matricula.Estado != "Activa" && matricula.Estado != "Completada")
                {
                    return BadRequest(new { message = "La matrÃƒÂ­cula debe estar activa para registrar pagos de mensualidad" });
                }

                // Validar que el nÃƒÂºmero de mensualidad sea vÃƒÂ¡lido
                if (model.NumeroMensualidad < 1)
                {
                    return BadRequest(new { message = "El nÃƒÂºmero de mensualidad debe ser mayor a 0" });
                }

                // Calcular el total de meses del curso
                var fechaInicio = matricula.CursoEspecializado.FechaInicio;
                var fechaFin = matricula.CursoEspecializado.FechaFin;
                var totalMeses = ((fechaFin.Year - fechaInicio.Year) * 12) + fechaFin.Month - fechaInicio.Month + 1;

                if (model.NumeroMensualidad > totalMeses)
                {
                    return BadRequest(new { message = $"El nÃƒÂºmero de mensualidad no puede ser mayor a {totalMeses}" });
                }

                // Verificar que no exista pago previo para este nÃƒÂºmero de mensualidad
                var pagoExistente = await _context.PagosCurso
                    .FirstOrDefaultAsync(p => p.MatriculaCursoId == model.MatriculaCursoId &&
                                             p.NumeroMensualidad == model.NumeroMensualidad &&
                                             p.TipoPago == "Mensualidad" &&
                                             p.Estado == "Completado");

                if (pagoExistente != null)
                {
                    return BadRequest(new { message = $"Ya existe un pago para la mensualidad {model.NumeroMensualidad}" });
                }

                // Obtener el cargo del estudiante (si es interno)
                int? cargoId = null;
                if (matricula.Estudiante.EsInterno && matricula.Estudiante.EstudianteCargos != null && matricula.Estudiante.EstudianteCargos.Any(ec => ec.Cargo != null))
                {
                    var cargo = matricula.Estudiante.EstudianteCargos.First(ec => ec.Cargo != null);
                    cargoId = cargo.CargoId;
                }

                // Obtener precio de mensualidad segÃƒÂºn categorÃƒÂ­a + cargo
                var precioMensualidad = await _context.PreciosMensualidadCurso
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                           && p.CargoId == cargoId
                                           && p.Activo);

                // Si no encuentra precio especÃƒÂ­fico para el cargo, buscar sin cargo
                if (precioMensualidad == null && cargoId.HasValue)
                {
                    precioMensualidad = await _context.PreciosMensualidadCurso
                        .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                               && p.CargoId == null
                                               && p.Activo);
                }

                if (precioMensualidad == null)
                {
                    return BadRequest(new { message = "No se encontrÃƒÂ³ precio configurado para las mensualidades de cursos especializados" });
                }

                decimal montoBase = precioMensualidad.Precio;
                decimal descuento = 0;

                // Aplicar beca si aplica
                if (matricula.Estudiante.EsBecado && matricula.Estudiante.PorcentajeBeca > 0)
                {
                    descuento = montoBase * (matricula.Estudiante.PorcentajeBeca / 100);
                }

                decimal montoFinal = montoBase - descuento;

                // Si es becado 100%, no se requiere pago
                if (montoFinal == 0)
                {
                    return BadRequest(new
                    {
                        message = "No se requiere pago para estudiantes becados al 100%.",
                        esBecado = true,
                        porcentajeBeca = matricula.Estudiante.PorcentajeBeca
                    });
                }

                // Validar detalle de pago
                if (model.DetallePago == null)
                {
                    return BadRequest(new { message = "Debe proporcionar el detalle del pago" });
                }

                if (model.DetallePago.TipoCambio <= 0)
                {
                    return BadRequest(new { message = "El tipo de cambio debe ser mayor a 0" });
                }

                // Calcular total pagado en USD
                var detalle = model.DetallePago;
                decimal totalCordobas = detalle.EfectivoCordobas + detalle.TarjetaCordobas;
                decimal totalDolares = detalle.EfectivoDolares + detalle.TarjetaDolares;
                decimal cordobasEnUSD = Math.Round(totalCordobas / detalle.TipoCambio, 2);
                decimal totalPagadoUSD = Math.Round(totalDolares + cordobasEnUSD, 2);

                // Validar que el monto pagado sea suficiente
                if (totalPagadoUSD < montoFinal)
                {
                    return BadRequest(new
                    {
                        message = "El monto pagado es insuficiente",
                        montoRequerido = montoFinal,
                        montoPagado = totalPagadoUSD,
                        diferencia = montoFinal - totalPagadoUSD
                    });
                }

                // Calcular vuelto
                decimal vuelto = Math.Round(totalPagadoUSD - montoFinal, 2);

                // Determinar mÃƒÂ©todo de pago
                string metodoPago = DeterminarMetodoPago(detalle);

                // Generar cÃƒÂ³digo de pago
                var anioActual = DateTime.Now.Year;
                var prefijo = $"PCURSO-{anioActual}-";
                var ultimoCodigo = await _context.PagosCurso
                    .Where(p => p.Codigo != null && p.Codigo.StartsWith(prefijo))
                    .OrderByDescending(p => p.Codigo)
                    .Select(p => p.Codigo)
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

                var pago = new PagoCurso
                {
                    Codigo = $"{prefijo}{siguienteNumero:D4}",
                    MatriculaCursoId = model.MatriculaCursoId,
                    NumeroMensualidad = model.NumeroMensualidad,
                    TipoPago = "Mensualidad",
                    Monto = montoBase,
                    Descuento = descuento,
                    MontoFinal = montoFinal,
                    FechaPago = DateTime.Now,
                    EfectivoCordobas = detalle.EfectivoCordobas,
                    EfectivoDolares = detalle.EfectivoDolares,
                    TarjetaCordobas = detalle.TarjetaCordobas,
                    TarjetaDolares = detalle.TarjetaDolares,
                    TipoCambio = detalle.TipoCambio,
                    TotalPagadoUSD = totalPagadoUSD,
                    Vuelto = vuelto,
                    VueltoCordobas = detalle.VueltoCordobas,
                    VueltoDolares = detalle.VueltoDolares,
                    MetodoPago = metodoPago,
                    NumeroComprobante = detalle.NumeroComprobante,
                    Observaciones = model.Observaciones,
                    Estado = "Completado"
                };

                _context.PagosCurso.Add(pago);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    pagoCursoId = pago.PagoCursoId,
                    codigo = pago.Codigo,
                    matriculaCursoId = matricula.MatriculaCursoId,
                    estudianteNombre = matricula.Estudiante.NombreCompleto,
                    cursoNombre = matricula.CursoEspecializado.Nombre,
                    numeroMensualidad = model.NumeroMensualidad,
                    montoFinal = pago.MontoFinal,
                    detallePago = new
                    {
                        efectivoCordobas = pago.EfectivoCordobas,
                        efectivoDolares = pago.EfectivoDolares,
                        tarjetaCordobas = pago.TarjetaCordobas,
                        tarjetaDolares = pago.TarjetaDolares,
                        tipoCambio = pago.TipoCambio,
                        totalPagadoUSD = pago.TotalPagadoUSD
                    },
                    vuelto = new
                    {
                        totalUSD = pago.Vuelto,
                        cordobas = pago.VueltoCordobas,
                        dolares = pago.VueltoDolares
                    },
                    metodoPago = pago.MetodoPago,
                    message = pago.Vuelto > 0
                        ? $"Pago registrado. Vuelto: C${pago.VueltoCordobas:F2} + ${pago.VueltoDolares:F2}"
                        : $"Pago de mensualidad {model.NumeroMensualidad} registrado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al procesar el pago",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        private string DeterminarMetodoPago(DetallePagoViewModel detalle)
        {
            bool tieneEfectivo = detalle.EfectivoCordobas > 0 || detalle.EfectivoDolares > 0;
            bool tieneTarjeta = detalle.TarjetaCordobas > 0 || detalle.TarjetaDolares > 0;

            if (tieneEfectivo && tieneTarjeta)
                return "Mixto";
            else if (tieneTarjeta)
                return "Tarjeta";
            else
                return "Efectivo";
        }
    }
}
