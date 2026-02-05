using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class TipoEvaluacionMap : IEntityTypeConfiguration<TipoEvaluacion>
    {
        public void Configure(EntityTypeBuilder<TipoEvaluacion> builder)
        {
            builder.ToTable("TipoEvaluacion")
                .HasKey(t => t.TipoEvaluacionId);

            builder.Property(t => t.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Descripcion)
                .HasMaxLength(500);

            builder.Property(t => t.CantidadComponentes)
                .IsRequired();

            builder.Property(t => t.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(t => t.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relación con ComponenteEvaluacion
            builder.HasMany(t => t.Componentes)
                .WithOne(c => c.TipoEvaluacion)
                .HasForeignKey(c => c.TipoEvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
