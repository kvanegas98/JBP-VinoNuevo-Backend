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

                // MORA: Solo cuenta materias de meses ANTERIORES no pagadas
                // El mes 1 comienza desde el PAGO DE MATRÍCULA (cuando el estudiante queda inscrito)
                var matriculasActivas = await _context.Matriculas
                    .Where(m => m.Estado == "Activa")
                    .Include(m => m.Modulo)
                    .Include(m => m.Estudiante)
                    .Include(m => m.CategoriaEstudiante)
                    .ToListAsync();

                decimal moraTotal = 0;
                int estudiantesEnMora = 0;
                var hoy = DateTime.Now;

                foreach (var mat in matriculasActivas)
                {
                    // Obtener el pago de matrícula para esta matrícula
                    var pagoMatricula = await _context.Pagos
                        .Where(p => p.MatriculaId == mat.MatriculaId &&
                                   p.TipoPago == "Matricula" &&
                                   p.Estado == "Completado")
                        .OrderBy(p => p.FechaPago)
                        .FirstOrDefaultAsync();

                    // Si no hay pago de matrícula, no hay mora (no está inscrito oficialmente)
                    if (pagoMatricula == null)
                        continue;

                    // Calcular meses transcurridos desde el pago de matrícula
                    // Mes 1 = mes del pago de matrícula, Mes 2 = siguiente mes, etc.
                    int mesesTranscurridos = ((hoy.Year - pagoMatricula.FechaPago.Year) * 12) +
                                            (hoy.Month - pagoMatricula.FechaPago.Month) + 1;

                    // Materias que DEBERÍAN estar pagadas (meses anteriores al actual)
                    // Si estamos en el mes 3, deberían estar pagadas las de orden 1 y 2 (no la 3 porque es el mes actual)
                    int materiasQueDeberianEstarPagadas = mesesTranscurridos - 1;
                    if (materiasQueDeberianEstarPagadas < 0) materiasQueDeberianEstarPagadas = 0;

                    // Si no hay materias que deberían estar pagadas, no hay mora posible
                    if (materiasQueDeberianEstarPagadas <= 0)
                        continue;

                    // Obtener materias del módulo ordenadas
                    var materiasModulo = await _context.Materias
                        .Where(m => m.ModuloId == mat.ModuloId && m.Activo)
                        .OrderBy(m => m.Orden)
                        .Select(m => m.MateriaId)
                        .ToListAsync();

                    // Limitar a las materias que existen en el módulo
                    if (materiasQueDeberianEstarPagadas > materiasModulo.Count)
                        materiasQueDeberianEstarPagadas = materiasModulo.Count;

                    // Cantidad de materias realmente pagadas (sin importar cuáles específicamente)
                    var cantidadMateriasPagadas = await _context.Pagos
                        .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                        p.TipoPago == "Mensualidad" &&
                                        p.Estado == "Completado" &&
                                        p.MateriaId.HasValue);

                    // Materias en mora = materias que deberían estar pagadas - materias pagadas
                    var materiasEnMora = materiasQueDeberianEstarPagadas - cantidadMateriasPagadas;
                    if (materiasEnMora < 0) materiasEnMora = 0;

                    if (materiasEnMora > 0)
                    {
                        estudiantesEnMora++;
                        // Estimar monto de mora usando el monto de mensualidad
                        moraTotal += mat.MontoFinal * materiasEnMora;
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

                // Alerta de mora (calculada arriba con la nueva lógica)
                if (estudiantesEnMora > 0)
                {
                    alertas.Add(new {
                        Tipo = "warning",
                        Mensaje = $"{estudiantesEnMora} estudiante(s) con materias en mora"
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
                        pagosPendientesEstimado = moraTotal,
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
        // MORA: Solo cuenta materias de meses ANTERIORES no pagadas (basado en Orden de materia)
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
                // MORA: Se calcula desde el PAGO DE MATRÍCULA (cuando el estudiante queda inscrito)
                var estudiantesConMora = new System.Collections.Generic.List<object>();
                var hoy = DateTime.Now;

                foreach (var mat in matriculasActivas)
                {
                    // Obtener el pago de matrícula para esta matrícula
                    var pagoMatricula = await _context.Pagos
                        .Where(p => p.MatriculaId == mat.MatriculaId &&
                                   p.TipoPago == "Matricula" &&
                                   p.Estado == "Completado")
                        .OrderBy(p => p.FechaPago)
                        .FirstOrDefaultAsync();

                    // Si no hay pago de matrícula, no hay mora (no está inscrito oficialmente)
                    if (pagoMatricula == null)
                        continue;

                    // Calcular meses transcurridos desde el pago de matrícula
                    // Mes 1 = mes del pago de matrícula, Mes 2 = siguiente mes, etc.
                    int mesesTranscurridos = ((hoy.Year - pagoMatricula.FechaPago.Year) * 12) +
                                            (hoy.Month - pagoMatricula.FechaPago.Month) + 1;

                    // Materias que DEBERÍAN estar pagadas (meses anteriores al actual)
                    int materiasQueDeberianEstarPagadas = mesesTranscurridos - 1;
                    if (materiasQueDeberianEstarPagadas < 0) materiasQueDeberianEstarPagadas = 0;

                    // Si no hay materias que deberían estar pagadas, no hay mora posible
                    if (materiasQueDeberianEstarPagadas <= 0)
                        continue;

                    // Obtener total de materias del módulo
                    var totalMaterias = await _context.Materias
                        .CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);

                    // Limitar a las materias que existen en el módulo
                    if (materiasQueDeberianEstarPagadas > totalMaterias)
                        materiasQueDeberianEstarPagadas = totalMaterias;

                    // Cantidad de materias realmente pagadas (sin importar cuáles específicamente)
                    var cantidadMateriasPagadas = await _context.Pagos
                        .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                        p.TipoPago == "Mensualidad" &&
                                        p.Estado == "Completado" &&
                                        p.MateriaId.HasValue);

                    // Materias en mora = materias que deberían estar pagadas - materias pagadas
                    var materiasEnMora = materiasQueDeberianEstarPagadas - cantidadMateriasPagadas;
                    if (materiasEnMora < 0) materiasEnMora = 0;

                    if (materiasEnMora > 0)
                    {
                        // Calcular severidad basada en cantidad de materias en mora
                        string sev = materiasEnMora == 1 ? "leve" :
                                    materiasEnMora <= 3 ? "moderada" : "grave";

                        // Filtro de severidad
                        if (!string.IsNullOrEmpty(severidad) && sev != severidad.ToLower())
                        {
                            continue;
                        }

                        // Estimar monto de mora usando el monto de mensualidad
                        decimal montoPendiente = mat.MontoFinal * materiasEnMora;

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
                            fechaPagoMatricula = pagoMatricula.FechaPago,
                            mesesTranscurridos = mesesTranscurridos,
                            materiasEnMora = materiasEnMora,
                            materiasPagadas = cantidadMateriasPagadas,
                            materiasQueDeberianEstarPagadas = materiasQueDeberianEstarPagadas,
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

                    // Calcular morosidad desde el PAGO DE MATRÍCULA
                    int estudiantesConMora = 0;
                    decimal montoPendiente = 0;
                    var hoyRed = DateTime.Now;

                    var matriculasActivas = matriculasRed.Where(m => m.Estado == "Activa").ToList();

                    foreach (var mat in matriculasActivas)
                    {
                        // Obtener el pago de matrícula
                        var pagoMatricula = await _context.Pagos
                            .Where(p => p.MatriculaId == mat.MatriculaId &&
                                       p.TipoPago == "Matricula" &&
                                       p.Estado == "Completado")
                            .OrderBy(p => p.FechaPago)
                            .FirstOrDefaultAsync();

                        // Si no hay pago de matrícula, no hay mora
                        if (pagoMatricula == null) continue;

                        // Calcular meses transcurridos desde el pago de matrícula
                        int mesesTranscurridos = ((hoyRed.Year - pagoMatricula.FechaPago.Year) * 12) +
                                                (hoyRed.Month - pagoMatricula.FechaPago.Month) + 1;

                        // Materias que DEBERÍAN estar pagadas (meses anteriores al actual)
                        int materiasQueDeberianEstarPagadas = mesesTranscurridos - 1;
                        if (materiasQueDeberianEstarPagadas <= 0) continue;

                        // Obtener total de materias del módulo
                        var totalMateriasModulo = await _context.Materias
                            .CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);

                        if (materiasQueDeberianEstarPagadas > totalMateriasModulo)
                            materiasQueDeberianEstarPagadas = totalMateriasModulo;

                        // Cantidad de materias realmente pagadas
                        var cantidadPagadas = await _context.Pagos
                            .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                            p.TipoPago == "Mensualidad" &&
                                            p.Estado == "Completado" &&
                                            p.MateriaId.HasValue);

                        var materiasEnMora = materiasQueDeberianEstarPagadas - cantidadPagadas;
                        if (materiasEnMora < 0) materiasEnMora = 0;

                        if (materiasEnMora > 0)
                        {
                            estudiantesConMora++;
                            montoPendiente += mat.MontoFinal * materiasEnMora;
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
                    decimal montoPendienteSinRed = 0;
                    var hoySinRed = DateTime.Now;

                    foreach (var mat in matriculasSinRed)
                    {
                        // Obtener el pago de matrícula
                        var pagoMatricula = await _context.Pagos
                            .Where(p => p.MatriculaId == mat.MatriculaId &&
                                       p.TipoPago == "Matricula" &&
                                       p.Estado == "Completado")
                            .OrderBy(p => p.FechaPago)
                            .FirstOrDefaultAsync();

                        // Si no hay pago de matrícula, no hay mora
                        if (pagoMatricula == null) continue;

                        // Calcular meses transcurridos desde el pago de matrícula
                        int mesesTranscurridos = ((hoySinRed.Year - pagoMatricula.FechaPago.Year) * 12) +
                                                (hoySinRed.Month - pagoMatricula.FechaPago.Month) + 1;

                        int materiasQueDeberianEstarPagadas = mesesTranscurridos - 1;
                        if (materiasQueDeberianEstarPagadas <= 0) continue;

                        var totalMateriasModulo = await _context.Materias
                            .CountAsync(m => m.ModuloId == mat.ModuloId && m.Activo);

                        if (materiasQueDeberianEstarPagadas > totalMateriasModulo)
                            materiasQueDeberianEstarPagadas = totalMateriasModulo;

                        var cantidadPagadas = await _context.Pagos
                            .CountAsync(p => p.MatriculaId == mat.MatriculaId &&
                                            p.TipoPago == "Mensualidad" &&
                                            p.Estado == "Completado" &&
                                            p.MateriaId.HasValue);

                        var materiasEnMora = materiasQueDeberianEstarPagadas - cantidadPagadas;
                        if (materiasEnMora < 0) materiasEnMora = 0;

                        if (materiasEnMora > 0)
                        {
                            morasinRed++;
                            montoPendienteSinRed += mat.MontoFinal * materiasEnMora;
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
                        montoPendiente = montoPendienteSinRed
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

        // GET: api/Reportes/Academico
        // Reporte académico: promedios, aprobación, rendimiento por módulo/materia/red
        [HttpGet("[action]")]
        public async Task<IActionResult> Academico(
            [FromQuery] int? anioLectivoId = null,
            [FromQuery] int? moduloId = null,
            [FromQuery] int? materiaId = null,
            [FromQuery] int? redId = null,
            [FromQuery] bool? esInterno = null)
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

                // Consulta base de notas
                var queryNotas = _context.Notas
                    .Include(n => n.Matricula)
                        .ThenInclude(m => m.Estudiante)
                            .ThenInclude(e => e.Red)
                    .Include(n => n.Matricula)
                        .ThenInclude(m => m.Modulo)
                            .ThenInclude(mod => mod.AnioLectivo)
                    .Include(n => n.Materia)
                    .AsQueryable();

                // Filtrar por año lectivo solo si se especificó o encontró uno
                if (anioLectivoId.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.Modulo.AnioLectivoId == anioLectivoId.Value);
                }

                // Filtros
                if (moduloId.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.ModuloId == moduloId.Value);
                }

                if (materiaId.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.MateriaId == materiaId.Value);
                }

                if (redId.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.Estudiante.RedId == redId.Value);
                }

                if (esInterno.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.Estudiante.EsInterno == esInterno.Value);
                }

                var todasLasNotas = await queryNotas.ToListAsync();

                if (!todasLasNotas.Any())
                {
                    return Ok(new
                    {
                        mensaje = "No hay notas registradas con los filtros seleccionados",
                        general = new { promedioGeneral = 0, totalNotas = 0, aprobados = 0, reprobados = 0 },
                        porModulo = new object[] { },
                        porMateria = new object[] { },
                        porRed = new object[] { },
                        comparativoInternoExterno = new { internos = new { }, externos = new { } },
                        topMejores = new object[] { },
                        topMenores = new object[] { }
                    });
                }

                // ========== MÉTRICAS GENERALES ==========
                var promedioGeneral = Math.Round(todasLasNotas.Average(n => n.Promedio), 2);
                var totalNotas = todasLasNotas.Count;
                var aprobados = todasLasNotas.Count(n => n.Promedio >= 70);
                var reprobados = totalNotas - aprobados;
                var porcentajeAprobacion = Math.Round((decimal)aprobados / totalNotas * 100, 1);

                // ========== RENDIMIENTO POR MÓDULO ==========
                var porModulo = todasLasNotas
                    .GroupBy(n => new { n.Matricula.ModuloId, n.Matricula.Modulo.Nombre })
                    .Select(g => new
                    {
                        moduloId = g.Key.ModuloId,
                        moduloNombre = g.Key.Nombre,
                        promedio = Math.Round(g.Average(n => n.Promedio), 2),
                        totalNotas = g.Count(),
                        aprobados = g.Count(n => n.Promedio >= 70),
                        reprobados = g.Count(n => n.Promedio < 70),
                        porcentajeAprobacion = Math.Round((decimal)g.Count(n => n.Promedio >= 70) / g.Count() * 100, 1)
                    })
                    .OrderByDescending(m => m.promedio)
                    .ToList();

                // ========== RENDIMIENTO POR MATERIA ==========
                var porMateria = todasLasNotas
                    .GroupBy(n => new { n.MateriaId, n.Materia.Nombre, n.Materia.Orden })
                    .Select(g => new
                    {
                        materiaId = g.Key.MateriaId,
                        materiaNombre = g.Key.Nombre,
                        orden = g.Key.Orden,
                        promedio = Math.Round(g.Average(n => n.Promedio), 2),
                        totalNotas = g.Count(),
                        aprobados = g.Count(n => n.Promedio >= 70),
                        reprobados = g.Count(n => n.Promedio < 70),
                        porcentajeAprobacion = Math.Round((decimal)g.Count(n => n.Promedio >= 70) / g.Count() * 100, 1),
                        promedioNota1 = Math.Round(g.Average(n => n.Nota1), 2),
                        promedioNota2 = Math.Round(g.Average(n => n.Nota2), 2)
                    })
                    .OrderBy(m => m.orden)
                    .ToList();

                // ========== RENDIMIENTO POR RED ==========
                var porRed = todasLasNotas
                    .GroupBy(n => new { RedId = n.Matricula.Estudiante.RedId, RedNombre = n.Matricula.Estudiante.Red?.Nombre ?? "Sin Red" })
                    .Select(g => new
                    {
                        redId = g.Key.RedId,
                        redNombre = g.Key.RedNombre,
                        promedio = Math.Round(g.Average(n => n.Promedio), 2),
                        totalNotas = g.Count(),
                        aprobados = g.Count(n => n.Promedio >= 70),
                        reprobados = g.Count(n => n.Promedio < 70),
                        porcentajeAprobacion = Math.Round((decimal)g.Count(n => n.Promedio >= 70) / g.Count() * 100, 1)
                    })
                    .OrderByDescending(r => r.promedio)
                    .ToList();

                // ========== COMPARATIVO INTERNOS VS EXTERNOS ==========
                var notasInternos = todasLasNotas.Where(n => n.Matricula.Estudiante.EsInterno).ToList();
                var notasExternos = todasLasNotas.Where(n => !n.Matricula.Estudiante.EsInterno).ToList();

                var comparativoInternoExterno = new
                {
                    internos = new
                    {
                        totalNotas = notasInternos.Count,
                        promedio = notasInternos.Any() ? Math.Round(notasInternos.Average(n => n.Promedio), 2) : 0,
                        aprobados = notasInternos.Count(n => n.Promedio >= 70),
                        reprobados = notasInternos.Count(n => n.Promedio < 70),
                        porcentajeAprobacion = notasInternos.Any()
                            ? Math.Round((decimal)notasInternos.Count(n => n.Promedio >= 70) / notasInternos.Count * 100, 1)
                            : 0
                    },
                    externos = new
                    {
                        totalNotas = notasExternos.Count,
                        promedio = notasExternos.Any() ? Math.Round(notasExternos.Average(n => n.Promedio), 2) : 0,
                        aprobados = notasExternos.Count(n => n.Promedio >= 70),
                        reprobados = notasExternos.Count(n => n.Promedio < 70),
                        porcentajeAprobacion = notasExternos.Any()
                            ? Math.Round((decimal)notasExternos.Count(n => n.Promedio >= 70) / notasExternos.Count * 100, 1)
                            : 0
                    }
                };

                // ========== TOP 10 MEJORES ESTUDIANTES ==========
                var promediosPorEstudiante = todasLasNotas
                    .GroupBy(n => new {
                        n.Matricula.EstudianteId,
                        n.Matricula.Estudiante.Codigo,
                        n.Matricula.Estudiante.NombreCompleto,
                        n.Matricula.Estudiante.EsInterno,
                        RedNombre = n.Matricula.Estudiante.Red?.Nombre ?? "Sin Red"
                    })
                    .Select(g => new
                    {
                        estudianteId = g.Key.EstudianteId,
                        estudianteCodigo = g.Key.Codigo,
                        estudianteNombre = g.Key.NombreCompleto,
                        esInterno = g.Key.EsInterno,
                        redNombre = g.Key.RedNombre,
                        promedio = Math.Round(g.Average(n => n.Promedio), 2),
                        materiasEvaluadas = g.Count(),
                        materiasAprobadas = g.Count(n => n.Promedio >= 70)
                    })
                    .ToList();

                var topMejores = promediosPorEstudiante
                    .OrderByDescending(e => e.promedio)
                    .ThenByDescending(e => e.materiasAprobadas)
                    .Take(10)
                    .ToList();

                var topMenores = promediosPorEstudiante
                    .OrderBy(e => e.promedio)
                    .Take(10)
                    .ToList();

                return Ok(new
                {
                    filtrosAplicados = new
                    {
                        anioLectivoId = anioLectivoId,
                        moduloId = moduloId,
                        materiaId = materiaId,
                        redId = redId,
                        esInterno = esInterno
                    },
                    general = new
                    {
                        promedioGeneral = promedioGeneral,
                        totalNotas = totalNotas,
                        aprobados = aprobados,
                        reprobados = reprobados,
                        porcentajeAprobacion = porcentajeAprobacion,
                        porcentajeReprobacion = 100 - porcentajeAprobacion
                    },
                    porModulo = porModulo,
                    porMateria = porMateria,
                    porRed = porRed,
                    comparativoInternoExterno = comparativoInternoExterno,
                    topMejores = topMejores,
                    topMenores = topMenores
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al generar el reporte académico",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/Reportes/Ranking
        // Ranking de estudiantes con paginación y filtros
        [HttpGet("[action]")]
        public async Task<IActionResult> Ranking(
            [FromQuery] int? anioLectivoId = null,
            [FromQuery] int? moduloId = null,
            [FromQuery] int? redId = null,
            [FromQuery] bool? esInterno = null,
            [FromQuery] bool? soloAprobados = null,
            [FromQuery] string orden = "desc", // "desc" = mejores primero, "asc" = menores primero
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
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

                // Consulta base de notas
                var queryNotas = _context.Notas
                    .Include(n => n.Matricula)
                        .ThenInclude(m => m.Estudiante)
                            .ThenInclude(e => e.Red)
                    .Include(n => n.Matricula)
                        .ThenInclude(m => m.Modulo)
                            .ThenInclude(mod => mod.AnioLectivo)
                    .Include(n => n.Materia)
                    .AsQueryable();

                // Filtrar por año lectivo solo si se especificó o encontró uno
                if (anioLectivoId.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.Modulo.AnioLectivoId == anioLectivoId.Value);
                }

                // Filtros
                if (moduloId.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.ModuloId == moduloId.Value);
                }

                if (redId.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.Estudiante.RedId == redId.Value);
                }

                if (esInterno.HasValue)
                {
                    queryNotas = queryNotas.Where(n => n.Matricula.Estudiante.EsInterno == esInterno.Value);
                }

                var todasLasNotas = await queryNotas.ToListAsync();

                if (!todasLasNotas.Any())
                {
                    return Ok(new
                    {
                        mensaje = "No hay notas registradas con los filtros seleccionados",
                        total = 0,
                        pagina = pagina,
                        porPagina = porPagina,
                        totalPaginas = 0,
                        datos = new object[] { }
                    });
                }

                // Agrupar por estudiante y calcular promedios
                var rankingCompleto = todasLasNotas
                    .GroupBy(n => new {
                        n.Matricula.EstudianteId,
                        n.Matricula.Estudiante.Codigo,
                        n.Matricula.Estudiante.NombreCompleto,
                        n.Matricula.Estudiante.EsInterno,
                        RedId = n.Matricula.Estudiante.RedId,
                        RedNombre = n.Matricula.Estudiante.Red?.Nombre ?? "Sin Red",
                        ModuloNombre = n.Matricula.Modulo.Nombre
                    })
                    .Select(g => new
                    {
                        estudianteId = g.Key.EstudianteId,
                        estudianteCodigo = g.Key.Codigo,
                        estudianteNombre = g.Key.NombreCompleto,
                        esInterno = g.Key.EsInterno,
                        redId = g.Key.RedId,
                        redNombre = g.Key.RedNombre,
                        moduloNombre = g.Key.ModuloNombre,
                        promedio = Math.Round(g.Average(n => n.Promedio), 2),
                        materiasEvaluadas = g.Count(),
                        materiasAprobadas = g.Count(n => n.Promedio >= 70),
                        materiasReprobadas = g.Count(n => n.Promedio < 70),
                        porcentajeAprobacion = Math.Round((decimal)g.Count(n => n.Promedio >= 70) / g.Count() * 100, 1),
                        promedioNota1 = Math.Round(g.Average(n => n.Nota1), 2),
                        promedioNota2 = Math.Round(g.Average(n => n.Nota2), 2),
                        notaMasAlta = Math.Round(g.Max(n => n.Promedio), 2),
                        notaMasBaja = Math.Round(g.Min(n => n.Promedio), 2),
                        aprobado = g.Average(n => n.Promedio) >= 70
                    })
                    .ToList();

                // Filtro de aprobados
                if (soloAprobados.HasValue)
                {
                    rankingCompleto = rankingCompleto.Where(e => e.aprobado == soloAprobados.Value).ToList();
                }

                // Ordenar
                var rankingOrdenado = orden?.ToLower() == "asc"
                    ? rankingCompleto.OrderBy(e => e.promedio).ThenBy(e => e.materiasAprobadas).ToList()
                    : rankingCompleto.OrderByDescending(e => e.promedio).ThenByDescending(e => e.materiasAprobadas).ToList();

                // Agregar posición en ranking
                var rankingConPosicion = rankingOrdenado
                    .Select((e, index) => new
                    {
                        posicion = index + 1,
                        e.estudianteId,
                        e.estudianteCodigo,
                        e.estudianteNombre,
                        e.esInterno,
                        e.redId,
                        e.redNombre,
                        e.moduloNombre,
                        e.promedio,
                        e.materiasEvaluadas,
                        e.materiasAprobadas,
                        e.materiasReprobadas,
                        e.porcentajeAprobacion,
                        e.promedioNota1,
                        e.promedioNota2,
                        e.notaMasAlta,
                        e.notaMasBaja,
                        e.aprobado
                    })
                    .ToList();

                // Paginación
                var total = rankingConPosicion.Count;
                var datosPaginados = rankingConPosicion
                    .Skip((pagina - 1) * porPagina)
                    .Take(porPagina)
                    .ToList();

                // Resumen
                var promedioGeneral = rankingCompleto.Any() ? Math.Round(rankingCompleto.Average(e => e.promedio), 2) : 0;
                var totalAprobados = rankingCompleto.Count(e => e.aprobado);
                var totalReprobados = rankingCompleto.Count(e => !e.aprobado);

                return Ok(new
                {
                    filtrosAplicados = new
                    {
                        anioLectivoId = anioLectivoId,
                        moduloId = moduloId,
                        redId = redId,
                        esInterno = esInterno,
                        soloAprobados = soloAprobados,
                        orden = orden
                    },
                    resumen = new
                    {
                        totalEstudiantes = total,
                        promedioGeneral = promedioGeneral,
                        aprobados = totalAprobados,
                        reprobados = totalReprobados,
                        porcentajeAprobacion = total > 0 ? Math.Round((decimal)totalAprobados / total * 100, 1) : 0
                    },
                    total = total,
                    pagina = pagina,
                    porPagina = porPagina,
                    totalPaginas = (int)Math.Ceiling((double)total / porPagina),
                    datos = datosPaginados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al generar el ranking de estudiantes",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
