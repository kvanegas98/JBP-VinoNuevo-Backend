using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class ModalidadMap : IEntityTypeConfiguration<Modalidad>
    {
        public void Configure(EntityTypeBuilder<Modalidad> builder)
        {
            builder.ToTable("modalidad")
                .HasKey(m => m.ModalidadId);

            builder.Property(m => m.Nombre)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
