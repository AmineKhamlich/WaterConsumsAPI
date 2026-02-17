using WConsumsAPI.Data; // Per accedir a la base de dades (DbContext).
using WConsumsAPI.DTOs; // Per utilitzar els objectes de transferència (DTOs).
using WConsumsAPI.Models; // Per accedir a les entitats reals de la base de dades.
using Microsoft.EntityFrameworkCore; // Per utilitzar funcions com ToListAsync().

namespace WConsumsAPI.Services
{
    // Aquesta classe IMPLEMENTA la interfície IDimCntService.
    // Aquí és on realment "passen les coses" (lògica de negoci).
    // DEPENDÈNCIES: Depèn d'AppDbContext per parlar amb la BD.
    public class DimCntService : IDimCntService
    {
        // Variable privada per guardar la referència al context de dades.
        private readonly AppDbContextDW _context;

        // CONSTRUCTOR: Rep el context per Injecció de Dependències.
        // Quan l'aplicació arranca, .NET automàticament ens passa el context aquí.
        public DimCntService(AppDbContextDW context)
        {
            _context = context; // Guardem el context per usar-lo als mètodes.
        }

        // Mètode per obtenir tots els registres.
        public async Task<List<DimCntDto>> GetAllAsync()
        {
            // 1. Demanem a la Base de Dades tots els registres de la taula DimCnts.
            // Fem servir 'await' per no bloquejar l'aplicació mentre esperem la BD.
            var entities = await _context.DimCnts
                .FromSqlRaw(
                    "EXEC sp_DimCnt_GetAll")
                .ToListAsync();
            
            // 2. Transformem (Select) cada entitat de BD en un DTO.
            // Això evita enviar camps innecessaris a l'usuari.
            return entities.Select(e => new DimCntDto
            {
                ID = e.ID,          // Copiem l'ID
                Descripcio = e.Descripcio, // Copiem la descripció
                Planta = e.Planta,  // Copiem la planta
                TagName = e.TagName, // Copiem el TagName
                SP_H_ACUM = e.SP_H_ACUM // Copiem el SP_H_ACUM
            }).ToList(); // Convertim el resultat final en una llista.
        }

        // Mètode per obtenir un registre per ID.
        public async Task<DimCntDto?> GetByIdAsync(int id)
        {
            // 1. Executem l'SP i portem el resultat a memòria (.ToList)
            // El .ToListAsync() és clau per evitar l'error de sintaxi SQL.
            var entities = await _context.DimCnts
                .FromSqlRaw(
                    "EXEC sp_DimCnt_GetById @ID = {0}", id)
                .ToListAsync();
            // 2. Agafem el primer (o null si no n'hi ha) de la llista en memòria
            var entity = entities.FirstOrDefault();
            // 3. Si no existeix, retornem null
            if (entity == null) return null;

            // Si el troba, el convertim a DTO i el retornem.
            return new DimCntDto
            {
                ID = entity.ID,
                Descripcio = entity.Descripcio,
                Planta = entity.Planta,
                TagName = entity.TagName,
                SP_H_ACUM = entity.SP_H_ACUM
            };
        }

        // Mètode per obtenir registres per planta.
        public async Task<List<DimCntDto>> GetByPlantaAsync(string planta)
        {
            // 1. Executem l'SP i portem el resultat a memòria (.ToList)
            var entities = await _context.DimCnts
                .FromSqlRaw(
                    "EXEC sp_DimCnt_GetByPlanta @Planta = {0}", planta)
                .ToListAsync();
            // 2. Convertim cada entitat a DTO i retornem la llista.
            return entities.Select(e => new DimCntDto
            {
                ID = e.ID,
                Descripcio = e.Descripcio,
                Planta = e.Planta,
                TagName = e.TagName
            }).ToList();
        }

        // Mètode per obtenir les plantes disponibles.
        public async Task<List<string>> GetPlantesAsync()
        {
            var plants = new List<string>();
            // 1. Obtenim la connexió del Context
            var connection = _context.Database.GetDbConnection();

            try
            {
                // 2. Obrim connexió si està tancada
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();
                // 3. Creem la comanda
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "EXEC sp_DimCnt_GetPlantes";

                    // 4. Llegim els resultats
                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (await result.ReadAsync())
                        {
                            // Llegim la primera columna (índex 0) si no és nula
                            if (!result.IsDBNull(0))
                            {
                                plants.Add(result.GetString(0));
                            }
                        }
                    }
                }
            }
            finally
            {
                // Opcional: EF Core gestiona la connexió, però tancar-la aquí és bona pràctica si hem fet Open manualment
                // connection.Close(); 
            }
            return plants;
        }
    }
}