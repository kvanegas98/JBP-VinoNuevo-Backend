using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class RedMap : IEntityTypeConfiguration<Red>
    {
        public void Configure(EntityTypeBuilder<Red> builder)
        {
            builder.ToTable("red")
                .HasKey(r => r.RedId);

            builder.Property(r => r.Nombre)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
