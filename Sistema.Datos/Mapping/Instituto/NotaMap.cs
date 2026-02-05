using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Instituto;

namespace Sistema.Datos.Mapping.Instituto
{
    public class NotaMap : IEntityTypeConfiguration<Nota>
    {
        public void Configure(EntityTypeBuilder<Nota> builder)
        {
            builder.ToTable("nota")
                .HasKey(n => n.NotaId);

            // Campos del sistema legacy (ahora int en lugar de decimal)
            builder.Property(n => n.Nota1)
                .HasColumnType("int");

            builder.Property(n => n.Nota2)
                .HasColumnType("int");

            builder.Property(n => n.Promedio)
                .HasColumnType("int");

            builder.Property(n => n.Observaciones)
                .HasMaxLength(500);

            // Campo NotaValor del nuevo sistema (mapea a columna "Nota")
            builder.Property(n => n.NotaValor)
                .HasColumnName("Nota")
                .HasColumnType("int");

            // Relaciones del sistema legacy
            builder.HasOne(n => n.Matricula)
                .WithMany(m => m.Notas)
                .HasForeignKey(n => n.MatriculaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.Materia)
                .WithMany(m => m.Notas)
                .HasForeignKey(n => n.MateriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones del nuevo sistema
            builder.HasOne(n => n.MatriculaCurso)
                .WithMany(mc => mc.Notas)
                .HasForeignKey(n => n.MatriculaCursoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.ComponenteEvaluacion)
                .WithMany(c => c.Notas)
                .HasForeignKey(n => n.ComponenteEvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(n => n.Usuario)
                .WithMany()
                .HasForeignKey(n => n.UsuarioRegistroId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
