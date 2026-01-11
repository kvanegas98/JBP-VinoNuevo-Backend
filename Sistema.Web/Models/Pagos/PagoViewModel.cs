using System;

namespace Sistema.Web.Models.Pagos
{
    public class PagoViewModel
    {
        public int PagoId { get; set; }
        public string Codigo { get; set; }
        public int MatriculaId { get; set; }
        public string MatriculaCodigo { get; set; }
        public int EstudianteId { get; set; }
        public string EstudianteCodigo { get; set; }
        public string EstudianteNombre { get; set; }
        public int? MateriaId { get; set; }
        public string MateriaNombre { get; set; }
        public int ModuloId { get; set; }
        public string ModuloNombre { get; set; }
        public string TipoPago { get; set; }
        public decimal Monto { get; set; }
        public decimal Descuento { get; set; }
        public decimal MontoFinal { get; set; }
        public DateTime FechaPago { get; set; }

        // Detalle de formas de pago
        public decimal EfectivoCordobas { get; set; }
        public decimal EfectivoDolares { get; set; }
        public decimal TarjetaCordobas { get; set; }
        public decimal TarjetaDolares { get; set; }
        public decimal TipoCambio { get; set; }
        public decimal TotalPagadoUSD { get; set; }

        public string MetodoPago { get; set; }
        public string NumeroComprobante { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
    }
}
