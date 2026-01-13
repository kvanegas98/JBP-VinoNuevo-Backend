using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class MateriaMap : IEntityTypeConfiguration<Materia>
    {
        public void Configure(EntityTypeBuilder<Materia> builder)
        {
            builder.ToTable("materia")
                .HasKey(m => m.MateriaId);

            builder.Property(m => m.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(m => m.Orden)
                .HasDefaultValue(1);

            builder.HasOne(m => m.Modulo)
                .WithMany(mod => mod.Materias)
                .HasForeignKey(m => m.ModuloId);
        }
    }
}
