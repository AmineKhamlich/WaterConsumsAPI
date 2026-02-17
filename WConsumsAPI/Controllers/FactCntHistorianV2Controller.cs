using System; // Per usar DateTime i altres tipus bàsics.
using System.Collections.Generic; // Per gestionar llistes (List<T>).
using System.Threading.Tasks; // Per operacions asíncrones (Task).
using Microsoft.AspNetCore.Mvc; // Per usar ControllerBase, HttpGet, etc.
using WConsumsAPI.DTOs; // Per usar el DTO FactCntHistorianDto.
using WConsumsAPI.Services; // Per usar el Servei IFactCntHistorianService.

namespace WConsumsAPI.Controllers
{
    // [Route] defineix l'URL base: http://.../FactCntHistorianV2
    [Route("api/[controller]")]
    // [ApiController] afegeix comportaments automàtics d'API.
    [ApiController]
    public class FactCntHistorianV2Controller : ControllerBase
    {
        // Dependència del Servei d'Històrics.
        private readonly IFactCntHistorianService _service;

        // Constructor: Injecció de Dependències.
        // Rep el servei automàticament.
        public FactCntHistorianV2Controller(IFactCntHistorianService service)
        {
            _service = service;
        }

        // GET: api/FactCntHistorianV2
        // Obtenir tots els registres històrics.
        [HttpGet]
        public async Task<ActionResult<List<FactCntHistorianDto>>> Get()
        {
            // Demanem al servei tots els registres.
            var dtos = await _service.GetAllAsync();
            
            // Retornem 200 OK amb la llista.
            return Ok(dtos);
        }

        // GET: api/FactCntHistorianV2/5
        // Obtenir un registre històric per ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<FactCntHistorianDto>> GetFactCntHistorianV2(int id)
        {
            // Demanem al servei que busqui pel ID.
            var dto = await _service.GetByIdAsync(id);

            // Si no el troba (null), retornem 404 Not Found.
            if (dto == null)
            {
                return NotFound();
            }

            // Si el troba, el retornem (200 OK).
            return dto;
        }

        // PUT: api/FactCntHistorianV2/5
        // Actualitzar un registre existent.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFactCntHistorianV2(int id, FactCntHistorianDto dto)
        {
            // Validem que l'ID de la URL coincideixi amb l'ID del cos (Body).
            if (id != dto.ID)
            {
                return BadRequest(); // 400 Bad Request.
            }

            // Intentem actualitzar a través del servei.
            var updated = await _service.UpdateAsync(id, dto);
            
            // Si el servei retorna false (no trobat/error), retornem 404.
            if (!updated)
            {
                return NotFound();
            }

            // Retornem 204 No Content (èxit sense dades de retorn).
            return NoContent();
        }

        // POST: api/FactCntHistorianV2
        // Crear un nou registre històric.
        [HttpPost]
        public async Task<ActionResult<FactCntHistorianDto>> PostFactCntHistorianV2(FactCntHistorianDto dto)
        {
            // Demanem al servei que creï el registre.
            var createdDto = await _service.CreateAsync(dto);

            // Retornem 201 Created amb la URL per consultar-lo (GetFactCntHistorianV2).
            return CreatedAtAction("GetFactCntHistorianV2", new { id = createdDto.ID }, createdDto);
        }

        // DELETE: api/FactCntHistorianV2/5
        // Esborrar un registre.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFactCntHistorianV2(int id)
        {
            // Intentem esborrar a través del servei.
            var deleted = await _service.DeleteAsync(id);
            
            // Si no el troba, retornem 404.
            if (!deleted)
            {
                return NotFound();
            }

            // Retornem 204 No Content.
            return NoContent();
        }
    }
}
