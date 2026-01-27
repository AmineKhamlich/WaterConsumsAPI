using System.Collections.Generic; // Per usar List<T>.
using System.Threading.Tasks; // Per usar Task (asincronia).
using Microsoft.AspNetCore.Mvc; // Per usar funcionalitats API (ControllerBase, HttpGet, etc.).
using WConsumsAPI.DTOs; // Per conèixer els formats de dades DTO.
using WConsumsAPI.Services; // Per utilitzar el Servei.

namespace WConsumsAPI.Controllers
{
    // [Route] defineix com s'accedeix a aquest controlador des del navegador.
    // [controller] es substitueix pel nom de la classe menys "Controller" -> "DimCnt".
    // URL Final: http://localhost:PORT/DimCnt
    [Route("[controller]")]
    
    // [ApiController] afegeix comportaments automàtics d'API (validació de models, respostes 400 autodescrites).
    [ApiController]
    public class DimCntController : ControllerBase
    {
        // Variable per guardar el servei. NO tenim accés a la BD directament.
        // DEPENDÈNCIA: Depèn de la interfície IDimCntService.
        private readonly IDimCntService _service;

        // CONSTRUCTOR: Injecció de Dependències.
        // Quan arriba una petició, .NET ens passa automàticament el servei configurat a Program.cs.
        public DimCntController(IDimCntService service)
        {
            _service = service;
        }

        // GET: api/DimCnt
        // Mètode HTTP GET per obtenir tota la llista.
        [HttpGet]
        public async Task<ActionResult<List<DimCntDto>>> Get()
        {
            // Deleguem la feina al servei.
            var dtos = await _service.GetAllAsync();
            
            // Retornem 200 OK amb la llista de dades.
            return Ok(dtos);
        }

        // GET: api/DimCnt/5
        // Mètode HTTP GET per obtenir un sol element per ID.
        // "{id}" indica que l'URL espera un paràmetre (ex: /DimCnt/5).
        [HttpGet("{id}")]
        public async Task<ActionResult<DimCntDto>> GetDimCnt(int id)
        {
            // Demanem al servei que busqui per ID.
            var dto = await _service.GetByIdAsync(id);

            // Si el servei retorna null, vol dir que no existeix.
            if (dto == null)
            {
                return NotFound(); // Retornem 404 Not Found.
            }

            // Si existeix, retornem l'objecte trobat (200 OK implícit).
            return dto;
        }

        // PUT: api/DimCnt/5
        // Mètode HTTP PUT per actualitzar un element complet.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDimCnt(int id, DimCntDto dto)
        {
            // Validació extra: l'ID de la URL ha de coincidir amb l'ID de les dades.
            if (id != dto.ID)
            {
                return BadRequest(); // 400 Bad Request si no coincideixen.
            }

            // Demanem al servei que actualitzi.
            var updated = await _service.UpdateAsync(id, dto);
            
            // Si el servei diu que no (normalment perquè no l'ha trobat).
            if (!updated)
            {
                return NotFound(); // 404 Not Found.
            }

            // Retornem 204 No Content (tot bé, però no retornem dades).
            return NoContent();
        }

        // POST: api/DimCnt
        // Mètode HTTP POST per crear un element nou.
        [HttpPost]
        public async Task<ActionResult<DimCntDto>> PostDimCnt(DimCntDto dto)
        {
            // Demanem al servei que creï l'element.
            var createdDto = await _service.CreateAsync(dto);
            
            // Retornem 201 Created.
            // També afegim una capçalera 'Location' que apunta a on es pot consultar el nou element (GetDimCnt).
            return CreatedAtAction("GetDimCnt", new { id = createdDto.ID }, createdDto);
        }

        // DELETE: api/DimCnt/5
        // Mètode HTTP DELETE per esborrar un element.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDimCnt(int id)
        {
            // Demanem al servei que esborri.
            var deleted = await _service.DeleteAsync(id);
            
            // Si retorna false, és que no existia.
            if (!deleted)
            {
                return NotFound(); // 404 Not Found.
            }

            // Retornem 204 No Content.
            return NoContent();
        }
    }
}
