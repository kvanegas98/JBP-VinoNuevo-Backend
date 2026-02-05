using System;

namespace Sistema.Web.Models.Notas
{
    /// <summary>
    /// ViewModel para listar notas del nuevo sistema flexible
    /// </summary>
    public class NotaNuevoSistemaViewModel
    {
        public int NotaId { get; set; }

        /// <summary>
        /// MatriculaId si es materia académica
        /// </summary>
        public int? MatriculaId { get; set; }

        /// <summary>
        /// MatriculaCursoId si es curso especializado
        /// </summary>
        public int? MatriculaCursoId { get; set; }

        /// <summary>
        /// Tipo de matrícula: "Académica" o "Curso Especializado"
        /// </summary>
        public string TipoMatricula { get; set; }

        /// <summary>
        /// Código de la matrícula
        /// </summary>
        public string MatriculaCodigo { get; set; }

        /// <summary>
        /// Nombre del programa (materia o curso)
        /// </summary>
        public string ProgramaNombre { get; set; }

        /// <summary>
        /// ID del componente de evaluación
        /// </summary>
        public int ComponenteEvaluacionId { get; set; }

        /// <summary>
        /// Nombre del componente (Examen 1, Proyecto Final, etc.)
        /// </summary>
        public string ComponenteNombre { get; set; }

        /// <summary>
        /// Orden del componente
        /// </summary>
        public int ComponenteOrden { get; set; }

        /// <summary>
        /// Porcentaje de peso del componente
        /// </summary>
        public decimal ComponentePeso { get; set; }

        /// <summary>
        /// Nota obtenida (0-100, entero)
        /// </summary>
        public int Nota { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string Observaciones { get; set; }

        /// <summary>
        /// Usuario que registró la nota
        /// </summary>
        public string UsuarioRegistro { get; set; }
    }
}
