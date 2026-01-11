using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sistema.Entidades.Usuarios;

namespace Sistema.Datos.Mapping.Usuarios
{
    public class RolMap : IEntityTypeConfiguration<Rol>
    {
        public void Configure(EntityTypeBuilder<Rol> builder)
        {
            builder.ToTable("rol")
                .HasKey(r => r.RolId);

            builder.Property(r => r.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.Descripcion)
                .HasMaxLength(200);
        }
    }
}
