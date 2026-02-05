using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Notas
{
    /// <summary>
    /// ViewModel para registrar una nota individual por componente
    /// </summary>
    public class RegistrarNotaViewModel
    {
        /// <summary>
        /// MatriculaId para materias académicas (mutuamente excluyente con MatriculaCursoId)
        /// </summary>
        public int? MatriculaId { get; set; }

        /// <summary>
        /// MatriculaCursoId para cursos especializados (mutuamente excluyente con MatriculaId)
        /// </summary>
        public int? MatriculaCursoId { get; set; }

        /// <summary>
        /// MateriaId para evaluaciones académicas por materia específica (opcional)
        /// </summary>
        public int? MateriaId { get; set; }

        /// <summary>
        /// Componente de evaluación (Examen 1, Examen 2, Proyecto, etc.)
        /// </summary>
        [Required(ErrorMessage = "El componente de evaluación es requerido")]
        public int ComponenteEvaluacionId { get; set; }

        /// <summary>
        /// Nota obtenida (0-100, número entero)
        /// </summary>
        [Required(ErrorMessage = "La nota es requerida")]
        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100")]
        public int Nota { get; set; }

        /// <summary>
        /// Observaciones adicionales (opcional)
        /// </summary>
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string Observaciones { get; set; }
    }
}
