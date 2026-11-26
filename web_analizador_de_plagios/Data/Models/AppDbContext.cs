using Microsoft.EntityFrameworkCore;
namespace web_analizador_de_plagios.Data.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Grupo> grupos => Set<Grupo>();
        public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
        public DbSet<Evaluacion> Evaluacion => Set<Evaluacion>();
        public DbSet<HistorialAnalisis> Evaluaciones => Set<HistorialAnalisis>();
        public DbSet<Docente> Docentes => Set<Docente>();
        public DbSet<Asignatura> Asignaturas => Set<Asignatura>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Claves primarias
            modelBuilder.Entity<Grupo>().HasKey(g => g.IdGrupo);
            modelBuilder.Entity<Estudiante>().HasKey(e => e.IdEstudiante);
            modelBuilder.Entity<Evaluacion>().HasKey(e => e.IdEvaluacion);
            modelBuilder.Entity<HistorialAnalisis>().HasKey(h => h.IdHistorial);
            modelBuilder.Entity<Docente>().HasKey(d => d.IdDocente);
            modelBuilder.Entity<Asignatura>().HasKey(a => a.IdAsignatura);

            // Relaciones explícitas (FKs sobre IdGrupo, IdEstudiante e IdEvaluacion)
            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Grupo)
                .WithMany(g => g.Estudiantes)
                .HasForeignKey(e => e.IdGrupo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HistorialAnalisis>()
                .HasOne(h => h.Estudiante)
                .WithMany(e => e.Historiales)
                .HasForeignKey(h => h.IdEstudiante)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HistorialAnalisis>()
                .HasOne(h => h.Evaluacion)
                .WithMany(ev => ev.Historiales)
                .HasForeignKey(h => h.IdEvaluacion)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices únicos
            modelBuilder.Entity<Estudiante>()
                .HasIndex(e => new { e.Carnet, e.IdGrupo })
                .IsUnique();

            modelBuilder.Entity<Docente>()
                .HasIndex(d => d.Correo)
                .IsUnique();

            modelBuilder.Entity<Asignatura>()
                .HasIndex(a => a.Codigo)
                .IsUnique();
        }
    }
}
