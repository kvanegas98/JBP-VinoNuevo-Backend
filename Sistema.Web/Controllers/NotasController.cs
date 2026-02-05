using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Datos;
using Sistema.Entidades.Instituto;
using Sistema.Web.Models.Notas;

namespace Sistema.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotasController : ControllerBase
    {
        private readonly DbContextSistema _context;

        public NotasController(DbContextSistema context)
        {
            _context = context;
        }

        // GET: api/Notas/Listar
        // Filtros opcionales: anioLectivoId, moduloId, materiaId, redId, aprobado, esInterno, busqueda
        // Paginación: pagina (1-based), registrosPorPagina
        [HttpGet("[action]")]
        public async Task<IActionResult> Listar(
            [FromQuery] int? anioLectivoId = null,
            [FromQuery] int? moduloId = null,
            [FromQuery] int? materiaId = null,
            [FromQuery] int? redId = null,
            [FromQuery] bool? aprobado = null,
            [FromQuery] bool? esInterno = null,
            [FromQuery] string busqueda = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10)
        {
            var query = _context.Notas
                .Include(n => n.Matricula)
                    .ThenInclude(m => m.Estudiante)
                        .ThenInclude(e => e.Red)
                .Include(n => n.Matricula)
                    .ThenInclude(m => m.Modulo)
                        .ThenInclude(mod => mod.AnioLectivo)
                .Include(n => n.Materia)
                .AsQueryable();

            // Filtro por año lectivo
            if (anioLectivoId.HasValue)
            {
                query = query.Where(n => n.Matricula.Modulo.AnioLectivoId == anioLectivoId.Value);
            }

            // Filtro por módulo
            if (moduloId.HasValue)
            {
                query = query.Where(n => n.Matricula.ModuloId == moduloId.Value);
            }

            // Filtro por materia
            if (materiaId.HasValue)
            {
                query = query.Where(n => n.MateriaId == materiaId.Value);
            }

            // Filtro por red
            if (redId.HasValue)
            {
                if (redId.Value == 0)
                {
                    // Sin red
                    query = query.Where(n => n.Matricula.Estudiante.RedId == null);
                }
                else
                {
                    query = query.Where(n => n.Matricula.Estudiante.RedId == redId.Value);
                }
            }

            // Filtro por aprobado/reprobado (promedio >= 70)
            if (aprobado.HasValue)
            {
                if (aprobado.Value)
                {
                    query = query.Where(n => n.Promedio >= 70);
                }
                else
                {
                    query = query.Where(n => n.Promedio < 70);
                }
            }

            // Filtro por interno/externo
            if (esInterno.HasValue)
            {
                query = query.Where(n => n.Matricula.Estudiante.EsInterno == esInterno.Value);
            }

            // Filtro por búsqueda (nombre o código del estudiante)
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(n =>
                    n.Matricula.Estudiante.NombreCompleto.ToLower().Contains(busqueda) ||
                    n.Matricula.Estudiante.Codigo.ToLower().Contains(busqueda) ||
                    n.Matricula.Codigo.ToLower().Contains(busqueda));
            }

            // Total de registros para la paginación
            var totalRegistros = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

            // Validar página
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var notas = await query
                .OrderBy(n => n.Matricula.Modulo.AnioLectivoId)
                .ThenBy(n => n.Matricula.Modulo.Numero)
                .ThenBy(n => n.Materia.Orden)
                .ThenBy(n => n.Matricula.Estudiante.NombreCompleto)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(n => new NotaViewModel
                {
                    NotaId = n.NotaId,
                    MatriculaId = n.MatriculaId,
                    MatriculaCodigo = n.Matricula.Codigo,
                    EstudianteId = n.Matricula.EstudianteId,
                    EstudianteCodigo = n.Matricula.Estudiante.Codigo,
                    EstudianteNombre = n.Matricula.Estudiante.NombreCompleto,
                    MateriaId = n.MateriaId,
                    MateriaNombre = n.Materia.Nombre,
                    MateriaOrden = n.Materia.Orden,
                    Nota1 = n.Nota1,
                    Nota2 = n.Nota2,
                    Promedio = n.Promedio,
                    FechaRegistro = n.FechaRegistro,
                    Observaciones = n.Observaciones,
                    ModuloId = n.Matricula.ModuloId,
                    ModuloNombre = n.Matricula.Modulo.Nombre,
                    AnioLectivoId = n.Matricula.Modulo.AnioLectivoId,
                    AnioLectivoNombre = n.Matricula.Modulo.AnioLectivo.Nombre,
                    RedId = n.Matricula.Estudiante.RedId,
                    RedNombre = n.Matricula.Estudiante.Red != null ? n.Matricula.Estudiante.Red.Nombre : null,
                    EsInterno = n.Matricula.Estudiante.EsInterno,
                    Aprobado = n.Promedio >= 70
                })
                .ToListAsync();

            return Ok(new
            {
                notas,
                paginacion = new
                {
                    paginaActual = pagina,
                    registrosPorPagina,
                    totalRegistros,
                    totalPaginas
                }
            });
        }

        // POST: api/Notas/Crear
        [HttpPost("[action]")]
        public async Task<IActionResult> Crear([FromBody] CrearNotaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validación adicional de rango (por si el cliente no valida)
                if (model.Nota1 < 0 || model.Nota1 > 100)
                {
                    return BadRequest(new { message = "Primer Parcial debe estar entre 0 y 100" });
                }

                if (model.Nota2.HasValue && (model.Nota2.Value < 0 || model.Nota2.Value > 100))
                {
                    return BadRequest(new { message = "Segundo Parcial debe estar entre 0 y 100" });
                }

                // Verificar que la matrícula existe y está activa o completada
                var matricula = await _context.Matriculas
                    .FirstOrDefaultAsync(m => m.MatriculaId == model.MatriculaId &&
                        (m.Estado == "Activa" || m.Estado == "Completada"));

                if (matricula == null)
                {
                    return BadRequest(new { message = "Matrícula no encontrada o tiene un estado inválido (debe ser Activa o Completada)" });
                }

                // Verificar que la materia pertenece al módulo de la matrícula
                var materia = await _context.Materias
                    .FirstOrDefaultAsync(m => m.MateriaId == model.MateriaId && m.ModuloId == matricula.ModuloId);

                if (materia == null)
                {
                    return BadRequest(new { message = "La materia no pertenece al módulo de la matrícula" });
                }

                // Verificar que no exista ya una nota para esta matrícula y materia
                var notaExistente = await _context.Notas
                    .AnyAsync(n => n.MatriculaId == model.MatriculaId && n.MateriaId == model.MateriaId);

                if (notaExistente)
                {
                    return BadRequest(new { message = "Ya existe una nota registrada para esta matrícula y materia" });
                }

                // Verificar pagos o beca
                // Cargar estudiante para verificar si es becado
                var matriculaConEstudiante = await _context.Matriculas
                    .Include(m => m.Estudiante)
                    .FirstOrDefaultAsync(m => m.MatriculaId == model.MatriculaId);

                // Si el estudiante NO es becado al 100%, verificar que haya pagado
                if (!matriculaConEstudiante.Estudiante.EsBecado ||
                    matriculaConEstudiante.Estudiante.PorcentajeBeca < 100)
                {
                    var pagoMateria = await _context.Pagos
                        .AnyAsync(p => p.MatriculaId == model.MatriculaId &&
                                       p.MateriaId == model.MateriaId &&
                                       p.TipoPago == "Mensualidad" &&
                                       p.Estado == "Completado");

                    if (!pagoMateria)
                    {
                        return BadRequest(new { message = $"No se puede registrar la nota. El estudiante no ha pagado la materia '{materia.Nombre}'" });
                    }
                }

                // Calcular el promedio (si Nota2 no existe, promedio = Nota1)
                int nota2Value = model.Nota2 ?? 0;
                int promedio = model.Nota2.HasValue
                    ? (int)Math.Round((model.Nota1 + nota2Value) / 2.0, 0)
                    : model.Nota1;

                var nota = new Nota
                {
                    MatriculaId = model.MatriculaId,
                    MateriaId = model.MateriaId,
                    Nota1 = model.Nota1,
                    Nota2 = nota2Value,
                    Promedio = promedio,
                    FechaRegistro = DateTime.Now,
                    Observaciones = model.Observaciones
                };

                _context.Notas.Add(nota);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    notaId = nota.NotaId,
                    nota1 = nota.Nota1,
                    nota2 = nota.Nota2,
                    promedio = nota.Promedio,
                    message = "Nota registrada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al crear la nota",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // PUT: api/Notas/Actualizar
        [HttpPut("[action]")]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarNotaLegacyViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validación adicional de rango
                if (model.Nota1 < 0 || model.Nota1 > 100)
                {
                    return BadRequest(new { message = "Primer Parcial debe estar entre 0 y 100" });
                }

                if (model.Nota2.HasValue && (model.Nota2.Value < 0 || model.Nota2.Value > 100))
                {
                    return BadRequest(new { message = "Segundo Parcial debe estar entre 0 y 100" });
                }

                var nota = await _context.Notas.FindAsync(model.NotaId);

                if (nota == null)
                {
                    return NotFound(new { message = "Nota no encontrada" });
                }

                // Actualizar notas y recalcular promedio
                int nota2Value = model.Nota2 ?? 0;
                nota.Nota1 = model.Nota1;
                nota.Nota2 = nota2Value;
                nota.Promedio = model.Nota2.HasValue
                    ? (int)Math.Round((model.Nota1 + nota2Value) / 2.0, 0)
                    : model.Nota1;
                nota.Observaciones = model.Observaciones;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    notaId = nota.NotaId,
                    nota1 = nota.Nota1,
                    nota2 = nota.Nota2,
                    promedio = nota.Promedio,
                    message = "Nota actualizada exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al actualizar la nota",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // GET: api/Notas/Descargar
        // Exporta notas a CSV, Excel o PDF con los mismos filtros que Listar
        // formato: csv, xlsx, pdf (default: xlsx)
        [HttpGet("[action]")]
        public async Task<IActionResult> Descargar(
            [FromQuery] int? anioLectivoId = null,
            [FromQuery] int? moduloId = null,
            [FromQuery] int? materiaId = null,
            [FromQuery] int? redId = null,
            [FromQuery] bool? aprobado = null,
            [FromQuery] bool? esInterno = null,
            [FromQuery] string busqueda = null,
            [FromQuery] string formato = "xlsx")
        {
            var query = _context.Notas
                .Include(n => n.Matricula)
                    .ThenInclude(m => m.Estudiante)
                        .ThenInclude(e => e.Red)
                .Include(n => n.Matricula)
                    .ThenInclude(m => m.Modulo)
                        .ThenInclude(mod => mod.AnioLectivo)
                .Include(n => n.Materia)
                .AsQueryable();

            // Filtro por año lectivo
            if (anioLectivoId.HasValue)
            {
                query = query.Where(n => n.Matricula.Modulo.AnioLectivoId == anioLectivoId.Value);
            }

            // Filtro por módulo
            if (moduloId.HasValue)
            {
                query = query.Where(n => n.Matricula.ModuloId == moduloId.Value);
            }

            // Filtro por materia
            if (materiaId.HasValue)
            {
                query = query.Where(n => n.MateriaId == materiaId.Value);
            }

            // Filtro por red
            if (redId.HasValue)
            {
                if (redId.Value == 0)
                {
                    query = query.Where(n => n.Matricula.Estudiante.RedId == null);
                }
                else
                {
                    query = query.Where(n => n.Matricula.Estudiante.RedId == redId.Value);
                }
            }

            // Filtro por aprobado/reprobado
            if (aprobado.HasValue)
            {
                if (aprobado.Value)
                {
                    query = query.Where(n => n.Promedio >= 70);
                }
                else
                {
                    query = query.Where(n => n.Promedio < 70);
                }
            }

            // Filtro por interno/externo
            if (esInterno.HasValue)
            {
                query = query.Where(n => n.Matricula.Estudiante.EsInterno == esInterno.Value);
            }

            // Filtro por búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(n =>
                    n.Matricula.Estudiante.NombreCompleto.ToLower().Contains(busqueda) ||
                    n.Matricula.Estudiante.Codigo.ToLower().Contains(busqueda) ||
                    n.Matricula.Codigo.ToLower().Contains(busqueda));
            }

            var notas = await query
                .OrderBy(n => n.Matricula.Modulo.AnioLectivoId)
                .ThenBy(n => n.Matricula.Modulo.Numero)
                .ThenBy(n => n.Materia.Orden)
                .ThenBy(n => n.Matricula.Estudiante.NombreCompleto)
                .Select(n => new
                {
                    AnioLectivo = n.Matricula.Modulo.AnioLectivo.Nombre,
                    Modulo = n.Matricula.Modulo.Nombre,
                    EstudianteCodigo = n.Matricula.Estudiante.Codigo,
                    EstudianteNombre = n.Matricula.Estudiante.NombreCompleto,
                    Red = n.Matricula.Estudiante.Red != null ? n.Matricula.Estudiante.Red.Nombre : "Sin Red",
                    Tipo = n.Matricula.Estudiante.EsInterno ? "Interno" : "Externo",
                    Materia = n.Materia.Nombre,
                    n.Nota1,
                    n.Nota2,
                    n.Promedio,
                    Estado = n.Promedio >= 70 ? "Aprobado" : "Reprobado",
                    FechaRegistro = n.FechaRegistro,
                    n.Observaciones
                })
                .ToListAsync();

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            switch (formato.ToLower())
            {
                case "pdf":
                    return GenerarPdf(notas, timestamp);
                case "csv":
                    return GenerarCsv(notas, timestamp);
                case "xlsx":
                default:
                    return GenerarExcel(notas, timestamp);
            }
        }

        private IActionResult GenerarCsv<T>(System.Collections.Generic.List<T> notas, string timestamp) where T : class
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Año Lectivo,Módulo,Código,Nombre,Red,Tipo,Materia,Primer Parcial,Segundo Parcial,Promedio,Estado,Fecha Registro,Observaciones");

            foreach (dynamic n in notas)
            {
                csv.AppendLine($"\"{n.AnioLectivo}\",\"{n.Modulo}\",\"{n.EstudianteCodigo}\",\"{n.EstudianteNombre}\",\"{n.Red}\",\"{n.Tipo}\",\"{n.Materia}\",{n.Nota1},{n.Nota2},{n.Promedio},\"{n.Estado}\",\"{((DateTime)n.FechaRegistro).ToString("dd/MM/yyyy")}\",\"{n.Observaciones?.Replace("\"", "\"\"")}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Notas_{timestamp}.csv");
        }

        private IActionResult GenerarExcel<T>(System.Collections.Generic.List<T> notas, string timestamp) where T : class
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Notas");

                // Encabezados
                var headers = new[] { "Año Lectivo", "Módulo", "Código", "Nombre", "Red", "Tipo", "Materia", "Primer Parcial", "Segundo Parcial", "Promedio", "Estado", "Fecha Registro", "Observaciones" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                }

                // Datos
                int row = 2;
                foreach (dynamic n in notas)
                {
                    worksheet.Cell(row, 1).Value = (string)n.AnioLectivo;
                    worksheet.Cell(row, 2).Value = (string)n.Modulo;
                    worksheet.Cell(row, 3).Value = (string)n.EstudianteCodigo;
                    worksheet.Cell(row, 4).Value = (string)n.EstudianteNombre;
                    worksheet.Cell(row, 5).Value = (string)n.Red;
                    worksheet.Cell(row, 6).Value = (string)n.Tipo;
                    worksheet.Cell(row, 7).Value = (string)n.Materia;
                    worksheet.Cell(row, 8).Value = (decimal)n.Nota1;
                    worksheet.Cell(row, 9).Value = (decimal)n.Nota2;
                    worksheet.Cell(row, 10).Value = (decimal)n.Promedio;
                    worksheet.Cell(row, 11).Value = (string)n.Estado;
                    worksheet.Cell(row, 12).Value = ((DateTime)n.FechaRegistro).ToString("dd/MM/yyyy");
                    worksheet.Cell(row, 13).Value = (string)n.Observaciones ?? "";

                    // Color para aprobados/reprobados
                    if ((string)n.Estado == "Reprobado")
                    {
                        worksheet.Cell(row, 10).Style.Font.FontColor = XLColor.Red;
                        worksheet.Cell(row, 11).Style.Font.FontColor = XLColor.Red;
                    }
                    else
                    {
                        worksheet.Cell(row, 10).Style.Font.FontColor = XLColor.Green;
                        worksheet.Cell(row, 11).Style.Font.FontColor = XLColor.Green;
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var bytes = stream.ToArray();
                    return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Notas_{timestamp}.xlsx");
                }
            }
        }

        private IActionResult GenerarPdf<T>(System.Collections.Generic.List<T> notas, string timestamp) where T : class
        {
            using (var stream = new MemoryStream())
            {
                var document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, stream);

                document.Open();

                // Título
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 7);

                var title = new Paragraph("Reporte de Notas", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var fecha = new Paragraph($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}", cellFont);
                fecha.Alignment = Element.ALIGN_CENTER;
                document.Add(fecha);
                document.Add(new Paragraph(" "));

                // Tabla
                var table = new PdfPTable(11) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 8, 8, 7, 15, 8, 6, 12, 6, 6, 6, 7 });

                // Encabezados
                var headers = new[] { "Año", "Módulo", "Código", "Nombre", "Red", "Tipo", "Materia", "P1", "P2", "Prom", "Estado" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(173, 216, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Datos
                foreach (dynamic n in notas)
                {
                    table.AddCell(new PdfPCell(new Phrase((string)n.AnioLectivo, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase((string)n.Modulo, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase((string)n.EstudianteCodigo, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase((string)n.EstudianteNombre, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase((string)n.Red, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase((string)n.Tipo, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase((string)n.Materia, cellFont)));
                    table.AddCell(new PdfPCell(new Phrase(((decimal)n.Nota1).ToString("0.00"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(((decimal)n.Nota2).ToString("0.00"), cellFont)) { HorizontalAlignment = Element.ALIGN_CENTER });

                    var promedioCell = new PdfPCell(new Phrase(((decimal)n.Promedio).ToString("0.00"), cellFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    var estadoCell = new PdfPCell(new Phrase((string)n.Estado, cellFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };

                    if ((string)n.Estado == "Reprobado")
                    {
                        promedioCell.Phrase.Font.Color = BaseColor.Red;
                        estadoCell.Phrase.Font.Color = BaseColor.Red;
                    }
                    else
                    {
                        promedioCell.Phrase.Font.Color = new BaseColor(0, 128, 0);
                        estadoCell.Phrase.Font.Color = new BaseColor(0, 128, 0);
                    }

                    table.AddCell(promedioCell);
                    table.AddCell(estadoCell);
                }

                document.Add(table);

                // Resumen
                document.Add(new Paragraph(" "));
                var totalRegistros = notas.Count;
                var resumen = new Paragraph($"Total de registros: {totalRegistros}", cellFont);
                document.Add(resumen);

                document.Close();

                var bytes = stream.ToArray();
                return File(bytes, "application/pdf", $"Notas_{timestamp}.pdf");
            }
        }

        // DELETE: api/Notas/Eliminar/{id}
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            try
            {
                var nota = await _context.Notas.FindAsync(id);

                if (nota == null)
                {
                    return NotFound(new { message = "Nota no encontrada" });
                }

                _context.Notas.Remove(nota);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Nota eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al eliminar la nota",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
    }
}
