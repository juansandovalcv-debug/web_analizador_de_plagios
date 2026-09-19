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

            modelBuilder.Entity<Estudiante>()
                .HasIndex(e => new { e.Carnet, e.IdGrupo })
                .IsUnique();

        }
    }
}
