using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class Materia
    {
        public int MateriaId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        public int ModuloId { get; set; }

        public bool Activo { get; set; }

        // Navegación
        public Modulo Modulo { get; set; }
        public ICollection<Sistema.Entidades.Instituto.Nota> Notas { get; set; }
        public ICollection<Sistema.Entidades.Instituto.Pago> Pagos { get; set; }
    }
}
