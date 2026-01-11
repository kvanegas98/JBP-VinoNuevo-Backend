using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Instituto;

namespace Sistema.Datos.Mapping.Instituto
{
    public class PagoMap : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("pago")
                .HasKey(p => p.PagoId);

            builder.Property(p => p.Codigo)
                .HasMaxLength(20);

            builder.HasIndex(p => p.Codigo)
                .IsUnique()
                .HasFilter("[Codigo] IS NOT NULL");

            builder.Property(p => p.TipoPago)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.Monto)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Descuento)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.MontoFinal)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.MetodoPago)
                .HasMaxLength(50);

            builder.Property(p => p.NumeroComprobante)
                .HasMaxLength(100);

            builder.Property(p => p.Observaciones)
                .HasMaxLength(500);

            builder.Property(p => p.Estado)
                .HasMaxLength(20);

            builder.HasOne(p => p.Matricula)
                .WithMany(m => m.Pagos)
                .HasForeignKey(p => p.MatriculaId);

            builder.HasOne(p => p.Materia)
                .WithMany(m => m.Pagos)
                .HasForeignKey(p => p.MateriaId);
        }
    }
}
