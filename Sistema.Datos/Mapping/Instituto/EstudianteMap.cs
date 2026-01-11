using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Instituto;

namespace Sistema.Datos.Mapping.Instituto
{
    public class EstudianteMap : IEntityTypeConfiguration<Estudiante>
    {
        public void Configure(EntityTypeBuilder<Estudiante> builder)
        {
            builder.ToTable("estudiante")
                .HasKey(e => e.EstudianteId);

            builder.Property(e => e.Codigo)
                .HasMaxLength(20);

            builder.HasIndex(e => e.Codigo)
                .IsUnique()
                .HasFilter("[Codigo] IS NOT NULL");

            builder.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(e => e.Cedula)
                .HasMaxLength(20);

            builder.Property(e => e.CorreoElectronico)
                .HasMaxLength(100);

            builder.Property(e => e.Celular)
                .HasMaxLength(20);

            builder.Property(e => e.Ciudad)
                .HasMaxLength(100);

            builder.Property(e => e.TipoEstudiante)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.IglesiaOrigen)
                .HasMaxLength(200);

            builder.Property(e => e.PastorOrigen)
                .HasMaxLength(200);

            builder.Property(e => e.DireccionIglesiaOrigen)
                .HasMaxLength(300);

            builder.Property(e => e.TelefonoIglesiaOrigen)
                .HasMaxLength(20);

            builder.HasOne(e => e.Red)
                .WithMany(r => r.Estudiantes)
                .HasForeignKey(e => e.RedId);
        }
    }
}
