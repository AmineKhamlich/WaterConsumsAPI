using WConsumsAPI.DTOs; // Per utilitzar el DTO que acabem de crear.
using System.Collections.Generic; // Per treballar amb llistes.
using System.Threading.Tasks; // Per operacions asíncrones.

namespace WConsumsAPI.Services
{
    // Interfície que defineix el contracte per al servei d'històrics.
    // Llista totes les accions que es poden fer amb els històrics, sense dir com es fan.
    public interface IFactCntHistorianService
    {
        // Obtenir tots els registres històrics.
        // ATENCIÓ: Si hi ha milions de registres, això pot ser lent. En el futur es podria filtrar.
        Task<List<FactCntHistorianDto>> GetAllAsync();

        // Obtenir un registre històric concret pel seu ID únic.
        Task<FactCntHistorianDto?> GetByIdAsync(int id);

        // Crear un nou registre històric.
        Task<FactCntHistorianDto> CreateAsync(FactCntHistorianDto dto);

        // Actualitzar un registre existent.
        Task<bool> UpdateAsync(int id, FactCntHistorianDto dto);

        // Esborrar un registre (encara que en històrics no és habitual esborrar).
        Task<bool> DeleteAsync(int id);

        // Obtenir dades filtrades per la gràfica M3
        Task<List<ConsumFiltratDto>> GetConsumFiltratAsync(int idComptador, System.DateTime start, System.DateTime end);

        // Obtenir el valor en temps real (Live) d'un comptador
        Task<double> GetLiveValueAsync(string tagName);

        // Obtenir els registres històrics d'un comptador per un dia concret
        Task<List<FactCntHistorianDto>> GetRegistresPerDiaAsync(int idComptador, DateTime data);

        // Actualitzar el valor d'un registre històric específic (per exemple, per corregir un error)
        Task<bool> UpdateRegistreSeleccionatAsync(int idHistorian, float? nouValor);
    }
}
