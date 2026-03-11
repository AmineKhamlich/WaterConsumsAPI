using WConsumsAPI.Data;
using WConsumsAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data; // Necessari per al ConnectionState
using System;

namespace WConsumsAPI.Services
{
    public class AppPlantaService : IAppPlantaService
    {
        private readonly AppDbContextAPP _context;

        public AppPlantaService(AppDbContextAPP context)
        {
            _context = context;
        }

        // 1. Obtenir totes les plantes CRIDANT AL PROCEDURE
        public async Task<List<PlantaDto>> GetAllPlantesAsync()
        {
            var plantes = new List<PlantaDto>();
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC sp_AppPlanta_GetAll";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        plantes.Add(new PlantaDto
                        {
                            Id_planta = reader.GetInt32(reader.GetOrdinal("Id_planta")),
                            Nom_planta = reader.GetString(reader.GetOrdinal("Nom_planta")),
                            Activa = reader.GetBoolean(reader.GetOrdinal("Activa"))
                        });
                    }
                }
            }
            return plantes;
        }

        // 2. Actualitzar massivament cridant al Procedure (Aquest ja estava bé)
        public async Task<bool> UpdateStatusMassiveAsync(List<int> idsActius)
        {
            try
            {
                // Traduïm la llista [1,3] a text "1,3". Si ve buida, passem "" perquè l'SP ho apagui tot.
                string llistaText = idsActius != null && idsActius.Count > 0
                                    ? string.Join(",", idsActius)
                                    : "";

                var sql = "EXEC sp_AppPlanta_AdminUpdateStatusMassive @LlistaIdsActius";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@LlistaIdsActius", llistaText));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}