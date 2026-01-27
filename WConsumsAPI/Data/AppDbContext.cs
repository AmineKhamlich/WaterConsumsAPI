using Microsoft.EntityFrameworkCore; // Espai de noms principal de Entity Framework Core.
using WConsumsAPI.Models; // Necessari per conèixer les nostres entitats (taules).

namespace WConsumsAPI.Data
{
    // Aquesta classe REPRESENTA la BASE DE DADES dins del codi.
    // Hereta de 'DbContext', la classe base de Microsoft EF Core.
    // RESPONSABILITAT: Gestionar la connexió i traduir classes C# a taules SQL.
    public class AppDbContext : DbContext
    {
        // CONSTRUCTOR: Rep les opcions de configuració (cadena de connexió, tipus de BD).
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Passem les opcions a la classe pare (base).
        }

        // Representa la taula 'DIM_CNT' de la base de dades.
        // DbSet<T> permet fer consultes LINQ (Select, Where...) sobre aquesta taula.
        public DbSet<DimCnt> DimCnts { get; set; }

        // Representa la taula 'FACT_CNT_HISTORIAN_V2'.
        public DbSet<FactCntHistorianV2> FactCntHistorians { get; set; }
    }
}
