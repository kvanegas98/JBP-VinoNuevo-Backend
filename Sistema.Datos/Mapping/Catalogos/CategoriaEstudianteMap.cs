using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class CategoriaEstudianteMap : IEntityTypeConfiguration<CategoriaEstudiante>
    {
        public void Configure(EntityTypeBuilder<CategoriaEstudiante> builder)
        {
            builder.ToTable("categoria_estudiante")
                .HasKey(c => c.CategoriaEstudianteId);

            builder.Property(c => c.Nombre)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
