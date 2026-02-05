using System.Collections.Generic;

namespace Sistema.Web.Models.Notas
{
    /// <summary>
    /// ViewModel para el response del cálculo de promedio final
    /// </summary>
    public class PromedioViewModel
    {
        /// <summary>
        /// Nota final calculada (0-100, redondeada a entero)
        /// </summary>
        public int NotaFinal { get; set; }

        /// <summary>
        /// Estado: Aprobado o Reprobado
        /// </summary>
        public string Estado { get; set; }

        /// <summary>
        /// Tipo de evaluación usado (Curso Regular, Curso Especializado, etc.)
        /// </summary>
        public string TipoEvaluacion { get; set; }

        /// <summary>
        /// Detalle de cada componente evaluado
        /// </summary>
        public List<DetalleComponenteNotaViewModel> Detalles { get; set; }

        /// <summary>
        /// Indica si todas las notas están completas
        /// </summary>
        public bool NotasCompletas { get; set; }

        /// <summary>
        /// Cantidad de componentes requeridos
        /// </summary>
        public int ComponentesRequeridos { get; set; }

        /// <summary>
        /// Cantidad de componentes registrados
        /// </summary>
        public int ComponentesRegistrados { get; set; }
    }
}
