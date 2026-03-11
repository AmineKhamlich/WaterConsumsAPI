using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using WConsumsAPI.Data;
using WConsumsAPI.DTOs;

namespace WConsumsAPI.Services
{
    public class IncidenciaService : IIncidenciaService
    {
        private readonly AppDbContextAPP _context;

        public IncidenciaService(AppDbContextAPP context)
        {
            _context = context;
        }

        private async Task<List<IncidenciaVistaDto>> ExecutarSpLlistatAsync(string spName, string idsPlantes)
        {
            var list = new List<IncidenciaVistaDto>();
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"EXEC {spName} @LlistaIdsPlantes";
                command.Parameters.Add(new SqlParameter("@LlistaIdsPlantes", idsPlantes));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var dto = new IncidenciaVistaDto();

                        dto.Id = reader.GetInt32(reader.GetOrdinal("ID"));
                        dto.DataCreacio = reader.GetDateTime(reader.GetOrdinal("Data Creació"));
                        dto.Gravetat = reader.IsDBNull(reader.GetOrdinal("Gravetat")) ? "" : reader.GetString(reader.GetOrdinal("Gravetat"));
                        dto.Estat = reader.IsDBNull(reader.GetOrdinal("Estat")) ? "" : reader.GetString(reader.GetOrdinal("Estat"));
                        dto.Comptador = reader.IsDBNull(reader.GetOrdinal("Comptador")) ? "" : reader.GetString(reader.GetOrdinal("Comptador"));
                        dto.Ubicacio = reader.IsDBNull(reader.GetOrdinal("Ubicació")) ? "" : reader.GetString(reader.GetOrdinal("Ubicació"));
                        dto.DetallAlarma = reader.IsDBNull(reader.GetOrdinal("Detall Alarma")) ? "" : reader.GetString(reader.GetOrdinal("Detall Alarma"));

                        int colH = reader.GetOrdinal("Hora Avís (H)");
                        if (!reader.IsDBNull(colH)) dto.HoraAvisH = reader.GetDateTime(colH);

                        int colHH = reader.GetOrdinal("Hora Crític (HH)");
                        if (!reader.IsDBNull(colHH)) dto.HoraCriticHH = reader.GetDateTime(colHH);

                        dto.ConsumRealAvui = reader.GetDouble(reader.GetOrdinal("Consum Real Avui"));

                        int colLimitH = reader.GetOrdinal("Límit H");
                        if (!reader.IsDBNull(colLimitH)) dto.LimitH = reader.GetInt32(colLimitH);

                        int colLimitHH = reader.GetOrdinal("Límit HH");
                        if (!reader.IsDBNull(colLimitHH)) dto.LimitHH = reader.GetInt32(colLimitHH);

                        // LLEGIM LES DUES NOVES COLUMNES
                        int colDataTancament = reader.GetOrdinal("Data Tancament");
                        if (!reader.IsDBNull(colDataTancament)) dto.DataTancament = reader.GetDateTime(colDataTancament);

                        int colTemps = reader.GetOrdinal("Temps Transcorregut");
                        if (!reader.IsDBNull(colTemps)) dto.TempsTranscorregut = reader.GetString(colTemps);

                        list.Add(dto);
                    }
                }
            }
            return list;
        }

        public async Task<List<IncidenciaVistaDto>> GetActivesAsync(string idsPlantes) =>
            await ExecutarSpLlistatAsync("sp_AppIncidencia_GetActivesAlarms", idsPlantes);

        public async Task<List<IncidenciaVistaDto>> GetHistoricAsync(string idsPlantes) =>
            await ExecutarSpLlistatAsync("sp_AppIncidencia_GetHistoricAlarms", idsPlantes);

        public async Task<bool> TancarIncidenciaAsync(TancarIncidenciaDto dto, int idUsuari)
        {
            try
            {
                var sql = "EXEC sp_AppIncidencia_TancarIncidencia @IdIncidencia, @IdUsuariTecnic, @Descripcio, @Solucio, @FotoBase64";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@IdIncidencia", dto.IdIncidencia),
                    new SqlParameter("@IdUsuariTecnic", idUsuari),
                    new SqlParameter("@Descripcio", dto.DescripcioIncidencia),
                    new SqlParameter("@Solucio", dto.SolucioAdaptada),
                    new SqlParameter("@FotoBase64", (object?)dto.FotoBase64 ?? DBNull.Value)
                );
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}