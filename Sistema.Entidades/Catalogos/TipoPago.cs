using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class TipoPago
    {
        public int TipoPagoId { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        public bool Activo { get; set; }

        public ICollection<Sistema.Entidades.Instituto.Pago> Pagos { get; set; }
    }
}
