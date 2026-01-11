using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sistema.Entidades.Catalogos;

namespace Sistema.Entidades.Instituto
{
    public class Matricula
    {
        public int MatriculaId { get; set; }

        [StringLength(20)]
        public string Codigo { get; set; } // MAT-2025-0001, MAT-2025-0002, etc.

        [Required]
        public int EstudianteId { get; set; }

        [Required]
        public int ModuloId { get; set; }

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

        // Navegación
        public Estudiante Estudiante { get; set; }
        public Catalogos.Modulo Modulo { get; set; }
        public Modalidad Modalidad { get; set; }
        public CategoriaEstudiante CategoriaEstudiante { get; set; }
        public ICollection<Nota> Notas { get; set; }
        public ICollection<Pago> Pagos { get; set; }
    }
}
