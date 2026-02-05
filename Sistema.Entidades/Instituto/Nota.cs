using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sistema.Entidades.Catalogos;
using Sistema.Entidades.Usuarios;

namespace Sistema.Entidades.Instituto
{
    /// <summary>
    /// Tabla de notas que soporta dos sistemas:
    /// - Sistema Legacy: Usa Nota1, Nota2, Promedio (campos fijos)
    /// - Sistema Nuevo: Usa ComponenteEvaluacionId + Nota (flexible)
    /// </summary>
    public class Nota
    {
        public int NotaId { get; set; }

        // ============================================
        // CAMPOS COMPARTIDOS
        // ============================================

        public DateTime FechaRegistro { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        // ============================================
        // CAMPOS LEGACY (para datos históricos)
        // ============================================

        /// <summary>
        /// MatriculaId para sistema académico (nullable en sistema nuevo)
        /// </summary>
        public int? MatriculaId { get; set; }

        /// <summary>
        /// MateriaId (solo se usa en sistema legacy)
        /// </summary>
        public int? MateriaId { get; set; }

        /// <summary>
        /// Primera nota del sistema legacy (0-100, entero)
        /// </summary>
        [Range(0, 100)]
        public int? Nota1 { get; set; }

        /// <summary>
        /// Segunda nota del sistema legacy (0-100, entero)
        /// </summary>
        [Range(0, 100)]
        public int? Nota2 { get; set; }

        /// <summary>
        /// Promedio calculado del sistema legacy (0-100, entero redondeado)
        /// </summary>
        public int? Promedio { get; set; }

        // ============================================
        // CAMPOS NUEVO SISTEMA (evaluación flexible)
        // ============================================

        /// <summary>
        /// MatriculaCursoId para cursos especializados
        /// </summary>
        public int? MatriculaCursoId { get; set; }

        /// <summary>
        /// Componente de evaluación (Examen 1, Examen 2, Proyecto, etc.)
        /// Si es NOT NULL, es una nota del sistema nuevo
        /// </summary>
        public int? ComponenteEvaluacionId { get; set; }

        /// <summary>
        /// Nota individual por componente (0-100, entero)
        /// </summary>
        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100")]
        [Column("Nota")]
        public int? NotaValor { get; set; }

        /// <summary>
        /// Usuario que registró la nota
        /// </summary>
        public int? UsuarioRegistroId { get; set; }

        // ============================================
        // PROPIEDADES DE NAVEGACIÓN
        // ============================================

        // Sistema académico
        public Matricula Matricula { get; set; }
        public Materia Materia { get; set; }

        // Sistema de cursos especializados
        public MatriculaCurso MatriculaCurso { get; set; }

        // Sistema nuevo de evaluación
        public ComponenteEvaluacion ComponenteEvaluacion { get; set; }
        public Usuario Usuario { get; set; }

        // ============================================
        // MÉTODOS AUXILIARES
        // ============================================

        /// <summary>
        /// Indica si esta nota pertenece al nuevo sistema flexible
        /// </summary>
        public bool EsNotaNuevoSistema => ComponenteEvaluacionId.HasValue;

        /// <summary>
        /// Indica si esta nota pertenece al sistema legacy
        /// </summary>
        public bool EsNotaLegacy => !ComponenteEvaluacionId.HasValue;
    }
}
