using WConsumsAPI.Data; // Accés al DbContext (Base de Dades).
using WConsumsAPI.DTOs; // Accés als DTOs.
using WConsumsAPI.Models; // Accés a les Entitats (Taules).
using Microsoft.EntityFrameworkCore; // Extensions d'Entity Framework (ToListAsync, etc.).
using Microsoft.Data.SqlClient; // IMPORTANT: Per fer servir SqlParameter
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WConsumsAPI.Services
{
    // Implementació real del servei d'històrics.
    // Aquesta classe conté el codi que llegeix i escriu a la base de dades.
    public class FactCntHistorianService : IFactCntHistorianService
    {
        // Referència al context de la base de dades.
        private readonly AppDbContextDW _context;

        // Constructor que rep el context per injecció de dependències.
        public FactCntHistorianService(AppDbContextDW context)
        {
            _context = context;
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
    }
}
