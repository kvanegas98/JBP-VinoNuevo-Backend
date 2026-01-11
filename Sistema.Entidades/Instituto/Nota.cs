using System;
using System.ComponentModel.DataAnnotations;
using Sistema.Entidades.Catalogos;

namespace Sistema.Entidades.Instituto
{
    public class Nota
    {
        public int NotaId { get; set; }

        [Required]
        public int MatriculaId { get; set; }

        [Required]
        public int MateriaId { get; set; }

        public decimal Calificacion { get; set; }

        public DateTime FechaRegistro { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        // Navegación
        public Matricula Matricula { get; set; }
        public Materia Materia { get; set; }
    }
}
