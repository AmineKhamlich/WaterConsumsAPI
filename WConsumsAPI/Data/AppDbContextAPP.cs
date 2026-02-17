using Microsoft.EntityFrameworkCore; // Per utilitzar funcions com ToListAsync().
using WConsumsAPI.Models; // Necessari per conèixer les nostres entitats (taules).

namespace WConsumsAPI.Data
{    
    // Context específic per a la BBDD "APP" (Usuaris, Rols, Incidències)
    public class AppDbContextAPP : DbContext
    {
        public AppDbContextAPP(DbContextOptions<AppDbContextAPP> options) : base(options)
        {
        }
        public DbSet<AppRol> AppRols { get; set; }
        public DbSet<AppUsuari> AppUsuaris { get; set; }
        public DbSet<AppIncidencia> AppIncidencies { get; set; }
    }
}
