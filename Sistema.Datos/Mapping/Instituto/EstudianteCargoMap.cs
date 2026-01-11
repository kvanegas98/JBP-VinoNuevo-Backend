using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Instituto;

namespace Sistema.Datos.Mapping.Instituto
{
    public class EstudianteCargoMap : IEntityTypeConfiguration<EstudianteCargo>
    {
        public void Configure(EntityTypeBuilder<EstudianteCargo> builder)
        {
            builder.ToTable("estudiante_cargo")
                .HasKey(ec => ec.EstudianteCargoId);

            builder.HasOne(ec => ec.Estudiante)
                .WithMany(e => e.EstudianteCargos)
                .HasForeignKey(ec => ec.EstudianteId);

            builder.HasOne(ec => ec.Cargo)
                .WithMany(c => c.EstudianteCargos)
                .HasForeignKey(ec => ec.CargoId);
        }
    }
}
