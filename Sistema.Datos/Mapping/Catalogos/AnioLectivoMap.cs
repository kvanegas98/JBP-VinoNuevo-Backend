using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class AnioLectivoMap : IEntityTypeConfiguration<AnioLectivo>
    {
        public void Configure(EntityTypeBuilder<AnioLectivo> builder)
        {
            builder.ToTable("anio_lectivo")
                .HasKey(a => a.AnioLectivoId);

            builder.Property(a => a.Nombre)
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
