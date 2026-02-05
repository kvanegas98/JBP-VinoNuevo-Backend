using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Notas
{
    /// <summary>
    /// ViewModel para actualizar una nota del sistema flexible
    /// </summary>
    public class ActualizarNotaViewModel
    {
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
