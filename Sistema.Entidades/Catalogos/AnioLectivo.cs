using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class AnioLectivo
    {
        public int AnioLectivoId { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public bool Activo { get; set; }

        // Navegación
        public ICollection<Modulo> Modulos { get; set; }
    }
}
