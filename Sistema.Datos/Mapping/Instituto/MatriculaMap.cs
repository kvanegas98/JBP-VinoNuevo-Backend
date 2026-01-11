using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Instituto;

namespace Sistema.Datos.Mapping.Instituto
{
    public class MatriculaMap : IEntityTypeConfiguration<Matricula>
    {
        public void Configure(EntityTypeBuilder<Matricula> builder)
        {
            builder.ToTable("matricula")
                .HasKey(m => m.MatriculaId);

            builder.Property(m => m.Codigo)
                .HasMaxLength(20);

            builder.HasIndex(m => m.Codigo)
                .IsUnique()
                .HasFilter("[Codigo] IS NOT NULL");

            builder.Property(m => m.MontoMatricula)
                .HasColumnType("decimal(18,2)");

            builder.Property(m => m.DescuentoAplicado)
                .HasColumnType("decimal(18,2)");

            builder.Property(m => m.MontoFinal)
                .HasColumnType("decimal(18,2)");

            builder.Property(m => m.Estado)
                .HasMaxLength(20);

            builder.HasOne(m => m.Estudiante)
                .WithMany(e => e.Matriculas)
                .HasForeignKey(m => m.EstudianteId);

            builder.HasOne(m => m.Modulo)
                .WithMany(mod => mod.Matriculas)
                .HasForeignKey(m => m.ModuloId);

            builder.HasOne(m => m.Modalidad)
                .WithMany(mo => mo.Matriculas)
                .HasForeignKey(m => m.ModalidadId);

            builder.HasOne(m => m.CategoriaEstudiante)
                .WithMany(c => c.Matriculas)
                .HasForeignKey(m => m.CategoriaEstudianteId);
        }
    }
}
