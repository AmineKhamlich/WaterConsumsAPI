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

                        void TryStr(string col, Action<string> set) { try { int i = reader.GetOrdinal(col); if (!reader.IsDBNull(i)) set(reader.GetString(i)); } catch { } }
                        void TryInt(string col, Action<int?> set) { try { int i = reader.GetOrdinal(col); if (!reader.IsDBNull(i)) set(reader.GetInt32(i)); } catch { } }
                        void TryDbl(string col, Action<double> set) { try { int i = reader.GetOrdinal(col); if (!reader.IsDBNull(i)) set(reader.GetDouble(i)); } catch { } }
                        void TryDt(string col, Action<DateTime?> set) { try { int i = reader.GetOrdinal(col); if (!reader.IsDBNull(i)) set(reader.GetDateTime(i)); } catch { } }

                        TryStr("Gravetat", v => dto.Gravetat = v);
                        TryStr("Estat", v => dto.Estat = v);
                        TryStr("Ubicació", v => dto.Ubicacio = v);
                        TryStr("Descripció Comptador", v => dto.DescripcioComptador = v);
                        TryStr("Data Creació", v => dto.DataCreacio = v);
                        TryStr("Data Tancament", v => dto.DataTancament = v);
                        TryStr("Tecnic Tancament", v => dto.TecnicTancament = v);
                        TryStr("Temps Transcorregut", v => dto.TempsTranscorregut = v);
                        TryStr("Comptador", v => dto.Comptador = v);
                        TryStr("Detall Alarma", v => dto.DetallAlarma = v);
                        TryInt("Límit H", v => dto.LimitH = v);
                        TryInt("Límit HH", v => dto.LimitHH = v);
                        TryDbl("Consum Dia Alarma", v => dto.ConsumDiaAlarma = v);
                        TryDbl("Consum Real Avui", v => dto.ConsumRealAvui = v);
                        TryStr("Descripció",         v => dto.Descripcio          = v);
                        TryStr("Descripció Solució", v => dto.DescripcioSolucio   = v);
                        TryStr("Foto",               v => dto.Foto                = v);
                        TryDt("Hora Avís (H)", v => dto.HoraAvisH = v);
                        TryDt("Hora Crític (HH)", v => dto.HoraCriticHH = v);

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
                string? rutaFotoGuardada = null;

                // 1. SI HI HA FOTO EN BASE64, LA GUARDEM A UNA CARPETA DEL SERVIDOR
                if (!string.IsNullOrEmpty(dto.FotoBase64))
                {
                    try
                    {
                        // Determinem on guardar-ho. Creem una carpeta "ImatgesIncidencies" al mateix nivell que l'API.
                        // Fem servir Path.Combine que funciona bé tant a Windows (locals) com a Ubuntu (producció).
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "ImatgesIncidencies");

                        // Si la carpeta no existeix, la creem.
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Generem un nom únic per a la foto amb la data i l'ID de l'incidència
                        var fileName = $"incidencia_{dto.IdIncidencia}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                        var fullPath = Path.Combine(folderPath, fileName);

                        // Convertim el Base64 de l'Android de tornada a bits (bytes) i el guardem
                        byte[] imageBytes = Convert.FromBase64String(dto.FotoBase64);
                        await File.WriteAllBytesAsync(fullPath, imageBytes);

                        // Aquesta és la ruta relativa que guardarem a la BBDD
                        // Exemple: "ImatgesIncidencies/incidencia_1_20260315.jpg"
                        rutaFotoGuardada = Path.Combine("ImatgesIncidencies", fileName).Replace("\\", "/");
                    }
                    catch (Exception ex)
                    {
                        // Si peta al guardar la foto, pots decidir si vols aturar o tancar l'incidència igualment
                        // De moment, aturem i retornem fals perquè el tècnic sàpiga que la foto no s'ha guardat
                        Console.WriteLine($"Error al guardar la imatge: {ex.Message}");
                        return false;
                    }
                }

                // 2. EXECUTEM L'STORED PROCEDURE PASSANT LA RUTA EN COMPTES DEL BASE64 GIGANT
                var sql = "EXEC sp_AppIncidencia_TancarIncidencia @IdIncidencia, @IdUsuariTecnic, @Descripcio, @Solucio, @FotoBase64";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@IdIncidencia", dto.IdIncidencia),
                    new SqlParameter("@IdUsuariTecnic", idUsuari),
                    new SqlParameter("@Descripcio", dto.DescripcioIncidencia),
                    new SqlParameter("@Solucio", dto.SolucioAdaptada),
                    // Ara el paràmetre "@FotoBase64" a la BBDD guardarà només la ruta curteta!
                    new SqlParameter("@FotoBase64", (object?)rutaFotoGuardada ?? DBNull.Value)
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