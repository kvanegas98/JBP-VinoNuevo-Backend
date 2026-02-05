using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Catalogos
{
    // ViewModel para crear un nuevo curso especializado
    public class CrearCursoEspecializadoViewModel
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }
    }

    // ViewModel para actualizar un curso especializado existente
    public class ActualizarCursoEspecializadoViewModel
    {
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public bool Activo { get; set; }
    }
}
