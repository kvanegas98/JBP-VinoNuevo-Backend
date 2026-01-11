using System;

namespace Sistema.Web.Models.Matriculas
{
    public class MatriculaViewModel
    {
        public int MatriculaId { get; set; }
        public string Codigo { get; set; }
        public int EstudianteId { get; set; }
        public string EstudianteCodigo { get; set; }
        public string EstudianteNombre { get; set; }
        public int ModuloId { get; set; }
        public string ModuloNombre { get; set; }
        public int AnioLectivoId { get; set; }
        public string AnioLectivoNombre { get; set; }
        public int ModalidadId { get; set; }
        public string ModalidadNombre { get; set; }
        public int CategoriaEstudianteId { get; set; }
        public string CategoriaEstudianteNombre { get; set; }
        public DateTime FechaMatricula { get; set; }
        public decimal MontoMatricula { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public decimal MontoFinal { get; set; }
        public string Estado { get; set; }
    }
}
