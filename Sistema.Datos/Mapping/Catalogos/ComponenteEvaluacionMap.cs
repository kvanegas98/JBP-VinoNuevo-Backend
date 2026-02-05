using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class ComponenteEvaluacionMap : IEntityTypeConfiguration<ComponenteEvaluacion>
    {
        public void Configure(EntityTypeBuilder<ComponenteEvaluacion> builder)
        {
            builder.ToTable("ComponenteEvaluacion")
                .HasKey(c => c.ComponenteEvaluacionId);

            builder.Property(c => c.TipoEvaluacionId)
                .IsRequired();

            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.PorcentajePeso)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(c => c.Orden)
                .IsRequired();

            builder.Property(c => c.NotaMinima)
                .HasColumnType("decimal(5,2)");

            builder.Property(c => c.EsObligatorio)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.FechaCreacion)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relación con TipoEvaluacion
            builder.HasOne(c => c.TipoEvaluacion)
                .WithMany(t => t.Componentes)
                .HasForeignKey(c => c.TipoEvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Notas
            builder.HasMany(c => c.Notas)
                .WithOne(n => n.ComponenteEvaluacion)
                .HasForeignKey(n => n.ComponenteEvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
