using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Estudiantes
{
    public class CrearEstudianteViewModel
    {
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
        public string TipoEstudiante { get; set; }

        public bool EsInterno { get; set; }

        // Campos de beca
        public bool EsBecado { get; set; }
        public decimal PorcentajeBeca { get; set; }

        // Campos para internos
        public int? RedId { get; set; }
        public List<int> CargosIds { get; set; }

        // Campos para externos
        [StringLength(200)]
        public string IglesiaOrigen { get; set; }

        [StringLength(200)]
        public string PastorOrigen { get; set; }

        [StringLength(300)]
        public string DireccionIglesiaOrigen { get; set; }

        [StringLength(20)]
        public string TelefonoIglesiaOrigen { get; set; }
    }
}
