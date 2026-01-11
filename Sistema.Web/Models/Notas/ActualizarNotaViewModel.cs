using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Notas
{
    public class ActualizarNotaViewModel
    {
        [Required]
        public int NotaId { get; set; }

        [Required]
        public int MatriculaId { get; set; }

        [Required]
        public int MateriaId { get; set; }

        [Required]
        public decimal Calificacion { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }
    }
}
