using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sistema.Entidades.Instituto;

namespace Sistema.Entidades.Catalogos
{
    /// <summary>
    /// Componentes de evaluación configurables por tipo
    /// Ejemplos: Examen 1, Examen 2, Proyecto Final, Nota Parcial 1, etc.
    /// </summary>
    public class ComponenteEvaluacion
    {
        public int ComponenteEvaluacionId { get; set; }

        /// <summary>
        /// Tipo de evaluación al que pertenece este componente
        /// </summary>
        [Required]
        public int TipoEvaluacionId { get; set; }

        /// <summary>
        /// Nombre del componente (Examen 1, Proyecto Final, etc.)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        /// <summary>
        /// Porcentaje de peso en la nota final (0-100)
        /// Ejemplo: 40.00 representa 40%
        /// La suma de todos los componentes de un tipo debe ser 100%
        /// </summary>
        [Required]
        [Range(0, 100)]
        public decimal PorcentajePeso { get; set; }

        /// <summary>
        /// Orden de visualización (1, 2, 3...)
        /// </summary>
        [Required]
        public int Orden { get; set; }

        /// <summary>
        /// Nota mínima requerida para este componente (opcional)
        /// </summary>
        [Range(0, 100)]
        public decimal? NotaMinima { get; set; }

        /// <summary>
        /// Indica si el componente es obligatorio
        /// </summary>
        public bool EsObligatorio { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        // ============================================
        // PROPIEDADES DE NAVEGACIÓN
        // ============================================

        /// <summary>
        /// Tipo de evaluación al que pertenece
        /// </summary>
        public TipoEvaluacion TipoEvaluacion { get; set; }

        /// <summary>
        /// Notas de estudiantes asociadas a este componente
        /// </summary>
        public ICollection<Nota> Notas { get; set; }
    }
}
