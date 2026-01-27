using WConsumsAPI.Data; // Accés al DbContext (Base de Dades).
using WConsumsAPI.DTOs; // Accés als DTOs.
using WConsumsAPI.Models; // Accés a les Entitats (Taules).
using Microsoft.EntityFrameworkCore; // Extensions d'Entity Framework (ToListAsync, etc.).
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WConsumsAPI.Services
{
    // Implementació real del servei d'històrics.
    // Aquesta classe conté el codi que llegeix i escriu a la base de dades.
    public class FactCntHistorianService : IFactCntHistorianService
    {
        // Referència al context de la base de dades.
        private readonly AppDbContext _context;

        // Constructor que rep el context per injecció de dependències.
        public FactCntHistorianService(AppDbContext context)
        {
            _context = context;
        }

        // Obté tots els elements de la taula FACT_CNT_HISTORIAN_V2.
        public async Task<List<FactCntHistorianDto>> GetAllAsync()
        {
            // 1. Llegim les entitats de la BD de forma asíncrona.
            var entities = await _context.FactCntHistorians.ToListAsync();

            // 2. Mapegem (convertim) cada entitat a un DTO per retornar-lo.
            return entities.Select(e => new FactCntHistorianDto
            {
                ID = e.ID,
                TagNameID = e.TagNameID,
                FechaNoel = e.FechaNoel,
                FechaInicio = e.FechaInicio,
                FechaFin = e.FechaFin,
                ValorDiferencial = e.ValorDiferencial,
                ValorAbsoluto = e.ValorAbsoluto,
                Hora = e.Hora,
                AñoMes = e.AñoMes
            }).ToList();
        }

        // Obté un element per ID.
        public async Task<FactCntHistorianDto?> GetByIdAsync(int id)
        {
            var entity = await _context.FactCntHistorians.FindAsync(id);
            if (entity == null) return null;

            return new FactCntHistorianDto
            {
                ID = entity.ID,
                TagNameID = entity.TagNameID,
                FechaNoel = entity.FechaNoel,
                FechaInicio = entity.FechaInicio,
                FechaFin = entity.FechaFin,
                ValorDiferencial = entity.ValorDiferencial,
                ValorAbsoluto = entity.ValorAbsoluto,
                Hora = entity.Hora,
                AñoMes = entity.AñoMes
            };
        }

        // Crea un nou registre a la base de dades.
        public async Task<FactCntHistorianDto> CreateAsync(FactCntHistorianDto dto)
        {
            // Creem l'entitat a partir del DTO.
            var entity = new FactCntHistorianV2
            {
                // L'ID normalment és automàtic, no l'assignem si no cal.
                TagNameID = dto.TagNameID,
                FechaNoel = dto.FechaNoel,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                ValorDiferencial = dto.ValorDiferencial,
                ValorAbsoluto = dto.ValorAbsoluto,
                Hora = dto.Hora,
                AñoMes = dto.AñoMes
            };

            _context.FactCntHistorians.Add(entity);
            await _context.SaveChangesAsync(); // Guardem a la BD.

            dto.ID = entity.ID; // Recuperem l'ID generat.
            return dto;
        }

        // Actualitza un registre existent.
        public async Task<bool> UpdateAsync(int id, FactCntHistorianDto dto)
        {
            if (id != dto.ID) return false;

            var entity = await _context.FactCntHistorians.FindAsync(id);
            if (entity == null) return false;

            // Actualitzem els camps.
            entity.TagNameID = dto.TagNameID;
            entity.FechaNoel = dto.FechaNoel;
            entity.FechaInicio = dto.FechaInicio;
            entity.FechaFin = dto.FechaFin;
            entity.ValorDiferencial = dto.ValorDiferencial;
            entity.ValorAbsoluto = dto.ValorAbsoluto;
            entity.Hora = dto.Hora;
            entity.AñoMes = dto.AñoMes;

            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FactCntHistorians.Any(e => e.ID == id)) return false;
                throw;
            }
        }

        // Esborra un registre per ID.
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.FactCntHistorians.FindAsync(id);
            if (entity == null) return false;

            _context.FactCntHistorians.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
