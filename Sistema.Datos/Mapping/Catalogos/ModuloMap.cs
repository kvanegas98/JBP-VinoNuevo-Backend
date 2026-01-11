using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Catalogos;

namespace Sistema.Datos.Mapping.Catalogos
{
    public class ModuloMap : IEntityTypeConfiguration<Modulo>
    {
        public void Configure(EntityTypeBuilder<Modulo> builder)
        {
            builder.ToTable("modulo")
                .HasKey(m => m.ModuloId);

            builder.Property(m => m.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasOne(m => m.AnioLectivo)
                .WithMany(a => a.Modulos)
                .HasForeignKey(m => m.AnioLectivoId);

            // Índice único: no puede haber dos módulos con el mismo número en el mismo año
            builder.HasIndex(m => new { m.AnioLectivoId, m.Numero })
                .IsUnique();
        }
    }
}
