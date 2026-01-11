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

            builder.Property(n => n.Calificacion)
                .HasColumnType("decimal(5,2)");

            builder.Property(n => n.Observaciones)
                .HasMaxLength(500);

            builder.HasOne(n => n.Matricula)
                .WithMany(m => m.Notas)
                .HasForeignKey(n => n.MatriculaId);

            builder.HasOne(n => n.Materia)
                .WithMany(m => m.Notas)
                .HasForeignKey(n => n.MateriaId);
        }
    }
}
