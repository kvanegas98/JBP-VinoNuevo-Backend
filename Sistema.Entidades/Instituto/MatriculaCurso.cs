using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sistema.Entidades.Catalogos;

namespace Sistema.Entidades.Instituto
{
    public class MatriculaCurso
    {
        public int MatriculaCursoId { get; set; }

        [StringLength(20)]
        public string Codigo { get; set; } // MCURSO-2026-0001, etc.

        [Required]
        public int EstudianteId { get; set; }

        [Required]
        public int CursoEspecializadoId { get; set; }

        [Required]
        public int ModalidadId { get; set; }

        [Required]
        public int CategoriaEstudianteId { get; set; }

        public DateTime FechaMatricula { get; set; }

        public decimal MontoMatricula { get; set; }

        public decimal DescuentoAplicado { get; set; }

        public decimal MontoFinal { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } // Pendiente, Activa, Completada, Anulada

        public bool Aprobado { get; set; } // Para controlar si ya aprobó el curso

        [StringLength(500)]
        public string Observaciones { get; set; }

        // Navegación
        public Estudiante Estudiante { get; set; }
        public CursoEspecializado CursoEspecializado { get; set; }
        public Modalidad Modalidad { get; set; }
        public CategoriaEstudiante CategoriaEstudiante { get; set; }
        public ICollection<PagoCurso> Pagos { get; set; }
        public ICollection<Nota> Notas { get; set; }
    }
}
