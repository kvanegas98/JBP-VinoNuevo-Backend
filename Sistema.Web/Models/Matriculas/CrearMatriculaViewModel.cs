using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Matriculas
{
    public class CrearMatriculaViewModel
    {
        [Required]
        public int EstudianteId { get; set; }

        [Required]
        public int ModuloId { get; set; }

        [Required]
        public int ModalidadId { get; set; }

        [Required]
        public int CategoriaEstudianteId { get; set; }

        public decimal MontoMatricula { get; set; }

        public decimal DescuentoAplicado { get; set; }

        public decimal MontoFinal { get; set; }
    }
}
