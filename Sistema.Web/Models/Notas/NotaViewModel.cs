using System;

namespace Sistema.Web.Models.Notas
{
    public class NotaViewModel
    {
        public int NotaId { get; set; }
        public int MatriculaId { get; set; }
        public string EstudianteNombre { get; set; }
        public int MateriaId { get; set; }
        public string MateriaNombre { get; set; }
        public decimal Calificacion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Observaciones { get; set; }
        public int AnioLectivoId { get; set; }
        public string AnioLectivoNombre { get; set; }
    }
}
