using WConsumsAPI.Data; // Accés al DbContext (Base de Dades).
using WConsumsAPI.DTOs; // Accés als DTOs.
using WConsumsAPI.Models; // Accés a les Entitats (Taules).
using Microsoft.EntityFrameworkCore; // Extensions d'Entity Framework (ToListAsync, etc.).
using Microsoft.Data.SqlClient; // IMPORTANT: Per fer servir SqlParameter
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Data;


namespace WConsumsAPI.Services
{
    // Implementació real del servei d'històrics.
    // Aquesta classe conté el codi que llegeix i escriu a la base de dades.
    public class FactCntHistorianService : IFactCntHistorianService
    {
        // Referència al context de la base de dades.
        private readonly AppDbContextDW _context;
        private readonly AppDbContextAPP _contextApp;

        // Constructor que rep el context per injecció de dependències.
        public FactCntHistorianService(AppDbContextDW context, AppDbContextAPP contextApp)
        {
            _context = context;
            _contextApp = contextApp;
        }

        // Obté tots els elements de la taula FACT_CNT_HISTORIAN_V2.
        public async Task<List<FactCntHistorianDto>> GetAllAsync()
        {
            // LECTURA MASSIVA: Molt més ràpid via SP
            var entities = await _context.FactCntHistorians
                .FromSqlRaw("EXEC sp_Fact_GetAll")
                .ToListAsync();
            return entities.Select(MapToDto).ToList();
        }

        // Obté un element per ID.
        public async Task<FactCntHistorianDto?> GetByIdAsync(int id)
        {
            var paramId = new SqlParameter("@ID", id);
            // 1. Primer obtenim la llista (així EF executa l'SP sense errors)
            var results = await _context.FactCntHistorians
                .FromSqlRaw("EXEC sp_Fact_GetById @ID", paramId)
                .ToListAsync();
            // 2. Ara agafem el primer de la llista que tenim a memòria
            var entity = results.FirstOrDefault();
            if (entity == null) return null;
            return MapToDto(entity);
        }

        // Actualitza un registre existent.
        public async Task<bool> UpdateAsync(int id, FactCntHistorianDto dto)
        {
            if (id != dto.ID) return false;
            // UPDATE DIRECTE: Sense llegir abans. Molt eficient.
            var sql = "EXEC sp_Fact_Update @ID, @TagNameID, @FechaNoel, @FechaInicio, @FechaFin, @ValorDiferencial, @ValorAbsoluto, @Hora, @AñoMes";
            int rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@ID", id),
                new SqlParameter("@TagNameID", dto.TagNameID),
                new SqlParameter("@FechaNoel", dto.FechaNoel),
                new SqlParameter("@FechaInicio", dto.FechaInicio),
                new SqlParameter("@FechaFin", dto.FechaFin),
                new SqlParameter("@ValorDiferencial", dto.ValorDiferencial ?? (object)DBNull.Value),
                new SqlParameter("@ValorAbsoluto", dto.ValorAbsoluto ?? (object)DBNull.Value),
                new SqlParameter("@Hora", dto.Hora ?? (object)DBNull.Value),
                new SqlParameter("@AñoMes", dto.AñoMes ?? (object)DBNull.Value)
            );
            return rowsAffected > 0;
        }
        //---------------------------------------------------------------
        public async Task<FactCntHistorianDto> CreateAsync(FactCntHistorianDto dto)
        {
            // INSERT: Fem servir ExecuteSqlRawAsync perquè no retorna files, sinó que modifica dades.
            // Gestionem el paràmetre de sortida @NewID
            var pNewId = new SqlParameter("@NewID", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            var sql = "EXEC sp_Fact_Insert @TagNameID, @FechaNoel, @FechaInicio, @FechaFin, @ValorDiferencial, @ValorAbsoluto, @Hora, @AñoMes, @NewID OUTPUT";
            await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@TagNameID", dto.TagNameID),
                new SqlParameter("@FechaNoel", dto.FechaNoel),
                new SqlParameter("@FechaInicio", dto.FechaInicio),
                new SqlParameter("@FechaFin", dto.FechaFin),
                new SqlParameter("@ValorDiferencial", dto.ValorDiferencial ?? (object)DBNull.Value),
                new SqlParameter("@ValorAbsoluto", dto.ValorAbsoluto ?? (object)DBNull.Value),
                new SqlParameter("@Hora", dto.Hora ?? (object)DBNull.Value),
                new SqlParameter("@AñoMes", dto.AñoMes ?? (object)DBNull.Value),
                pNewId
            );
            // Recuperem l'ID generat
            dto.ID = (int)pNewId.Value;
            return dto;
        }
        //---------------------------------------------------------------

        // Esborra un registre per ID.
        public async Task<bool> DeleteAsync(int id)
        {
            // DELETE DIRECTE
            var sql = "EXEC sp_Fact_Delete @ID";
            int rowsAffected = await _context.Database.ExecuteSqlRawAsync(sql, new SqlParameter("@ID", id));
            return rowsAffected > 0;
        }

        // Helper per no repetir codi de mapeig
        // Aquest metode .....
        private static FactCntHistorianDto MapToDto(FactCntHistorianV2 e)
        {
            return new FactCntHistorianDto
            {
                ID = e.ID,
                TagNameID = e.TagNameID,
                FechaNoel = e.FechaNoel,
                FechaInicio = e.FechaInicio,
                FechaFin = e.FechaFin,
                ValorDiferencial = e.ValorDiferencial,
                ValorAbsoluto = e.ValorAbsoluto,
                Hora = e.Hora,
                AñoMes = e.AñoMes
            };
        }

        //---------------------------------------------

        public async Task<List<ConsumFiltratDto>> GetConsumFiltratAsync(int idComptador, DateTime start, DateTime end)
        {
            var list = new List<ConsumFiltratDto>();
            var connection = _contextApp.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC sp_AppConsums_GetConsumFiltrat @CNT_ID, @DataInici, @DataFi";
                command.Parameters.Add(new SqlParameter("@CNT_ID", idComptador));
                command.Parameters.Add(new SqlParameter("@DataInici", start));
                command.Parameters.Add(new SqlParameter("@DataFi", end));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new ConsumFiltratDto
                        {
                            Data = reader.GetDateTime(0),
                            Consum = reader.GetDouble(1)
                        });
                    }
                }
            }
            return list;
        }

        //---------------------------------------------

        public async Task<double> GetLiveValueAsync(string tagName)
        {
            var connection = _contextApp.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC sp_AppConsums_GetLiveValue @TagNameId";
                command.Parameters.Add(new SqlParameter("@TagNameId", tagName));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                            return reader.GetDouble(0);
                    }
                }
            }
            return 0.0;
        }

        //---------------------------------------------

        // Cercar els registres d'un dia (crida a sp_AppConsum_GetRegistresPerDia)
        public async Task<List<FactCntHistorianDto>> GetRegistresPerDiaAsync(int idComptador, DateTime data)
        {
            var list = new List<FactCntHistorianDto>();
            var connection = _contextApp.Database.GetDbConnection(); // Usem context APP per l'SP
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC sp_AppConsum_GetRegistresPerDia @IdComptador, @Fecha";
                command.Parameters.Add(new SqlParameter("@IdComptador", idComptador));
                command.Parameters.Add(new SqlParameter("@Fecha", data.Date));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var dto = new FactCntHistorianDto
                        {
                            // Aquestes primeres normalment són les 0, 1, 2
                            ID = reader.GetInt32(0),
                            ValorDiferencial = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                            ValorDifMod = reader.IsDBNull(2) ? (double?)null : reader.GetDouble(2)
                        };
                        
                        try {
                            int idx = reader.GetOrdinal("FechaFin");
                            if (!reader.IsDBNull(idx)) {
                                dto.FechaFin = reader.GetDateTime(idx);
                            }
                        } catch { } // Si per alguna raó l'SP no torna la columna, no fem explotar res.

                        list.Add(dto);
                    }
                }
            }
            return list;
        }

        // 2. Corregir un registre (crida a sp_AppConsum_UpdateRegistreModificat)
        public async Task<bool> UpdateRegistreSeleccionatAsync(int idHistorian, float? nouValor)
        {
            var sql = "EXEC sp_AppConsum_UpdateRegistreModificat @IdHistorian, @NouValor";
            int rowsAffected = await _contextApp.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@IdHistorian", idHistorian),
                new SqlParameter("@NouValor", (object?)nouValor ?? DBNull.Value)
            );
            return rowsAffected > 0;
        }


    }
}
