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

        /// <summary>
        /// Primera nota (0-100)
        /// </summary>
        [Range(0, 100)]
        public decimal Nota1 { get; set; }

        /// <summary>
        /// Segunda nota (0-100)
        /// </summary>
        [Range(0, 100)]
        public decimal Nota2 { get; set; }

        /// <summary>
        /// Promedio calculado de Nota1 y Nota2
        /// </summary>
        public decimal Promedio { get; set; }

        public DateTime FechaRegistro { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        // Navegación
        public Matricula Matricula { get; set; }
        public Materia Materia { get; set; }
    }
}
