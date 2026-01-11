using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class PrecioMensualidadMap : IEntityTypeConfiguration<PrecioMensualidad>
    {
        public void Configure(EntityTypeBuilder<PrecioMensualidad> builder)
        {
            builder.ToTable("precio_mensualidad")
                .HasKey(p => p.PrecioMensualidadId);

            builder.Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(p => p.CategoriaEstudiante)
                .WithMany()
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
