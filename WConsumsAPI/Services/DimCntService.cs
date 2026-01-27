using WConsumsAPI.Data; // Per accedir a la base de dades (DbContext).
using WConsumsAPI.DTOs; // Per utilitzar els objectes de transferència (DTOs).
using WConsumsAPI.Models; // Per accedir a les entitats reals de la base de dades.
using Microsoft.EntityFrameworkCore; // Per utilitzar funcions com ToListAsync().
using System.Collections.Generic; // Per gestionar llistes.
using System.Linq; // Per fer consultes i transformacions (Select).
using System.Threading.Tasks; // Per operacions asíncrones.

namespace WConsumsAPI.Services
{
    // Aquesta classe IMPLEMENTA la interfície IDimCntService.
    // Aquí és on realment "passen les coses" (lògica de negoci).
    // DEPENDÈNCIES: Depèn d'AppDbContext per parlar amb la BD.
    public class DimCntService : IDimCntService
    {
        // Variable privada per guardar la referència al context de dades.
        private readonly AppDbContext _context;

        // CONSTRUCTOR: Rep el context per Injecció de Dependències.
        // Quan l'aplicació arranca, .NET automàticament ens passa el context aquí.
        public DimCntService(AppDbContext context)
        {
            _context = context; // Guardem el context per usar-lo als mètodes.
        }

        // Mètode per obtenir tots els registres.
        public async Task<List<DimCntDto>> GetAllAsync()
        {
            // 1. Demanem a la Base de Dades tots els registres de la taula DimCnts.
            // Fem servir 'await' per no bloquejar l'aplicació mentre esperem la BD.
            var entities = await _context.DimCnts.ToListAsync();
            
            // 2. Transformem (Select) cada entitat de BD en un DTO.
            // Això evita enviar camps innecessaris a l'usuari.
            return entities.Select(e => new DimCntDto
            {
                ID = e.ID,          // Copiem l'ID
                Descripcio = e.Descripcio, // Copiem la descripció
                Planta = e.Planta,  // Copiem la planta
                TagName = e.TagName // Copiem el TagName
            }).ToList(); // Convertim el resultat final en una llista.
        }

        // Mètode per obtenir un registre per ID.
        public async Task<DimCntDto?> GetByIdAsync(int id)
        {
            // Busquem a la BD pel seu ID (Primary Key).
            var entity = await _context.DimCnts.FindAsync(id);
            
            // Si no el troba, retornem null immediatament.
            if (entity == null) return null;

            // Si el troba, el convertim a DTO i el retornem.
            return new DimCntDto
            {
                ID = entity.ID,
                Descripcio = entity.Descripcio,
                Planta = entity.Planta,
                TagName = entity.TagName
            };
        }

        // Mètode per crear un nou registre.
        public async Task<DimCntDto> CreateAsync(DimCntDto dto)
        {
            // Creem una nova entitat de BD a partir de les dades del DTO.
            var entity = new DimCnt
            {
                // NO assignem ID perquè normalment s'autogenera.
                Descripcio = dto.Descripcio,
                Planta = dto.Planta,
                TagName = dto.TagName,
                
                // Assignem valors per defecte a camps obligatoris de la BD que no demanem al DTO.
                // Això evita errors d'inserció SQL.
                SourceID = 0,
                QuantityID = 0,
                TotalPlanta = 0,
                Final = 0
            };

            // Afegim l'entitat al context (memòria).
            _context.DimCnts.Add(entity);
            
            // Guardem els canvis a la BD real. Aquí s'executa l'INSERT.
            await _context.SaveChangesAsync();

            // Mirem quin ID li ha assignat la BD i l'actualitzem al DTO per retornar-lo.
            dto.ID = entity.ID; 
            return dto;
        }

        // Mètode per actualitzar un registre existent.
        public async Task<bool> UpdateAsync(int id, DimCntDto dto)
        {
            // Comprovació de seguretat: l'ID de la URL ha de coincidir amb l'ID del cos.
            if (id != dto.ID) return false;

            // Busquem l'entitat que volem modificar.
            var entity = await _context.DimCnts.FindAsync(id);
            if (entity == null) return false; // Si no existeix, retornem false.

            // Actualitzem només els camps que permetem modificar.
            entity.Descripcio = dto.Descripcio;
            entity.Planta = dto.Planta;
            entity.TagName = dto.TagName;

            // Marquem l'estat com a modificat perquè l'ORM sàpiga que ha de fer un UPDATE.
            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                // Intentem guardar els canvis.
                await _context.SaveChangesAsync();
                return true; // Tot ha anat bé.
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si hi ha un error de concurrència (algú altre l'ha esborrat mentre editàvem), comprovem si existeix.
                if (!_context.DimCnts.Any(e => e.ID == id)) return false;
                throw; // Si és un altre error, el llancem amunt.
            }
        }

        // Mètode per esborrar un registre.
        public async Task<bool> DeleteAsync(int id)
        {
            // Busquem l'entitat a esborrar.
            var entity = await _context.DimCnts.FindAsync(id);
            if (entity == null) return false; // Si no existeix, no podem esborrar-la.

            // Marquem l'entitat per ser esborrada.
            _context.DimCnts.Remove(entity);
            
            // Guardem canvis. Aquí s'executa el DELETE.
            await _context.SaveChangesAsync();
            return true;
        }
    }
}