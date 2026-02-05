using System;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Web.Models.Pagos
{
    // Detalle de pago para soportar pagos mixtos
    public class DetallePagoViewModel
    {
        // Pagos en efectivo
        public decimal EfectivoCordobas { get; set; }  // Monto en efectivo córdobas
        public decimal EfectivoDolares { get; set; }   // Monto en efectivo dólares

        // Pagos con tarjeta
        public decimal TarjetaCordobas { get; set; }   // Monto con tarjeta córdobas
        public decimal TarjetaDolares { get; set; }    // Monto con tarjeta dólares

        // Tipo de cambio del día
        [Required]
        public decimal TipoCambio { get; set; }        // Ej: 36.50

        [StringLength(100)]
        public string NumeroComprobante { get; set; }  // Para pagos con tarjeta

        // Vuelto entregado al cliente
        public decimal VueltoCordobas { get; set; }    // Vuelto entregado en córdobas
        public decimal VueltoDolares { get; set; }     // Vuelto entregado en dólares
    }

    // Pago de matrícula (para activar la matrícula)
    public class PagarMatriculaViewModel
    {
        [Required]
        public int MatriculaId { get; set; }

        [Required]
        public DetallePagoViewModel DetallePago { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }
    }

    // Pago de mensualidad (por materia)
    public class PagarMensualidadViewModel
    {
        [Required]
        public int MatriculaId { get; set; }

        [Required]
        public int MateriaId { get; set; }

        [Required]
        public DetallePagoViewModel DetallePago { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }
    }

    // Anular pago
    public class AnularPagoViewModel
    {
        public string Motivo { get; set; }
    }

    // Consulta de cierre de caja
    public class CierreCajaViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    // ========== CURSOS ESPECIALIZADOS ==========

    // Pago de matrícula de curso
    public class PagarMatriculaCursoViewModel
    {
        [Required]
        public int MatriculaCursoId { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        [Required]
        public DetallePagoViewModel DetallePago { get; set; }
    }

    // Pago de mensualidad de curso
    public class PagarMensualidadCursoViewModel
    {
        [Required]
        public int MatriculaCursoId { get; set; }

        [Required]
        public int NumeroMensualidad { get; set; }

        [StringLength(500)]
        public string Observaciones { get; set; }

        [Required]
        public DetallePagoViewModel DetallePago { get; set; }
    }
}
