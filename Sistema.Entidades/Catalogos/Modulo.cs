using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Catalogos
{
    public class Modulo
    {
        public int ModuloId { get; set; }

        [Required]
        public int AnioLectivoId { get; set; }

        [Required]
        public int Numero { get; set; } // 1, 2, 3

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } // "Módulo 1", "Módulo 2", etc.

        public bool Activo { get; set; }

        // Navegación
        public AnioLectivo AnioLectivo { get; set; }
        public ICollection<Materia> Materias { get; set; }
        public ICollection<Sistema.Entidades.Instituto.Matricula> Matriculas { get; set; }
    }
}
