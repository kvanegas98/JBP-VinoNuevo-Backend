using System.Collections.Generic;

namespace Sistema.Web.Models.Estudiantes
{
    public class EstudianteViewModel
    {
        public int EstudianteId { get; set; }
        public string Codigo { get; set; }
        public string NombreCompleto { get; set; }
        public string Cedula { get; set; }
        public string CorreoElectronico { get; set; }
        public string Celular { get; set; }
        public string Ciudad { get; set; }
        public string TipoEstudiante { get; set; }
        public bool EsInterno { get; set; }
        public bool EsBecado { get; set; }
        public decimal PorcentajeBeca { get; set; }
        public int? RedId { get; set; }
        public string RedNombre { get; set; }
        public string RedColor { get; set; }
        public List<int> CargosIds { get; set; }
        public List<CargoViewModel> Cargos { get; set; }
        public string IglesiaOrigen { get; set; }
        public string PastorOrigen { get; set; }
        public string DireccionIglesiaOrigen { get; set; }
        public string TelefonoIglesiaOrigen { get; set; }
        public bool Activo { get; set; }
    }

    public class CargoViewModel
    {
        public int CargoId { get; set; }
        public string Nombre { get; set; }
        public decimal PorcentajeDescuento { get; set; }
    }
}
