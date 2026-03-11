using Microsoft.EntityFrameworkCore; // Per utilitzar funcions com ToListAsync().
using WConsumsAPI.Models; // Necessari per conèixer les nostres entitats (taules).

namespace WConsumsAPI.Data
{
    // Context específic per a la BBDD "APP" (Usuaris, Rols, Incidències, i Plantes)
    public class AppDbContextAPP : DbContext
    {
        public AppDbContextAPP(DbContextOptions<AppDbContextAPP> options) : base(options)
        {
        }

        public DbSet<AppRol> AppRols { get; set; }
        public DbSet<AppUsuari> AppUsuaris { get; set; }
        public DbSet<AppIncidencia> AppIncidencies { get; set; }

        // --- LES NOVES TAULES DE PLANTES ---
        public DbSet<AppPlanta> AppPlantes { get; set; }
        public DbSet<AppUsuariPlanta> AppUsuarisPlantes { get; set; }

        // ====================================================================
        // AQUÍ CONFIGUREM LA CLAU COMPOSTA DE LA TAULA PONT
        // ====================================================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // És important cridar el base.OnModelCreating sempre al principi
            base.OnModelCreating(modelBuilder);

            // Li diem a C# que la taula AppUsuariPlanta té 2 claus primàries (Id_usuari i Id_planta)
            modelBuilder.Entity<AppUsuariPlanta>()
                .HasKey(up => new { up.Id_usuari, up.Id_planta });
        }
    }
}