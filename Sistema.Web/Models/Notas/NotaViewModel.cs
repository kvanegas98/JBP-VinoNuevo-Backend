using System;

namespace Sistema.Web.Models.Notas
{
    public class NotaViewModel
    {
        public int NotaId { get; set; }
        public int MatriculaId { get; set; }
        public string MatriculaCodigo { get; set; }
        public int EstudianteId { get; set; }
        public string EstudianteCodigo { get; set; }
        public string EstudianteNombre { get; set; }
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; }
        public int MateriaOrden { get; set; }
        public decimal Nota1 { get; set; }
        public decimal Nota2 { get; set; }
        public decimal Promedio { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Observaciones { get; set; }
        public int ModuloId { get; set; }
        public string ModuloNombre { get; set; }
        public int AnioLectivoId { get; set; }
        public string AnioLectivoNombre { get; set; }
        public int? RedId { get; set; }
        public string RedNombre { get; set; }
        public bool EsInterno { get; set; }
        public bool Aprobado { get; set; }
    }
}
