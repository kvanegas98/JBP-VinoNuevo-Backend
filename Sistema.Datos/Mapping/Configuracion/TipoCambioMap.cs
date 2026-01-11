using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Configuracion;

namespace Sistema.Datos.Mapping.Configuracion
{
    public class TipoCambioMap : IEntityTypeConfiguration<TipoCambio>
    {
        public void Configure(EntityTypeBuilder<TipoCambio> builder)
        {
            builder.ToTable("tipo_cambio")
                .HasKey(t => t.TipoCambioId);

            builder.Property(t => t.TasaCompra)
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(t => t.TasaVenta)
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(t => t.VigenteDesde)
                .IsRequired();

            builder.Property(t => t.VigenteHasta)
                .IsRequired(false);

            builder.Property(t => t.FechaRegistro)
                .IsRequired();

            builder.HasOne(t => t.Usuario)
                .WithMany()
                .HasForeignKey(t => t.UsuarioId);
        }
    }
}
