using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Notas
{
    public class ActualizarNotaViewModel
    {
        [Required]
        public int NotaId { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Nota1 debe estar entre 0 y 100")]
        public decimal Nota1 { get; set; }

        [Range(0, 100, ErrorMessage = "Nota2 debe estar entre 0 y 100")]
        public decimal? Nota2 { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }
    }
}
