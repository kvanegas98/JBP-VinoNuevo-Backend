namespace Sistema.Web.Models.Notas
{
    /// <summary>
    /// ViewModel para el detalle de cada componente de evaluación
    /// </summary>
    public class DetalleComponenteNotaViewModel
    {
        /// <summary>
        /// ID del componente de evaluación
        /// </summary>
        public int ComponenteEvaluacionId { get; set; }

        /// <summary>
        /// Nombre del componente (Examen 1, Examen 2, Proyecto Final, etc.)
        /// </summary>
        public string Componente { get; set; }

        /// <summary>
        /// Nota obtenida en este componente (0-100, entero)
        /// </summary>
        public int Nota { get; set; }

        /// <summary>
        /// Porcentaje de peso del componente (0-100)
        /// Ejemplo: 40.0 representa 40%
        /// </summary>
        public decimal Peso { get; set; }

        /// <summary>
        /// Aporte de este componente a la nota final
        /// Calculado como: Nota × (Peso / 100)
        /// </summary>
        public decimal Aporte { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int Orden { get; set; }
    }
}
