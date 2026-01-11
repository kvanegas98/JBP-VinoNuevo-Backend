using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class Cargo
    {
        public int CargoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        public decimal PorcentajeDescuento { get; set; }

        public bool Activo { get; set; }

        public ICollection<Sistema.Entidades.Instituto.EstudianteCargo> EstudianteCargos { get; set; }
    }
}
