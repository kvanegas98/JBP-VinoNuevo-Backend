using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    /// <summary>
    /// Tipos de evaluación disponibles (Regular, Especializado, etc.)
    /// Define cuántos componentes y qué estructura tiene cada tipo de curso
    /// </summary>
    public class TipoEvaluacion
    {
        public int TipoEvaluacionId { get; set; }

        /// <summary>
        /// Código único del tipo (REGULAR, ESPECIALIZADO)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Codigo { get; set; }

        /// <summary>
        /// Nombre descriptivo del tipo de evaluación
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        /// <summary>
        /// Descripción detallada del tipo de evaluación
        /// </summary>
        [StringLength(500)]
        public string Descripcion { get; set; }

        /// <summary>
        /// Cantidad de componentes obligatorios
        /// Ejemplo: 3 para Regular (Examen1, Examen2, Proyecto)
        ///          2 para Especializado (Parcial1, Parcial2)
        /// </summary>
        [Required]
        public int CantidadComponentes { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        // ============================================
        // PROPIEDADES DE NAVEGACIÓN
        // ============================================

        /// <summary>
        /// Componentes de evaluación asociados a este tipo
        /// </summary>
        public ICollection<ComponenteEvaluacion> Componentes { get; set; }

        /// <summary>
        /// Materias académicas que usan este tipo de evaluación
        /// </summary>
        public ICollection<Materia> Materias { get; set; }

        /// <summary>
        /// Cursos especializados que usan este tipo de evaluación
        /// </summary>
        public ICollection<CursoEspecializado> CursosEspecializados { get; set; }
    }
}
