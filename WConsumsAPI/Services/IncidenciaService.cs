using WConsumsAPI.Data;
using WConsumsAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data; // Necessari per ConnectionState
using System;

namespace WConsumsAPI.Services
{
    public class IncidenciaService : IIncidenciaService
    {
        private readonly AppDbContextAPP _context;

        public IncidenciaService(AppDbContextAPP context)
        {
            _context = context;
        }

        // NOU MÈTODE QUE CRIDA A L'SP DE LA BASE DE DADES
        public async Task<List<IncidenciaVistaDto>> GetIncidenciesFiltradesAsync(string idsPlantes)
        {
            var list = new List<IncidenciaVistaDto>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    // 1. Cridem al Procedure
                    command.CommandText = "EXEC sp_AppIncidencia_GetFiltrades @LlistaIdsPlantes";

                    // 2. Li passem el paràmetre ('ALL', 'NONE', o '1,3')
                    command.Parameters.Add(new SqlParameter("@LlistaIdsPlantes", idsPlantes));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dto = new IncidenciaVistaDto();

                            // 3. Mapeig Manual que ja tenies (Seguretat total amb noms de columnes)
                            dto.Id = reader.GetInt32(reader.GetOrdinal("ID"));
                            dto.DataCreacio = reader.GetDateTime(reader.GetOrdinal("Data Creació"));

                            // Gestió de nuls per Strings
                            dto.Gravetat = reader.IsDBNull(reader.GetOrdinal("Gravetat")) ? "" : reader.GetString(reader.GetOrdinal("Gravetat"));
                            dto.Estat = reader.IsDBNull(reader.GetOrdinal("Estat")) ? "" : reader.GetString(reader.GetOrdinal("Estat"));
                            dto.Comptador = reader.IsDBNull(reader.GetOrdinal("Comptador")) ? "" : reader.GetString(reader.GetOrdinal("Comptador"));
                            dto.Ubicacio = reader.IsDBNull(reader.GetOrdinal("Ubicació")) ? "" : reader.GetString(reader.GetOrdinal("Ubicació"));
                            dto.DetallAlarma = reader.IsDBNull(reader.GetOrdinal("Detall Alarma")) ? "" : reader.GetString(reader.GetOrdinal("Detall Alarma"));

                            // Gestió de nuls per Dates
                            int colH = reader.GetOrdinal("Hora Avís (H)");
                            if (!reader.IsDBNull(colH)) dto.HoraAvisH = reader.GetDateTime(colH);

                            int colHH = reader.GetOrdinal("Hora Crític (HH)");
                            if (!reader.IsDBNull(colHH)) dto.HoraCriticHH = reader.GetDateTime(colHH);

                            // Gestió de numèrics
                            dto.ConsumRealAvui = reader.GetDouble(reader.GetOrdinal("Consum Real Avui"));

                            int colLimitH = reader.GetOrdinal("Límit H");
                            if (!reader.IsDBNull(colLimitH)) dto.LimitH = reader.GetInt32(colLimitH);

                            int colLimitHH = reader.GetOrdinal("Límit HH");
                            if (!reader.IsDBNull(colLimitHH)) dto.LimitHH = reader.GetInt32(colLimitHH);

                            list.Add(dto);
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open) connection.Close();
            }

            return list;
        }
    }
}