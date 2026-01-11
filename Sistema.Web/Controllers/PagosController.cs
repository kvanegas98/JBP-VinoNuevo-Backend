using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Instituto;
using Sistema.Web.Models.Pagos;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public PagosController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Pagos/Listar
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar()
        {
            var pagos = await _context.Pagos
                .Include(p => p.Matricula)
                    .ThenInclude(m => m.Estudiante)
                .Include(p => p.Matricula)
                    .ThenInclude(m => m.Modulo)
                .Include(p => p.Materia)
                .OrderByDescending(p => p.FechaPago)
                .Select(p => new PagoViewModel
                {
                    PagoId = p.PagoId,
                    Codigo = p.Codigo,
                    MatriculaId = p.MatriculaId,
                    MatriculaCodigo = p.Matricula.Codigo,
                    EstudianteId = p.Matricula.EstudianteId,
                    EstudianteCodigo = p.Matricula.Estudiante.Codigo,
                    EstudianteNombre = p.Matricula.Estudiante.NombreCompleto,
                    MateriaId = p.MateriaId,
                    MateriaNombre = p.Materia != null ? p.Materia.Nombre : null,
                    ModuloId = p.Matricula.ModuloId,
                    ModuloNombre = p.Matricula.Modulo.Nombre,
                    TipoPago = p.TipoPago,
                    Monto = p.Monto,
                    Descuento = p.Descuento,
                    MontoFinal = p.MontoFinal,
                    FechaPago = p.FechaPago,
                    EfectivoCordobas = p.EfectivoCordobas,
                    EfectivoDolares = p.EfectivoDolares,
                    TarjetaCordobas = p.TarjetaCordobas,
                    TarjetaDolares = p.TarjetaDolares,
                    TipoCambio = p.TipoCambio,
                    TotalPagadoUSD = p.TotalPagadoUSD,
                    MetodoPago = p.MetodoPago,
                    NumeroComprobante = p.NumeroComprobante,
                    Observaciones = p.Observaciones,
                    Estado = p.Estado
                })
                .ToListAsync();

            return Ok(pagos);
        }

        // GET: api/Pagos/BuscarEstudiante/{texto}
        // Busca estudiantes con matrículas pendientes, activas o completadas
        [HttpGet("[action]/{texto}")]
        public async Task<IActionResult> BuscarEstudiante([FromRoute] string texto)
        {
            var estudiantes = await _context.Estudiantes
                .Where(e => e.Activo &&
                           (e.NombreCompleto.Contains(texto) ||
                            e.Codigo.Contains(texto) ||
                            e.Cedula.Contains(texto)))
                .Select(e => new
                {
                    e.EstudianteId,
                    e.Codigo,
                    e.NombreCompleto,
                    e.Cedula,
                    e.EsBecado,
                    e.PorcentajeBeca,
                    MatriculasPendientes = e.Matriculas.Count(m => m.Estado == "Pendiente"),
                    MatriculasActivas = e.Matriculas.Count(m => m.Estado == "Activa"),
                    MatriculasCompletadas = e.Matriculas.Count(m => m.Estado == "Completada")
                })
                .Where(e => e.MatriculasPendientes > 0 || e.MatriculasActivas > 0 || e.MatriculasCompletadas > 0)
                .Take(10)
                .ToListAsync();

            return Ok(estudiantes);
        }

        // GET: api/Pagos/MatriculasPendientes/{estudianteId}
        // Lista matrículas pendientes de pago de matrícula
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> MatriculasPendientes([FromRoute] int estudianteId)
        {
            var matriculas = await _context.Matriculas
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .Where(m => m.EstudianteId == estudianteId && m.Estado == "Pendiente")
                .Select(m => new
                {
                    m.MatriculaId,
                    m.Codigo,
                    m.ModuloId,
                    ModuloNombre = m.Modulo.Nombre,
                    AnioLectivoNombre = m.Modulo.AnioLectivo.Nombre,
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

        // GET: api/Pagos/MatriculasActivas/{estudianteId}
        // Lista matrículas activas para pagar mensualidades
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> MatriculasActivas([FromRoute] int estudianteId)
        {
            var matriculas = await _context.Matriculas
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .Where(m => m.EstudianteId == estudianteId && m.Estado == "Activa")
                .Select(m => new
                {
                    m.MatriculaId,
                    m.Codigo,
                    m.ModuloId,
                    ModuloNombre = m.Modulo.Nombre,
                    AnioLectivoNombre = m.Modulo.AnioLectivo.Nombre,
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

        // GET: api/Pagos/MatriculasCompletadas/{estudianteId}
        // Lista matrículas completadas (para ver historial)
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> MatriculasCompletadas([FromRoute] int estudianteId)
        {
            var matriculas = await _context.Matriculas
                .Include(m => m.Modulo)
                    .ThenInclude(mod => mod.AnioLectivo)
                .Include(m => m.Modalidad)
                .Include(m => m.CategoriaEstudiante)
                .Where(m => m.EstudianteId == estudianteId && m.Estado == "Completada")
                .Select(m => new
                {
                    m.MatriculaId,
                    m.Codigo,
                    m.ModuloId,
                    ModuloNombre = m.Modulo.Nombre,
                    AnioLectivoNombre = m.Modulo.AnioLectivo.Nombre,
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

        // GET: api/Pagos/MateriasParaPago/{matriculaId}
        // Lista materias del módulo con estado de pago
        [HttpGet("[action]/{matriculaId}")]
        public async Task<IActionResult> MateriasParaPago([FromRoute] int matriculaId)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Estudiante)
                .Include(m => m.Modulo)
                .Include(m => m.CategoriaEstudiante)
                .FirstOrDefaultAsync(m => m.MatriculaId == matriculaId);

            if (matricula == null)
            {
                return NotFound(new { message = "Matrícula no encontrada" });
            }

            if (matricula.Estado != "Activa" && matricula.Estado != "Completada")
            {
                return BadRequest(new { message = "La matrícula debe estar activa o completada para consultar mensualidades" });
            }

            bool soloLectura = matricula.Estado == "Completada";

            // Obtener materias del módulo
            var materias = await _context.Materias
                .Where(mat => mat.ModuloId == matricula.ModuloId && mat.Activo)
                .ToListAsync();

            // Obtener pagos de mensualidad existentes para esta matrícula
            var pagosExistentes = await _context.Pagos
                .Where(p => p.MatriculaId == matriculaId &&
                           p.TipoPago == "Mensualidad" &&
                           p.Estado == "Completado")
                .Select(p => p.MateriaId)
                .ToListAsync();

            // Obtener el cargo del estudiante (si es interno)
            int? cargoId = null;
            string cargoNombre = null;

            if (matricula.Estudiante.EsInterno)
            {
                var estudianteCargo = await _context.EstudianteCargos
                    .Include(ec => ec.Cargo)
                    .Where(ec => ec.EstudianteId == matricula.EstudianteId)
                    .FirstOrDefaultAsync();

                if (estudianteCargo != null)
                {
                    cargoId = estudianteCargo.CargoId;
                    cargoNombre = estudianteCargo.Cargo.Nombre;
                }
            }

            // Obtener precio de mensualidad según categoría + cargo
            var precioMensualidad = await _context.PreciosMensualidad
                .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                       && p.CargoId == cargoId
                                       && p.Activo);

            // Si no encuentra precio específico para el cargo, buscar sin cargo (precio base de la categoría)
            if (precioMensualidad == null && cargoId.HasValue)
            {
                precioMensualidad = await _context.PreciosMensualidad
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                           && p.CargoId == null
                                           && p.Activo);
            }

            decimal montoBase = precioMensualidad?.Precio ?? 0;
            decimal descuento = 0;
            decimal montoFinal = montoBase;
            string tipoDescuento = cargoNombre ?? "Sin cargo";

            // Verificar si es becado y aplicar descuento
            bool esBecado = matricula.Estudiante.EsBecado;
            decimal porcentajeBeca = matricula.Estudiante.PorcentajeBeca;

            if (esBecado && porcentajeBeca > 0)
            {
                descuento = montoBase * (porcentajeBeca / 100);
                montoFinal = montoBase - descuento;
                tipoDescuento = $"Beca ({porcentajeBeca}%)";
            }

            bool esBecado100 = esBecado && porcentajeBeca >= 100;

            // Si es becado 100%, todas las materias se consideran "pagadas" automáticamente
            var resultado = materias.Select(mat => new
            {
                mat.MateriaId,
                mat.Nombre,
                Pagado = esBecado100 || pagosExistentes.Contains(mat.MateriaId),
                PagadoAutomaticamente = esBecado100,
                MontoBase = montoBase,
                Descuento = descuento,
                MontoFinal = montoFinal,
                TipoDescuento = tipoDescuento
            }).ToList();

            int materiasPagadas = esBecado100 ? materias.Count : pagosExistentes.Count;
            int materiasPendientes = esBecado100 ? 0 : materias.Count - pagosExistentes.Count;

            return Ok(new
            {
                matriculaId = matricula.MatriculaId,
                matriculaCodigo = matricula.Codigo,
                estudianteNombre = matricula.Estudiante.NombreCompleto,
                moduloNombre = matricula.Modulo.Nombre,
                categoriaNombre = matricula.CategoriaEstudiante.Nombre,
                cargoNombre = cargoNombre,
                esBecado = esBecado,
                porcentajeBeca = porcentajeBeca,
                esBecado100 = esBecado100,
                estadoMatricula = matricula.Estado,
                soloLectura = soloLectura,
                materias = resultado,
                resumen = new
                {
                    totalMaterias = materias.Count,
                    materiasPagadas = materiasPagadas,
                    materiasPendientes = materiasPendientes,
                    montoPendiente = materiasPendientes * montoFinal
                }
            });
        }

        // POST: api/Pagos/PagarMatricula
        // Paga la matrícula y la activa
        [HttpPost("[action]")]
        public async Task<IActionResult> PagarMatricula([FromBody] PagarMatriculaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var matricula = await _context.Matriculas
                    .Include(m => m.Estudiante)
                    .Include(m => m.Modulo)
                    .FirstOrDefaultAsync(m => m.MatriculaId == model.MatriculaId);

                if (matricula == null)
                {
                    return NotFound(new { message = "Matrícula no encontrada" });
                }

                if (matricula.Estado == "Activa")
                {
                    return BadRequest(new { message = "La matrícula ya está pagada y activa" });
                }

                if (matricula.Estado == "Anulada")
                {
                    return BadRequest(new { message = "No se puede pagar una matrícula anulada" });
                }

                // Verificar que no exista pago de matrícula previo
                var pagoExistente = await _context.Pagos
                    .FirstOrDefaultAsync(p => p.MatriculaId == model.MatriculaId &&
                                             p.TipoPago == "Matricula" &&
                                             p.Estado == "Completado");

                if (pagoExistente != null)
                {
                    return BadRequest(new { message = "Ya existe un pago de matrícula para esta inscripción" });
                }

                // Si el monto es $0 (becado 100%), no se requiere pago - la matrícula ya está activa
                if (matricula.MontoFinal == 0)
                {
                    return BadRequest(new {
                        message = "No se requiere pago para estudiantes becados al 100%. La matrícula ya fue activada automáticamente.",
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

                // Calcular total pagado en USD (redondeado a 2 decimales para evitar problemas de precisión)
                var detalle = model.DetallePago;
                decimal totalCordobas = detalle.EfectivoCordobas + detalle.TarjetaCordobas;
                decimal totalDolares = detalle.EfectivoDolares + detalle.TarjetaDolares;
                decimal cordobasEnUSD = Math.Round(totalCordobas / detalle.TipoCambio, 2);
                decimal totalPagadoUSD = Math.Round(totalDolares + cordobasEnUSD, 2);

                // Validar que el monto pagado sea suficiente
                if (totalPagadoUSD < matricula.MontoFinal)
                {
                    return BadRequest(new {
                        message = "El monto pagado es insuficiente",
                        montoRequerido = matricula.MontoFinal,
                        montoPagado = totalPagadoUSD,
                        diferencia = matricula.MontoFinal - totalPagadoUSD
                    });
                }

                // Calcular vuelto (el usuario puede elegir libremente cómo entregarlo)
                decimal vuelto = Math.Round(totalPagadoUSD - matricula.MontoFinal, 2);

                // Determinar método de pago (para referencia rápida)
                string metodoPago = DeterminarMetodoPago(detalle);

                // Generar código de pago
                var anioActual = DateTime.Now.Year;
                var prefijo = $"PAG-{anioActual}-";
                var ultimoCodigo = await _context.Pagos
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

                var pago = new Pago
                {
                    Codigo = $"{prefijo}{siguienteNumero:D4}",
                    MatriculaId = model.MatriculaId,
                    MateriaId = null,
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

                _context.Pagos.Add(pago);

                // Activar la matrícula
                matricula.Estado = "Activa";

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    pagoId = pago.PagoId,
                    codigo = pago.Codigo,
                    matriculaId = matricula.MatriculaId,
                    matriculaCodigo = matricula.Codigo,
                    estudianteNombre = matricula.Estudiante.NombreCompleto,
                    moduloNombre = matricula.Modulo.Nombre,
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
                        : "Pago de matrícula registrado y matrícula activada"
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

        // POST: api/Pagos/PagarMensualidad
        // Paga la mensualidad de una materia
        [HttpPost("[action]")]
        public async Task<IActionResult> PagarMensualidad([FromBody] PagarMensualidadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var matricula = await _context.Matriculas
                    .Include(m => m.Estudiante)
                        .ThenInclude(e => e.EstudianteCargos)
                            .ThenInclude(ec => ec.Cargo)
                    .Include(m => m.Modulo)
                    .Include(m => m.CategoriaEstudiante)
                    .FirstOrDefaultAsync(m => m.MatriculaId == model.MatriculaId);

                if (matricula == null)
                {
                    return NotFound(new { message = "Matrícula no encontrada" });
                }

                if (matricula.Estado != "Activa")
                {
                    return BadRequest(new { message = "La matrícula debe estar activa para pagar mensualidades" });
                }

                // Verificar que la materia existe y pertenece al módulo
                var materia = await _context.Materias
                    .FirstOrDefaultAsync(mat => mat.MateriaId == model.MateriaId &&
                                                mat.ModuloId == matricula.ModuloId);

                if (materia == null)
                {
                    return BadRequest(new { message = "La materia no pertenece al módulo de esta matrícula" });
                }

                // Verificar que no exista pago previo de esta materia
                var pagoExistente = await _context.Pagos
                    .FirstOrDefaultAsync(p => p.MatriculaId == model.MatriculaId &&
                                             p.MateriaId == model.MateriaId &&
                                             p.TipoPago == "Mensualidad" &&
                                             p.Estado == "Completado");

                if (pagoExistente != null)
                {
                    return BadRequest(new { message = "Ya existe un pago de mensualidad para esta materia" });
                }

                // Obtener el cargo del estudiante (si es interno)
                int? cargoId = null;
                string cargoNombre = null;

                if (matricula.Estudiante.EsInterno && matricula.Estudiante.EstudianteCargos.Any())
                {
                    var cargo = matricula.Estudiante.EstudianteCargos.First();
                    cargoId = cargo.CargoId;
                    cargoNombre = cargo.Cargo.Nombre;
                }

                // Obtener precio de mensualidad según categoría + cargo
                var precioMensualidad = await _context.PreciosMensualidad
                    .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                           && p.CargoId == cargoId
                                           && p.Activo);

                // Si no encuentra precio específico para el cargo, buscar sin cargo (precio base)
                if (precioMensualidad == null && cargoId.HasValue)
                {
                    precioMensualidad = await _context.PreciosMensualidad
                        .FirstOrDefaultAsync(p => p.CategoriaEstudianteId == matricula.CategoriaEstudianteId
                                               && p.CargoId == null
                                               && p.Activo);
                }

                decimal montoBase = precioMensualidad?.Precio ?? 0;
                decimal descuento = 0;
                decimal montoFinal = montoBase;
                string tipoDescuento = cargoNombre ?? "Sin cargo";

                // Verificar si es becado y aplicar descuento
                bool esBecado = matricula.Estudiante.EsBecado;
                decimal porcentajeBeca = matricula.Estudiante.PorcentajeBeca;

                if (esBecado && porcentajeBeca > 0)
                {
                    descuento = montoBase * (porcentajeBeca / 100);
                    montoFinal = montoBase - descuento;
                    tipoDescuento = $"Beca ({porcentajeBeca}%)";
                }

                // Si el monto es $0 (becado 100%), no se requiere pago
                if (montoFinal == 0)
                {
                    return BadRequest(new {
                        message = "No se requiere pago para estudiantes becados al 100%. La materia se considera pagada automáticamente.",
                        esBecado = true,
                        porcentajeBeca = porcentajeBeca,
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

                // Calcular total pagado en USD (redondeado a 2 decimales para evitar problemas de precisión)
                var detalle = model.DetallePago;
                decimal totalCordobas = detalle.EfectivoCordobas + detalle.TarjetaCordobas;
                decimal totalDolares = detalle.EfectivoDolares + detalle.TarjetaDolares;
                decimal cordobasEnUSD = Math.Round(totalCordobas / detalle.TipoCambio, 2);
                decimal totalPagadoUSD = Math.Round(totalDolares + cordobasEnUSD, 2);

                // Validar que el monto pagado sea suficiente
                if (totalPagadoUSD < montoFinal)
                {
                    return BadRequest(new {
                        message = "El monto pagado es insuficiente",
                        montoRequerido = montoFinal,
                        montoPagado = totalPagadoUSD,
                        diferencia = montoFinal - totalPagadoUSD
                    });
                }

                // Calcular vuelto (el usuario puede elegir libremente cómo entregarlo)
                decimal vuelto = Math.Round(totalPagadoUSD - montoFinal, 2);

                // Determinar método de pago
                string metodoPago = DeterminarMetodoPago(detalle);

                // Generar código de pago
                var anioActual = DateTime.Now.Year;
                var prefijo = $"PAG-{anioActual}-";
                var ultimoCodigo = await _context.Pagos
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

                var pago = new Pago
                {
                    Codigo = $"{prefijo}{siguienteNumero:D4}",
                    MatriculaId = model.MatriculaId,
                    MateriaId = model.MateriaId,
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

                _context.Pagos.Add(pago);
                await _context.SaveChangesAsync();

                // Verificar si se completaron todas las materias del módulo
                bool matriculaCompletada = false;
                var totalMateriasModulo = await _context.Materias
                    .CountAsync(m => m.ModuloId == matricula.ModuloId && m.Activo);

                var materiasPagadas = await _context.Pagos
                    .CountAsync(p => p.MatriculaId == matricula.MatriculaId &&
                                    p.TipoPago == "Mensualidad" &&
                                    p.Estado == "Completado");

                if (materiasPagadas >= totalMateriasModulo)
                {
                    matricula.Estado = "Completada";
                    await _context.SaveChangesAsync();
                    matriculaCompletada = true;
                }

                string mensaje = pago.Vuelto > 0
                    ? $"Pago registrado. Vuelto: C${pago.VueltoCordobas:F2} + ${pago.VueltoDolares:F2}"
                    : "Pago de mensualidad registrado exitosamente";

                if (matriculaCompletada)
                {
                    mensaje += ". ¡Felicidades! Se han completado todos los pagos del módulo.";
                }

                return Ok(new
                {
                    pagoId = pago.PagoId,
                    codigo = pago.Codigo,
                    matriculaId = matricula.MatriculaId,
                    matriculaCodigo = matricula.Codigo,
                    estudianteNombre = matricula.Estudiante.NombreCompleto,
                    materiaNombre = materia.Nombre,
                    categoriaNombre = matricula.CategoriaEstudiante.Nombre,
                    cargoNombre = cargoNombre,
                    montoFinal = montoFinal,
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
                    matriculaCompletada = matriculaCompletada,
                    progreso = new
                    {
                        materiasPagadas = materiasPagadas,
                        totalMaterias = totalMateriasModulo
                    },
                    message = mensaje
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

        // GET: api/Pagos/HistorialEstudiante/{estudianteId}
        // Historial de pagos de un estudiante
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> HistorialEstudiante([FromRoute] int estudianteId)
        {
            var pagos = await _context.Pagos
                .Include(p => p.Matricula)
                    .ThenInclude(m => m.Modulo)
                .Include(p => p.Materia)
                .Where(p => p.Matricula.EstudianteId == estudianteId)
                .OrderByDescending(p => p.FechaPago)
                .Select(p => new PagoViewModel
                {
                    PagoId = p.PagoId,
                    Codigo = p.Codigo,
                    MatriculaId = p.MatriculaId,
                    MatriculaCodigo = p.Matricula.Codigo,
                    MateriaId = p.MateriaId,
                    MateriaNombre = p.Materia != null ? p.Materia.Nombre : null,
                    ModuloId = p.Matricula.ModuloId,
                    ModuloNombre = p.Matricula.Modulo.Nombre,
                    TipoPago = p.TipoPago,
                    Monto = p.Monto,
                    Descuento = p.Descuento,
                    MontoFinal = p.MontoFinal,
                    FechaPago = p.FechaPago,
                    EfectivoCordobas = p.EfectivoCordobas,
                    EfectivoDolares = p.EfectivoDolares,
                    TarjetaCordobas = p.TarjetaCordobas,
                    TarjetaDolares = p.TarjetaDolares,
                    TipoCambio = p.TipoCambio,
                    TotalPagadoUSD = p.TotalPagadoUSD,
                    MetodoPago = p.MetodoPago,
                    NumeroComprobante = p.NumeroComprobante,
                    Estado = p.Estado
                })
                .ToListAsync();

            return Ok(pagos);
        }

        // PUT: api/Pagos/Anular/{id}
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> Anular([FromRoute] int id, [FromQuery] string motivo = null)
        {
            var pago = await _context.Pagos
                .Include(p => p.Matricula)
                .FirstOrDefaultAsync(p => p.PagoId == id);

            if (pago == null)
            {
                return NotFound(new { message = "Pago no encontrado" });
            }

            if (pago.Estado == "Anulado")
            {
                return BadRequest(new { message = "El pago ya está anulado" });
            }

            pago.Estado = "Anulado";
            pago.Observaciones = string.IsNullOrEmpty(pago.Observaciones)
                ? $"ANULADO: {motivo}"
                : $"{pago.Observaciones} | ANULADO: {motivo}";

            // Si es pago de matrícula, volver la matrícula a Pendiente
            if (pago.TipoPago == "Matricula")
            {
                pago.Matricula.Estado = "Pendiente";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                pagoId = pago.PagoId,
                codigo = pago.Codigo,
                estado = pago.Estado,
                estadoMatricula = pago.TipoPago == "Matricula" ? pago.Matricula.Estado : null,
                message = "Pago anulado exitosamente"
            });
        }

        // GET: api/Pagos/ResumenEstudiante/{estudianteId}
        // Resumen de pagos pendientes y realizados
        [HttpGet("[action]/{estudianteId}")]
        public async Task<IActionResult> ResumenEstudiante([FromRoute] int estudianteId)
        {
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.EstudianteId == estudianteId);

            if (estudiante == null)
            {
                return NotFound(new { message = "Estudiante no encontrado" });
            }

            var matriculas = await _context.Matriculas
                .Include(m => m.Modulo)
                .Include(m => m.Pagos)
                .Where(m => m.EstudianteId == estudianteId && m.Estado != "Anulada")
                .ToListAsync();

            var resumen = new List<object>();

            foreach (var mat in matriculas)
            {
                var materias = await _context.Materias
                    .Where(m => m.ModuloId == mat.ModuloId && m.Activo)
                    .ToListAsync();

                var pagoMatricula = mat.Pagos.FirstOrDefault(p => p.TipoPago == "Matricula" && p.Estado == "Completado");
                var pagosMensualidad = mat.Pagos.Where(p => p.TipoPago == "Mensualidad" && p.Estado == "Completado").ToList();

                resumen.Add(new
                {
                    matriculaId = mat.MatriculaId,
                    matriculaCodigo = mat.Codigo,
                    moduloNombre = mat.Modulo.Nombre,
                    estadoMatricula = mat.Estado,
                    matriculaPagada = pagoMatricula != null,
                    montoMatricula = mat.MontoFinal,
                    totalMaterias = materias.Count,
                    materiasPagadas = pagosMensualidad.Count,
                    materiasPendientes = materias.Count - pagosMensualidad.Count
                });
            }

            return Ok(new
            {
                estudianteId = estudiante.EstudianteId,
                estudianteCodigo = estudiante.Codigo,
                estudianteNombre = estudiante.NombreCompleto,
                matriculas = resumen
            });
        }

        // Método auxiliar para determinar el tipo de pago
        private string DeterminarMetodoPago(DetallePagoViewModel detalle)
        {
            bool tieneEfectivo = detalle.EfectivoCordobas > 0 || detalle.EfectivoDolares > 0;
            bool tieneTarjeta = detalle.TarjetaCordobas > 0 || detalle.TarjetaDolares > 0;

            if (tieneEfectivo && tieneTarjeta)
                return "Mixto";
            if (tieneTarjeta)
                return "Tarjeta";
            return "Efectivo";
        }

        // GET: api/Pagos/CierreCaja
        // Obtiene el resumen de pagos para cierre de caja
        [HttpGet("[action]")]
        public async Task<IActionResult> CierreCaja([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            // Si no se especifica fecha, usar el día actual
            var inicio = fechaInicio?.Date ?? DateTime.Today;
            var fin = (fechaFin?.Date ?? DateTime.Today).AddDays(1).AddSeconds(-1); // Fin del día

            var pagos = await _context.Pagos
                .Where(p => p.Estado == "Completado" &&
                           p.FechaPago >= inicio &&
                           p.FechaPago <= fin)
                .ToListAsync();

            // Resumen por forma de pago y moneda
            var resumen = new
            {
                fechaInicio = inicio,
                fechaFin = fin,
                totalPagos = pagos.Count,

                // Efectivo
                efectivo = new
                {
                    cordobas = pagos.Sum(p => p.EfectivoCordobas),
                    dolares = pagos.Sum(p => p.EfectivoDolares)
                },

                // Tarjeta
                tarjeta = new
                {
                    cordobas = pagos.Sum(p => p.TarjetaCordobas),
                    dolares = pagos.Sum(p => p.TarjetaDolares)
                },

                // Totales
                totales = new
                {
                    totalCordobas = pagos.Sum(p => p.EfectivoCordobas + p.TarjetaCordobas),
                    totalDolares = pagos.Sum(p => p.EfectivoDolares + p.TarjetaDolares),
                    totalEnUSD = pagos.Sum(p => p.TotalPagadoUSD),
                    montoFinal = pagos.Sum(p => p.MontoFinal),
                    totalVueltosUSD = pagos.Sum(p => p.Vuelto),
                    totalVueltosCordobas = pagos.Sum(p => p.VueltoCordobas),
                    totalVueltosDolares = pagos.Sum(p => p.VueltoDolares)
                },

                // Desglose por tipo de pago
                porTipoPago = new
                {
                    matriculas = new
                    {
                        cantidad = pagos.Count(p => p.TipoPago == "Matricula"),
                        totalUSD = pagos.Where(p => p.TipoPago == "Matricula").Sum(p => p.TotalPagadoUSD)
                    },
                    mensualidades = new
                    {
                        cantidad = pagos.Count(p => p.TipoPago == "Mensualidad"),
                        totalUSD = pagos.Where(p => p.TipoPago == "Mensualidad").Sum(p => p.TotalPagadoUSD)
                    }
                },

                // Desglose por método de pago
                porMetodo = new
                {
                    efectivo = pagos.Count(p => p.MetodoPago == "Efectivo"),
                    tarjeta = pagos.Count(p => p.MetodoPago == "Tarjeta"),
                    mixto = pagos.Count(p => p.MetodoPago == "Mixto")
                }
            };

            return Ok(resumen);
        }

        // GET: api/Pagos/DetalleCierreCaja
        // Obtiene el detalle de pagos para cierre de caja
        [HttpGet("[action]")]
        public async Task<IActionResult> DetalleCierreCaja([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin)
        {
            var inicio = fechaInicio?.Date ?? DateTime.Today;
            var fin = (fechaFin?.Date ?? DateTime.Today).AddDays(1).AddSeconds(-1);

            var pagos = await _context.Pagos
                .Include(p => p.Matricula)
                    .ThenInclude(m => m.Estudiante)
                .Include(p => p.Materia)
                .Where(p => p.Estado == "Completado" &&
                           p.FechaPago >= inicio &&
                           p.FechaPago <= fin)
                .OrderBy(p => p.FechaPago)
                .Select(p => new
                {
                    p.PagoId,
                    p.Codigo,
                    p.FechaPago,
                    estudianteNombre = p.Matricula.Estudiante.NombreCompleto,
                    p.TipoPago,
                    materiaNombre = p.Materia != null ? p.Materia.Nombre : null,
                    p.MontoFinal,
                    p.EfectivoCordobas,
                    p.EfectivoDolares,
                    p.TarjetaCordobas,
                    p.TarjetaDolares,
                    p.TipoCambio,
                    p.TotalPagadoUSD,
                    p.Vuelto,
                    p.VueltoCordobas,
                    p.VueltoDolares,
                    p.MetodoPago,
                    p.NumeroComprobante
                })
                .ToListAsync();

            return Ok(new
            {
                fechaInicio = inicio,
                fechaFin = fin,
                pagos = pagos
            });
        }
    }
}
