using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Globalization;
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

        public async Task<NotificacioIncidenciaDto?> GetNotificacioAsync(int idIncidencia)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT TOP 1
                        INC.ID_INCIDENCIA,
                        INC.ID_DIM_CNT,
                        INC.descripcio,
                        INC.data_creacio,
                        INC.data_detectat_H,
                        INC.data_detectat_HH,
                        INC.nivell_actual,
                        INC.SP_H,
                        INC.SP_HH,
                        D.Descripcio AS DescripcioComptador,
                        D.Planta AS Ubicacio,
                        D.TagName,
                        ISNULL((
                            SELECT SUM(F.ValorDiferencial)
                            FROM [DW].[dbo].[FACT_CNT_HISTORIAN_V2] F
                            WHERE F.TagNameID = INC.ID_DIM_CNT
                              AND F.FechaNoel = CAST(GETDATE() AS date)
                        ), 0) AS ConsumRealAvui
                    FROM [APP].[dbo].[APP_INCIDENCIA] INC
                    LEFT JOIN [DW].[dbo].[DIM_CNT] D ON D.ID = INC.ID_DIM_CNT
                    WHERE INC.ID_INCIDENCIA = @IdIncidencia;";

                command.Parameters.Add(new SqlParameter("@IdIncidencia", idIncidencia));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return null;

                    string GetString(string col)
                    {
                        var ordinal = reader.GetOrdinal(col);
                        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
                    }

                    int GetInt(string col)
                    {
                        var ordinal = reader.GetOrdinal(col);
                        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
                    }

                    int? GetNullableInt(string col)
                    {
                        var ordinal = reader.GetOrdinal(col);
                        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
                    }

                    DateTime? GetNullableDateTime(string col)
                    {
                        var ordinal = reader.GetOrdinal(col);
                        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
                    }

                    double GetDouble(string col)
                    {
                        var ordinal = reader.GetOrdinal(col);
                        return reader.IsDBNull(ordinal) ? 0 : Convert.ToDouble(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
                    }

                    var dto = new NotificacioIncidenciaDto
                    {
                        Id = GetInt("ID_INCIDENCIA"),
                        IdDimCnt = GetInt("ID_DIM_CNT"),
                        Comptador = GetString("DescripcioComptador"),
                        Ubicacio = GetString("Ubicacio"),
                        TagName = GetString("TagName"),
                        DetallAlarma = GetString("descripcio"),
                        DataCreacio = GetNullableDateTime("data_creacio") ?? DateTime.Now,
                        HoraAvisH = GetNullableDateTime("data_detectat_H"),
                        HoraCriticHH = GetNullableDateTime("data_detectat_HH"),
                        NivellActual = GetInt("nivell_actual"),
                        LimitH = GetNullableInt("SP_H"),
                        LimitHH = GetNullableInt("SP_HH"),
                        ConsumRealAvui = GetDouble("ConsumRealAvui")
                    };

                    dto.Gravetat = dto.NivellActual switch
                    {
                        >= 3 => "Critica",
                        2 => "Alerta",
                        1 => "Avis",
                        _ => "Incidencia"
                    };

                    dto.Titol = string.IsNullOrWhiteSpace(dto.Comptador)
                        ? $"Nova {dto.Gravetat}"
                        : $"{dto.Gravetat}: {dto.Comptador}";

                    var limits = dto.NivellActual >= 2
                        ? $"limit HH {FormatNullableInt(dto.LimitHH)}"
                        : $"limit H {FormatNullableInt(dto.LimitH)}";

                    var ubicacio = string.IsNullOrWhiteSpace(dto.Ubicacio) ? "Sense planta" : dto.Ubicacio;
                    var detall = string.IsNullOrWhiteSpace(dto.DetallAlarma) ? "Alarma de consum detectada" : dto.DetallAlarma;
                    dto.Missatge = $"{ubicacio} - {detall}. Consum avui {dto.ConsumRealAvui:0.##} m3, {limits}.";

                    return dto;
                }
            }
        }

        private static string FormatNullableInt(int? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "N/D";
        }

        public async Task<bool> TancarIncidenciaAsync(TancarIncidenciaDto dto, int idUsuari)
        {
            try
            {
                string? rutaFotoGuardada = null;

                // 1. SI HI HA FOTO ADJUNTA, LA GUARDEM A UNA CARPETA DEL SERVIDOR DIRECTAMENT
                if (dto.FotoFile != null && dto.FotoFile.Length > 0)
                {
                    try
                    {
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "ImatgesIncidencies");
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Generem un nom únic amb l'extensió original o jpg per defecte
                        var extension = Path.GetExtension(dto.FotoFile.FileName);
                        if (string.IsNullOrEmpty(extension)) extension = ".jpg";
                        
                        var fileName = $"incidencia_{dto.IdIncidencia}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                        var fullPath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await dto.FotoFile.CopyToAsync(stream);
                        }

                        rutaFotoGuardada = Path.Combine("ImatgesIncidencies", fileName).Replace("\\", "/");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al guardar la imatge binaria: {ex.Message}");
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
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPCIÓ AL TANCAR INCIDÈNCIA: {ex.Message}");
                Console.WriteLine($"STACKTRACE: {ex.StackTrace}");
                if (ex.InnerException != null) {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }
                throw; // Bubble it up to the controller to return as 500 error body
            }
        }
    }
}
