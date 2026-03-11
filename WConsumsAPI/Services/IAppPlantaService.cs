using WConsumsAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WConsumsAPI.Services
{
    public interface IAppPlantaService
    {
        // Per carregar els Checkboxes a la pantalla de l'Admin
        Task<List<PlantaDto>> GetAllPlantesAsync();

        // Per guardar els canvis de l'Admin
        Task<bool> UpdateStatusMassiveAsync(List<int> idsActius);
    }
}