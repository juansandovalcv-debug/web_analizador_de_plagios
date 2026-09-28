using Microsoft.EntityFrameworkCore;
namespace web_analizador_de_plagios.Data.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Grupo> grupos => Set<Grupo>();
        public DbSet<Estudiante> Estudiantes => Set<Estudiante>();
        public DbSet<Evaluacion> Evaluacion  => Set<Evaluacion>();
        public DbSet<HistorialAnalisis> Evaluaciones => Set<HistorialAnalisis>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           base.OnModelCreating(modelBuilder);

            //claves primarias
            modelBuilder.Entity<Grupo>().HasKey(g => g.IdGrupo);
            modelBuilder.Entity<Estudiante>().HasKey(e => e.IdEstudiante);
            modelBuilder.Entity<Evaluacion>().HasKey(e => e.IdEvaluacion);
            modelBuilder.Entity<HistorialAnalisis>().HasKey(h => h.IdHistorial);

            // Índice único para evitar duplicar carnet por grupo
            modelBuilder.Entity<Estudiante>()
                .HasIndex(e => new { e.Carnet, e.IdGrupo })
                .IsUnique();
        }
    }
}
