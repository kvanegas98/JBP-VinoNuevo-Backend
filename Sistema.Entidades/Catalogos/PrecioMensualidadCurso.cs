using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class PrecioMensualidadCurso
    {
        public int PrecioMensualidadCursoId { get; set; }

        [Required]
        public int CategoriaEstudianteId { get; set; }

        // NULL = Externos (sin cargo), ID = Internos con cargo específico
        public int? CargoId { get; set; }

        public decimal Precio { get; set; }

        public bool Activo { get; set; }

        public CategoriaEstudiante CategoriaEstudiante { get; set; }
        public Cargo Cargo { get; set; }
    }
}
