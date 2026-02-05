using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.MatriculasCurso
{
    public class CrearMatriculaCursoViewModel
    {
        [Required]
        public int EstudianteId { get; set; }

        [Required]
        public int CursoEspecializadoId { get; set; }

        [Required]
        public int ModalidadId { get; set; }

        [Required]
        public int CategoriaEstudianteId { get; set; }

        public decimal MontoMatricula { get; set; }

        public decimal DescuentoAplicado { get; set; }

        public decimal MontoFinal { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }
    }
}
