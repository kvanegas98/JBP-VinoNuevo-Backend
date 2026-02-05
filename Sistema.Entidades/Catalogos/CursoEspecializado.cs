using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class CursoEspecializado
    {
        public int CursoEspecializadoId { get; set; }

        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } // CURSO-2026-001, etc.

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        /// <summary>
        /// Tipo de evaluación que usa este curso
        /// Por defecto: 2 (ESPECIALIZADO - Parcial1 50%, Parcial2 50%)
        /// </summary>
        [Required]
        public int TipoEvaluacionId { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        // Navegación
        public TipoEvaluacion TipoEvaluacion { get; set; }
        public ICollection<Instituto.MatriculaCurso> Matriculas { get; set; }
    }
}
