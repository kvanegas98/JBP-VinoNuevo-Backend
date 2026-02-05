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

        /// <summary>
        /// Orden de la materia dentro del módulo (1 = primer mes, 2 = segundo mes, etc.)
        /// </summary>
        public int Orden { get; set; }

        /// <summary>
        /// Tipo de evaluación que usa esta materia
        /// Por defecto: 1 (REGULAR - Examen1 40%, Examen2 40%, Proyecto 20%)
        /// </summary>
        [Required]
        public int TipoEvaluacionId { get; set; }

        public bool Activo { get; set; }

        // Navegación
        public Modulo Modulo { get; set; }
        public TipoEvaluacion TipoEvaluacion { get; set; }
        public ICollection<Sistema.Entidades.Instituto.Nota> Notas { get; set; }
        public ICollection<Sistema.Entidades.Instituto.Pago> Pagos { get; set; }
    }
}
