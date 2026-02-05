using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Notas
{
    /// <summary>
    /// ViewModel para actualizar notas del sistema legacy (Nota1, Nota2)
    /// </summary>
    public class ActualizarNotaLegacyViewModel
    {
        [Required]
        public int NotaId { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Nota1 debe estar entre 0 y 100")]
        public int Nota1 { get; set; }

        [Range(0, 100, ErrorMessage = "Nota2 debe estar entre 0 y 100")]
        public int? Nota2 { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }
    }
}
