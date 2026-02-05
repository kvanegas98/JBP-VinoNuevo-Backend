using System;
using System.ComponentModel.DataAnnotations;
using Sistema.Entidades.Catalogos;

namespace Sistema.Entidades.Instituto
{
    public class PagoCurso
    {
        public int PagoCursoId { get; set; }

        [StringLength(20)]
        public string Codigo { get; set; } // PCURSO-2026-0001, etc.

        [Required]
        public int MatriculaCursoId { get; set; }

        public int? NumeroMensualidad { get; set; } // NULL = pago matrícula, Número (1,2,3...) = número de mensualidad

        [Required]
        [StringLength(20)]
        public string TipoPago { get; set; } // "Matricula" o "Mensualidad"

        public decimal Monto { get; set; }
        public decimal Descuento { get; set; }
        public decimal MontoFinal { get; set; } // Monto total a pagar en USD (referencia)

        public DateTime FechaPago { get; set; }

        // ========== DETALLE DE FORMAS DE PAGO ==========
        // Pagos en efectivo
        public decimal EfectivoCordobas { get; set; }  // Monto pagado en efectivo córdobas
        public decimal EfectivoDolares { get; set; }   // Monto pagado en efectivo dólares

        // Pagos con tarjeta
        public decimal TarjetaCordobas { get; set; }   // Monto pagado con tarjeta córdobas
        public decimal TarjetaDolares { get; set; }    // Monto pagado con tarjeta dólares

        // Tipo de cambio usado para la conversión
        public decimal TipoCambio { get; set; }        // Ej: 36.50 (córdobas por dólar)

        // Total calculado en dólares (para verificación)
        public decimal TotalPagadoUSD { get; set; }    // Suma de todos los pagos convertidos a USD

        // Vuelto (cambio) a entregar al cliente
        public decimal Vuelto { get; set; }            // Diferencia entre TotalPagadoUSD y MontoFinal (en USD)
        public decimal VueltoCordobas { get; set; }    // Vuelto entregado en córdobas
        public decimal VueltoDolares { get; set; }     // Vuelto entregado en dólares

        [StringLength(50)]
        public string MetodoPago { get; set; } // Efectivo, Tarjeta, Mixto (para referencia rápida)

        [StringLength(100)]
        public string NumeroComprobante { get; set; }  // Para pagos con tarjeta

        [StringLength(500)]
        public string Observaciones { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } // Completado, Anulado

        // Navegación
        public MatriculaCurso MatriculaCurso { get; set; }
    }
}
