using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class PrecioMatriculaMap : IEntityTypeConfiguration<PrecioMatricula>
    {
        public void Configure(EntityTypeBuilder<PrecioMatricula> builder)
        {
            builder.ToTable("precio_matricula")
                .HasKey(p => p.PrecioMatriculaId);

            builder.Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(p => p.CategoriaEstudiante)
                .WithMany(c => c.PreciosMatricula)
                .HasForeignKey(p => p.CategoriaEstudianteId);

            builder.HasOne(p => p.Cargo)
                .WithMany()
                .HasForeignKey(p => p.CargoId)
                .IsRequired(false);

            // Índice único compuesto: Categoría + Cargo (NULL incluido)
            builder.HasIndex(p => new { p.CategoriaEstudianteId, p.CargoId })
                .IsUnique();
        }
    }
}
