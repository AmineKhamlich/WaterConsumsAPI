using WConsumsAPI.DTOs; // Necessari per conèixer el tipus 'DimCntDto' que retornarem.
using System.Collections.Generic; // Necessari per utilitzar llistes (List<T>).
using System.Threading.Tasks; // Necessari per a la programació asíncrona (Task).

namespace WConsumsAPI.Services // Defineix l'espai de noms per als serveis.
{
    // Aquesta INTERFÍCIE (IDimCntService) defineix el CONTRACTE del que es pot fer amb els comptadors.
    // NO implementa la lògica, només diu "QUÈ" es pot fer, no "COM".
    // DEPENDÈNCIES: Utilitza 'DimCntDto'.
    public interface IDimCntService
    {
        // Defineix un mètode per obtenir TOTS els comptadors.
        // Retorna una Tasca (Task) que contindrà una Llista de DTOs.
        Task<List<DimCntDto>> GetAllAsync();

        // Defineix un mètode per obtenir UN comptador pel seu ID.
        // Rep un 'int id' i retorna un únic 'DimCntDto' (o null si no existeix).
        Task<DimCntDto?> GetByIdAsync(int id);

        // Defineix un metode per obtenir comptadors per planta.
        // Rep un 'string planta' i retorna una Llista de DTOs.
        Task<List<DimCntDto>> GetByPlantaAsync(string planta);

        // Defineix un mètode per obtenir les plantes disponibles.
        // Retorna una Llista de cadenes (strings) amb els noms de les plantes.
        Task<List<string>> GetPlantesAsync();
    }
}
