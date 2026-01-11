using Sistema.Entidades.Catalogos;

namespace Sistema.Entidades.Instituto
{
    public class EstudianteCargo
    {
        public int EstudianteCargoId { get; set; }
        public int EstudianteId { get; set; }
        public int CargoId { get; set; }

        public Estudiante Estudiante { get; set; }
        public Cargo Cargo { get; set; }
    }
}
