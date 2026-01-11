using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class TipoPagoMap : IEntityTypeConfiguration<TipoPago>
    {
        public void Configure(EntityTypeBuilder<TipoPago> builder)
        {
            builder.ToTable("tipo_pago")
                .HasKey(t => t.TipoPagoId);

            builder.Property(t => t.Nombre)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
