using WConsumsAPI.DTOs;

namespace WConsumsAPI.Services
{
    public interface IIncidenciaService
    {
        Task<List<IncidenciaVistaDto>> GetActivesByPlantaAsync(string planta);
    }
}
