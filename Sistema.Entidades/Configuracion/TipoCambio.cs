using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Entidades.Configuracion
{
    public class TipoCambio
    {
        public int TipoCambioId { get; set; }

        [Required]
        public decimal TasaCompra { get; set; }  // Cuando el cliente paga en córdobas

        [Required]
        public decimal TasaVenta { get; set; }   // Cuando das vuelto en córdobas

        public DateTime VigenteDesde { get; set; }  // Cuándo empezó a regir

        public DateTime? VigenteHasta { get; set; } // NULL = es el actual vigente

        [Required]
        public int UsuarioId { get; set; }  // Quién lo registró

        public DateTime FechaRegistro { get; set; }  // Cuándo se creó el registro

        [StringLength(500)]
        public string Observaciones { get; set; }  // Opcional: "Ajuste por inflación", etc.

        // Navegación
        public Sistema.Entidades.Usuarios.Usuario Usuario { get; set; }
    }
}
