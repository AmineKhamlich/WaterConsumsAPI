using WConsumsAPI.DTOs;

namespace WConsumsAPI.Services
{
    public interface IIncidenciaService
    {
        // Mètode per obtenir les alarmes filtrades segons la llista de plantes
        Task<List<IncidenciaVistaDto>> GetActivesAsync(string idsPlantes);

        // Metode per obtenir l'hisoric de les incidències d'una planta concreta
        Task<List<IncidenciaVistaDto>> GetHistoricAsync(string idsPlanta);

        // metode per tancar una incidencia concreta (per ID)
        Task<bool> TancarIncidenciaAsync(TancarIncidenciaDto dto, int idUsuari);
    }
}
