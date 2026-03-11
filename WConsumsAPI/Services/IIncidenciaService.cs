using WConsumsAPI.DTOs;

namespace WConsumsAPI.Services
{
    public interface IIncidenciaService
    {
        // Mètode per obtenir les alarmes filtrades segons la llista de plantes
        Task<List<IncidenciaVistaDto>> GetIncidenciesFiltradesAsync(string idsPlantes);
    }
}
