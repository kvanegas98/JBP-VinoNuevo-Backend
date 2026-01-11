using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class Red
    {
        public int RedId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(7)]
        public string Color { get; set; } // Hexadecimal: #FF5733

        public bool Activo { get; set; }

        public ICollection<Sistema.Entidades.Instituto.Estudiante> Estudiantes { get; set; }
    }
}
