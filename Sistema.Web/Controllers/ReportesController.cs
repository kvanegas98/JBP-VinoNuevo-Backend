using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public ReportesController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Reportes/Dashboard
        // Dashboard principal con métricas generales (optimizado)
        [HttpGet("[action]")]
        public async Task<IActionResult> Dashboard([FromQuery] int? anioLectivoId = null)
        {
            try
            {
                // Si no se especifica año, tomar el más reciente
                if (!anioLectivoId.HasValue)
                {
                    var ultimoAnio = await _context.AniosLectivos
                        .OrderByDescending(a => a.AnioLectivoId)
                        .FirstOrDefaultAsync();
                    anioLectivoId = ultimoAnio?.AnioLectivoId;
                }

                // Estudiantes activos
                var totalEstudiantes = await _context.Estudiantes.CountAsync(e => e.Activo);
                var estudiantesInternos = await _context.Estudiantes.CountAsync(e => e.Activo && e.EsInterno);
                var estudiantesExternos = totalEstudiantes - estudiantesInternos;
                var estudiantesBecados = await _context.Estudiantes.CountAsync(e => e.Activo && e.EsBecado);

                // Matrículas
                var matriculas = await _context.Matriculas
                    .Where(m => !anioLectivoId.HasValue || m.Modulo.AnioLectivoId == anioLectivoId.Value)
                    .GroupBy(m => m.Estado)
                    .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                    .ToListAsync();

                var totalMatriculas = matriculas.Sum(m => m.Cantidad);
                var pendientes = matriculas.FirstOrDefault(m => m.Estado == "Pendiente")?.Cantidad ?? 0;
                var activas = matriculas.FirstOrDefault(m => m.Estado == "Activa")?.Cantidad ?? 0;
                var completadas = matriculas.FirstOrDefault(m => m.Estado == "Completada")?.Cantidad ?? 0;
                var anuladas = matriculas.FirstOrDefault(m => m.Estado == "Anulada")?.Cantidad ?? 0;

                // Finanzas del mes actual
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddSeconds(-1);

                var pagosMes = await _context.Pagos
                    .Where(p => p.Estado == "Completado" &&
                               p.FechaPago >= inicioMes &&
                               p.FechaPago <= finMes)
                    .ToListAsync();

                var recaudadoMes = pagosMes.Sum(p => p.MontoFinal);
                var pagosPorTipo = pagosMes
                    .GroupBy(p => p.TipoPago)
                    .Select(g => new {
                        Tipo = g.Key,
                        Cantidad = g.Count(),
                        Total = g.Sum(p => p.MontoFinal)
                    })
                    .ToList();

                var pagosPorMetodo = pagosMes
                    .GroupBy(p => p.MetodoPago)
                    .Select(g => new {
                        Metodo = g.Key,
                        Total = g.Sum(p => p.MontoFinal),
                        Porcentaje = recaudadoMes > 0 ? Math.Round((g.Sum(p => p.MontoFinal) / recaudadoMes) * 100, 1) : 0
                    })
                    .ToList();

                // Pagos pendientes (estimado)
                var matriculasActivas = await _context.Matriculas
                    .Where(m => m.Estado == "Activa")
                    .Include(m => m.Modulo)
                    .ToListAsync();

                decimal pagosPendientesEstimado = 0;
                foreach (var mat in matriculasActivas)
                {
                    var totalMaterias = await _context.Materias
                        .CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);
                    var materiasPagadas = await _context.Pagos
                        .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                        p.TipoPago == "Mensualidad" &&
                                        p.Estado == "Completado");
                    var materiasPendientes = totalMaterias - materiasPagadas;

                    // Estimado: usar el monto final de la matrícula como base
                    if (materiasPendientes > 0)
                    {
                        pagosPendientesEstimado += mat.MontoFinal * materiasPendientes;
                    }
                }

                // Estudiantes por red
                var estudiantesPorRed = await _context.Estudiantes
                    .Where(e => e.Activo)
                    .Include(e => e.Red)
                    .ToListAsync();

                var estudiantesAgrupadosPorRed = estudiantesPorRed
                    .GroupBy(e => e.Red?.Nombre ?? "Sin Red")
                    .Select(g => new { Red = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad)
                    .ToList();

                // Alertas
                var alertas = new System.Collections.Generic.List<object>();

                // Alerta: Estudiantes con muchas materias pendientes
                var estudiantesConMora = await _context.Matriculas
                    .Where(m => m.Estado == "Activa")
                    .Include(m => m.Estudiante)
                    .Include(m => m.Modulo)
                    .ToListAsync();

                int contadorMoraAlta = 0;
                foreach (var mat in estudiantesConMora)
                {
                    var totalMaterias = await _context.Materias.CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);
                    var materiasPagadas = await _context.Pagos
                        .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                        p.TipoPago == "Mensualidad" &&
                                        p.Estado == "Completado");
                    if ((totalMaterias - materiasPagadas) >= 2)
                    {
                        contadorMoraAlta++;
                    }
                }

                if (contadorMoraAlta > 0)
                {
                    alertas.Add(new {
                        Tipo = "warning",
                        Mensaje = $"{contadorMoraAlta} estudiantes con 2+ materias pendientes"
                    });
                }

                // Alerta: Tipo de cambio sin actualizar
                var ultimoTipoCambio = await _context.TiposCambio
                    .Where(t => t.VigenteHasta == null)
                    .FirstOrDefaultAsync();

                if (ultimoTipoCambio != null)
                {
                    var diasSinActualizar = (DateTime.Now - ultimoTipoCambio.FechaRegistro).Days;
                    if (diasSinActualizar >= 7)
                    {
                        alertas.Add(new {
                            Tipo = "warning",
                            Mensaje = $"Tipo de cambio: {diasSinActualizar} días sin actualizar"
                        });
                    }
                }

                return Ok(new
                {
                    estudiantes = new
                    {
                        total = totalEstudiantes,
                        activos = totalEstudiantes,
                        internos = estudiantesInternos,
                        externos = estudiantesExternos,
                        becados = estudiantesBecados
                    },
                    matriculas = new
                    {
                        total = totalMatriculas,
                        pendientes = pendientes,
                        activas = activas,
                        completadas = completadas,
                        anuladas = anuladas,
                        porcentajes = new
                        {
                            pendientes = totalMatriculas > 0 ? Math.Round((decimal)pendientes / totalMatriculas * 100, 1) : 0,
                            activas = totalMatriculas > 0 ? Math.Round((decimal)activas / totalMatriculas * 100, 1) : 0,
                            completadas = totalMatriculas > 0 ? Math.Round((decimal)completadas / totalMatriculas * 100, 1) : 0,
                            anuladas = totalMatriculas > 0 ? Math.Round((decimal)anuladas / totalMatriculas * 100, 1) : 0
                        }
                    },
                    finanzas = new
                    {
                        mes = DateTime.Now.ToString("MMMM yyyy"),
                        recaudado = recaudadoMes,
                        pagosPendientesEstimado = pagosPendientesEstimado,
                        porTipo = pagosPorTipo,
                        porMetodo = pagosPorMetodo
                    },
                    redes = estudiantesAgrupadosPorRed,
                    alertas = alertas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al generar el dashboard",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/Reportes/Morosidad
        // Reporte de estudiantes con pagos pendientes (con paginación)
        [HttpGet("[action]")]
        public async Task<IActionResult> Morosidad(
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20,
            [FromQuery] int? redId = null,
            [FromQuery] bool? esInterno = null,
            [FromQuery] int? moduloId = null,
            [FromQuery] string severidad = null) // "leve", "moderada", "grave"
        {
            try
            {
                // Obtener todas las matrículas activas
                var query = _context.Matriculas
                    .Where(m => m.Estado == "Activa")
                    .Include(m => m.Estudiante)
                        .ThenInclude(e => e.Red)
                    .Include(m => m.Modulo)
                    .AsQueryable();

                // Filtros
                if (redId.HasValue)
                {
                    query = query.Where(m => m.Estudiante.RedId == redId.Value);
                }

                if (esInterno.HasValue)
                {
                    query = query.Where(m => m.Estudiante.EsInterno == esInterno.Value);
                }

                if (moduloId.HasValue)
                {
                    query = query.Where(m => m.ModuloId == moduloId.Value);
                }

                var matriculasActivas = await query.ToListAsync();

                // Calcular mora para cada matrícula
                var estudiantesConMora = new System.Collections.Generic.List<object>();

                foreach (var mat in matriculasActivas)
                {
                    var totalMaterias = await _context.Materias
                        .CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);

                    var materiasPagadas = await _context.Pagos
                        .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                        p.TipoPago == "Mensualidad" &&
                                        p.Estado == "Completado");

                    var materiasPendientes = totalMaterias - materiasPagadas;

                    if (materiasPendientes > 0)
                    {
                        // Calcular severidad
                        string sev = materiasPendientes == 1 ? "leve" :
                                    materiasPendientes <= 3 ? "moderada" : "grave";

                        // Filtro de severidad
                        if (!string.IsNullOrEmpty(severidad) && sev != severidad.ToLower())
                        {
                            continue;
                        }

                        // Estimar monto (simplificado: usar MontoFinal de matrícula como base)
                        decimal montoPendiente = mat.MontoFinal * materiasPendientes;

                        estudiantesConMora.Add(new
                        {
                            estudianteId = mat.EstudianteId,
                            estudianteCodigo = mat.Estudiante.Codigo,
                            estudianteNombre = mat.Estudiante.NombreCompleto,
                            matriculaId = mat.MatriculaId,
                            matriculaCodigo = mat.Codigo,
                            moduloNombre = mat.Modulo.Nombre,
                            redNombre = mat.Estudiante.Red?.Nombre ?? "Sin Red",
                            esInterno = mat.Estudiante.EsInterno,
                            materiasPendientes = materiasPendientes,
                            totalMaterias = totalMaterias,
                            montoPendiente = montoPendiente,
                            severidad = sev
                        });
                    }
                }

                // Ordenar por monto pendiente (mayor a menor)
                var estudiantesOrdenados = estudiantesConMora
                    .OrderByDescending(e => ((dynamic)e).montoPendiente)
                    .ToList();

                // Paginación
                var total = estudiantesOrdenados.Count;
                var datos = estudiantesOrdenados
                    .Skip((pagina - 1) * porPagina)
                    .Take(porPagina)
                    .ToList();

                // Resumen
                var montoTotalMora = estudiantesOrdenados.Sum(e => (decimal)((dynamic)e).montoPendiente);
                var porSeveridad = estudiantesOrdenados
                    .GroupBy(e => ((dynamic)e).severidad)
                    .Select(g => new { severidad = g.Key, cantidad = g.Count() })
                    .ToList();

                return Ok(new
                {
                    total = total,
                    pagina = pagina,
                    porPagina = porPagina,
                    totalPaginas = (int)Math.Ceiling((double)total / porPagina),
                    resumen = new
                    {
                        totalEstudiantesConMora = total,
                        montoTotalMora = montoTotalMora,
                        leve = porSeveridad.FirstOrDefault(s => s.severidad == "leve")?.cantidad ?? 0,
                        moderada = porSeveridad.FirstOrDefault(s => s.severidad == "moderada")?.cantidad ?? 0,
                        grave = porSeveridad.FirstOrDefault(s => s.severidad == "grave")?.cantidad ?? 0
                    },
                    datos = datos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al generar el reporte de morosidad",
                    error = ex.Message
                });
            }
        }

        // GET: api/Reportes/PorRed
        // Análisis de estudiantes y finanzas por red
        [HttpGet("[action]")]
        public async Task<IActionResult> PorRed([FromQuery] int? anioLectivoId = null)
        {
            try
            {
                var redes = await _context.Redes
                    .Where(r => r.Activo)
                    .ToListAsync();

                var reportePorRed = new System.Collections.Generic.List<object>();

                foreach (var red in redes)
                {
                    // Estudiantes de esta red
                    var estudiantesRed = await _context.Estudiantes
                        .Where(e => e.Activo && e.RedId == red.RedId)
                        .ToListAsync();

                    var totalEstudiantes = estudiantesRed.Count;
                    var internos = estudiantesRed.Count(e => e.EsInterno);
                    var externos = totalEstudiantes - internos;
                    var becados = estudiantesRed.Count(e => e.EsBecado);

                    // Matrículas de estudiantes de esta red
                    var estudianteIds = estudiantesRed.Select(e => e.EstudianteId).ToList();

                    var matriculasRed = await _context.Matriculas
                        .Where(m => estudianteIds.Contains(m.EstudianteId))
                        .Where(m => !anioLectivoId.HasValue || m.Modulo.AnioLectivoId == anioLectivoId.Value)
                        .ToListAsync();

                    var activas = matriculasRed.Count(m => m.Estado == "Activa");
                    var completadas = matriculasRed.Count(m => m.Estado == "Completada");
                    var totalMatriculas = matriculasRed.Count;

                    var tasaCompletacion = totalMatriculas > 0
                        ? Math.Round((decimal)completadas / totalMatriculas * 100, 1)
                        : 0;

                    // Calcular morosidad
                    int estudiantesConMora = 0;
                    decimal montoPendiente = 0;

                    var matriculasActivas = matriculasRed.Where(m => m.Estado == "Activa").ToList();

                    foreach (var mat in matriculasActivas)
                    {
                        var totalMaterias = await _context.Materias
                            .CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);

                        var materiasPagadas = await _context.Pagos
                            .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                            p.TipoPago == "Mensualidad" &&
                                            p.Estado == "Completado");

                        var materiasPendientes = totalMaterias - materiasPagadas;

                        if (materiasPendientes > 0)
                        {
                            estudiantesConMora++;
                            montoPendiente += mat.MontoFinal * materiasPendientes;
                        }
                    }

                    var tasaMorosidad = activas > 0
                        ? Math.Round((decimal)estudiantesConMora / activas * 100, 1)
                        : 0;

                    reportePorRed.Add(new
                    {
                        redId = red.RedId,
                        redNombre = red.Nombre,
                        totalEstudiantes = totalEstudiantes,
                        internos = internos,
                        externos = externos,
                        becados = becados,
                        porcentajeBecados = totalEstudiantes > 0
                            ? Math.Round((decimal)becados / totalEstudiantes * 100, 1)
                            : 0,
                        matriculasActivas = activas,
                        matriculasCompletadas = completadas,
                        tasaCompletacion = tasaCompletacion,
                        estudiantesConMora = estudiantesConMora,
                        tasaMorosidad = tasaMorosidad,
                        montoPendiente = montoPendiente
                    });
                }

                // Estudiantes sin red
                var estudiantesSinRed = await _context.Estudiantes
                    .Where(e => e.Activo && e.RedId == null)
                    .ToListAsync();

                if (estudiantesSinRed.Any())
                {
                    var sinRedIds = estudiantesSinRed.Select(e => e.EstudianteId).ToList();
                    var matriculasSinRed = await _context.Matriculas
                        .Where(m => sinRedIds.Contains(m.EstudianteId) && m.Estado == "Activa")
                        .ToListAsync();

                    int morasinRed = 0;
                    foreach (var mat in matriculasSinRed)
                    {
                        var totalMaterias = await _context.Materias
                            .CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);
                        var materiasPagadas = await _context.Pagos
                            .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                            p.TipoPago == "Mensualidad" &&
                                            p.Estado == "Completado");
                        if ((totalMaterias - materiasPagadas) > 0)
                        {
                            morasinRed++;
                        }
                    }

                    reportePorRed.Add(new
                    {
                        redId = (int?)null,
                        redNombre = "Sin Red Asignada",
                        totalEstudiantes = estudiantesSinRed.Count,
                        internos = estudiantesSinRed.Count(e => e.EsInterno),
                        externos = estudiantesSinRed.Count(e => !e.EsInterno),
                        becados = estudiantesSinRed.Count(e => e.EsBecado),
                        porcentajeBecados = 0m,
                        matriculasActivas = matriculasSinRed.Count,
                        matriculasCompletadas = 0,
                        tasaCompletacion = 0m,
                        estudiantesConMora = morasinRed,
                        tasaMorosidad = matriculasSinRed.Count > 0
                            ? Math.Round((decimal)morasinRed / matriculasSinRed.Count * 100, 1)
                            : 0,
                        montoPendiente = 0m
                    });
                }

                // Ordenar por morosidad (mayor a menor) para identificar redes que necesitan atención
                var ranking = reportePorRed
                    .OrderBy(r => ((dynamic)r).tasaMorosidad)
                    .ToList();

                return Ok(new
                {
                    totalRedes = redes.Count,
                    datos = reportePorRed,
                    ranking = new
                    {
                        mejorRed = ranking.FirstOrDefault(),
                        necesitaAtencion = reportePorRed
                            .Where(r => ((dynamic)r).tasaMorosidad > 30)
                            .ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al generar el reporte por red",
                    error = ex.Message
                });
            }
        }

        // GET: api/Reportes/Financiero
        // Reporte financiero detallado con filtros
        [HttpGet("[action]")]
        public async Task<IActionResult> Financiero(
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] string tipoPago = null, // "Matricula" o "Mensualidad"
            [FromQuery] string metodoPago = null, // "Efectivo", "Tarjeta", "Mixto"
            [FromQuery] int? moduloId = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 50)
        {
            try
            {
                // Fechas por defecto: mes actual
                var inicio = fechaInicio?.Date ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var fin = (fechaFin?.Date ?? DateTime.Today).AddDays(1).AddSeconds(-1);

                var query = _context.Pagos
                    .Where(p => p.Estado == "Completado" &&
                               p.FechaPago >= inicio &&
                               p.FechaPago <= fin)
                    .Include(p => p.Matricula)
                        .ThenInclude(m => m.Estudiante)
                    .Include(p => p.Matricula)
                        .ThenInclude(m => m.Modulo)
                    .Include(p => p.Materia)
                    .AsQueryable();

                // Filtros
                if (!string.IsNullOrEmpty(tipoPago))
                {
                    query = query.Where(p => p.TipoPago == tipoPago);
                }

                if (!string.IsNullOrEmpty(metodoPago))
                {
                    query = query.Where(p => p.MetodoPago == metodoPago);
                }

                if (moduloId.HasValue)
                {
                    query = query.Where(p => p.Matricula.ModuloId == moduloId.Value);
                }

                var totalRegistros = await query.CountAsync();

                // Métricas generales (sin paginación)
                var todosPagos = await query.ToListAsync();

                var totalRecaudado = todosPagos.Sum(p => p.MontoFinal);
                var totalCordobas = todosPagos.Sum(p => p.EfectivoCordobas + p.TarjetaCordobas);
                var totalDolares = todosPagos.Sum(p => p.EfectivoDolares + p.TarjetaDolares);
                var totalVueltos = todosPagos.Sum(p => p.Vuelto);

                var porTipo = todosPagos
                    .GroupBy(p => p.TipoPago)
                    .Select(g => new
                    {
                        tipo = g.Key,
                        cantidad = g.Count(),
                        total = g.Sum(p => p.MontoFinal)
                    })
                    .ToList();

                var porMetodo = todosPagos
                    .GroupBy(p => p.MetodoPago)
                    .Select(g => new
                    {
                        metodo = g.Key,
                        cantidad = g.Count(),
                        total = g.Sum(p => p.MontoFinal),
                        porcentaje = totalRecaudado > 0
                            ? Math.Round((g.Sum(p => p.MontoFinal) / totalRecaudado) * 100, 1)
                            : 0
                    })
                    .ToList();

                var porDia = todosPagos
                    .GroupBy(p => p.FechaPago.Date)
                    .Select(g => new
                    {
                        fecha = g.Key.ToString("yyyy-MM-dd"),
                        cantidad = g.Count(),
                        total = g.Sum(p => p.MontoFinal)
                    })
                    .OrderBy(x => x.fecha)
                    .ToList();

                // Datos paginados
                var datosPaginados = await query
                    .OrderByDescending(p => p.FechaPago)
                    .Skip((pagina - 1) * porPagina)
                    .Take(porPagina)
                    .Select(p => new
                    {
                        p.PagoId,
                        p.Codigo,
                        p.FechaPago,
                        estudianteNombre = p.Matricula.Estudiante.NombreCompleto,
                        moduloNombre = p.Matricula.Modulo.Nombre,
                        materiaNombre = p.Materia != null ? p.Materia.Nombre : null,
                        p.TipoPago,
                        p.MetodoPago,
                        p.MontoFinal,
                        p.TotalPagadoUSD,
                        p.Vuelto,
                        p.EfectivoCordobas,
                        p.EfectivoDolares,
                        p.TarjetaCordobas,
                        p.TarjetaDolares
                    })
                    .ToListAsync();

                return Ok(new
                {
                    periodo = new
                    {
                        inicio = inicio.ToString("yyyy-MM-dd"),
                        fin = fin.ToString("yyyy-MM-dd")
                    },
                    resumen = new
                    {
                        totalPagos = totalRegistros,
                        totalRecaudado = totalRecaudado,
                        totalCordobas = totalCordobas,
                        totalDolares = totalDolares,
                        totalVueltos = totalVueltos,
                        promedioTicket = totalRegistros > 0 ? Math.Round(totalRecaudado / totalRegistros, 2) : 0
                    },
                    porTipo = porTipo,
                    porMetodo = porMetodo,
                    tendencia = porDia,
                    pagina = pagina,
                    porPagina = porPagina,
                    totalPaginas = (int)Math.Ceiling((double)totalRegistros / porPagina),
                    datos = datosPaginados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al generar el reporte financiero",
                    error = ex.Message
                });
            }
        }
    }
}
