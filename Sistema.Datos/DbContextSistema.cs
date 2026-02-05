using Microsoft.EntityFrameworkCore;
using Sistema.Datos.Mapping.Catalogos;
using Sistema.Datos.Mapping.Configuracion;
using Sistema.Datos.Mapping.Instituto;
using Sistema.Datos.Mapping.Usuarios;
using Sistema.Entidades.Catalogos;
using Sistema.Entidades.Configuracion;
using Sistema.Entidades.Instituto;
using Sistema.Entidades.Usuarios;

namespace Sistema.Datos
{
    public class DbContextSistema : DbContext
    {
        // Catálogos
        public DbSet<AnioLectivo> AniosLectivos { get; set; }
        public DbSet<Modalidad> Modalidades { get; set; }
        public DbSet<CategoriaEstudiante> CategoriasEstudiante { get; set; }
        public DbSet<Red> Redes { get; set; }
        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<TipoPago> TiposPago { get; set; }
        public DbSet<PrecioMatricula> PreciosMatricula { get; set; }
        public DbSet<PrecioMensualidad> PreciosMensualidad { get; set; }

        // Catálogos - Cursos Especializados
        public DbSet<CursoEspecializado> CursosEspecializados { get; set; }
        public DbSet<PrecioMatriculaCurso> PreciosMatriculaCurso { get; set; }
        public DbSet<PrecioMensualidadCurso> PreciosMensualidadCurso { get; set; }

        // Catálogos - Sistema de Evaluación
        public DbSet<TipoEvaluacion> TiposEvaluacion { get; set; }
        public DbSet<ComponenteEvaluacion> ComponenteEvaluacion { get; set; }

        // Instituto
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<EstudianteCargo> EstudianteCargos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        // Instituto - Cursos Especializados
        public DbSet<MatriculaCurso> MatriculasCurso { get; set; }
        public DbSet<PagoCurso> PagosCurso { get; set; }

        // Usuarios
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        // Configuración
        public DbSet<TipoCambio> TiposCambio { get; set; }

        public DbContextSistema(DbContextOptions<DbContextSistema> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Catálogos
            modelBuilder.ApplyConfiguration(new AnioLectivoMap());
            modelBuilder.ApplyConfiguration(new ModalidadMap());
            modelBuilder.ApplyConfiguration(new CategoriaEstudianteMap());
            modelBuilder.ApplyConfiguration(new RedMap());
            modelBuilder.ApplyConfiguration(new CargoMap());
            modelBuilder.ApplyConfiguration(new MateriaMap());
            modelBuilder.ApplyConfiguration(new ModuloMap());
            modelBuilder.ApplyConfiguration(new TipoPagoMap());
            modelBuilder.ApplyConfiguration(new PrecioMatriculaMap());
            modelBuilder.ApplyConfiguration(new PrecioMensualidadMap());

            // Catálogos - Sistema de Evaluación
            modelBuilder.ApplyConfiguration(new TipoEvaluacionMap());
            modelBuilder.ApplyConfiguration(new ComponenteEvaluacionMap());

            // Instituto
            modelBuilder.ApplyConfiguration(new EstudianteMap());
            modelBuilder.ApplyConfiguration(new EstudianteCargoMap());
            modelBuilder.ApplyConfiguration(new MatriculaMap());
            modelBuilder.ApplyConfiguration(new NotaMap());
            modelBuilder.ApplyConfiguration(new PagoMap());

            // Usuarios
            modelBuilder.ApplyConfiguration(new RolMap());
            modelBuilder.ApplyConfiguration(new UsuarioMap());

            // Configuración
            modelBuilder.ApplyConfiguration(new TipoCambioMap());
        }
    }
}
