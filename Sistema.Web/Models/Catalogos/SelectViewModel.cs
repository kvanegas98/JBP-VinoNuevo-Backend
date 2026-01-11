namespace Sistema.Web.Models.Catalogos
{
    public class SelectViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class CargoSelectViewModel
    {
        public int CargoId { get; set; }
        public string Nombre { get; set; }
        public decimal PorcentajeDescuento { get; set; }
    }

    public class PrecioMatriculaViewModel
    {
        public int PrecioMatriculaId { get; set; }
        public int CategoriaEstudianteId { get; set; }
        public int AnioLectivoId { get; set; }
        public decimal Precio { get; set; }
    }
}
