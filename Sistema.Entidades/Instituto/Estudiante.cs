using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sistema.Entidades.Catalogos;

namespace Sistema.Entidades.Instituto
{
    public class Estudiante
    {
        public int EstudianteId { get; set; }

        [StringLength(20)]
        public string Codigo { get; set; } // EST-0001, EST-0002, etc.

        [Required]
        [StringLength(200)]
        public string NombreCompleto { get; set; }

        [StringLength(20)]
        public string Cedula { get; set; }

        [StringLength(100)]
        public string CorreoElectronico { get; set; }

        [StringLength(20)]
        public string Celular { get; set; }

        [StringLength(100)]
        public string Ciudad { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoEstudiante { get; set; } // Nuevo, Regular, Reingreso

        public bool EsInterno { get; set; }

        // Campos de beca
        public bool EsBecado { get; set; }
        public decimal PorcentajeBeca { get; set; } // 0-100, si es 100 = beca completa

        // Campos para estudiantes internos
        public int? RedId { get; set; }

        // Campos para estudiantes externos
        [StringLength(200)]
        public string IglesiaOrigen { get; set; }

        [StringLength(200)]
        public string PastorOrigen { get; set; }

        [StringLength(300)]
        public string DireccionIglesiaOrigen { get; set; }

        [StringLength(20)]
        public string TelefonoIglesiaOrigen { get; set; }

        public bool Activo { get; set; }

        // Navegación
        public Red Red { get; set; }
        public ICollection<EstudianteCargo> EstudianteCargos { get; set; }
        public ICollection<Matricula> Matriculas { get; set; }
    }
}
