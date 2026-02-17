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
    [Route("api/[controller]")]
    
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
            var resultsGetAll = await _service.GetAllAsync();
            
            // Retornem 200 OK amb la llista de dades.
            return Ok(resultsGetAll);
        }

        // GET: api/DimCnt/5
        // Mètode HTTP GET per obtenir un sol element per ID.
        // "{id}" indica que l'URL espera un paràmetre (ex: /DimCnt/5).
        [HttpGet("{id}")]
        public async Task<ActionResult<DimCntDto>> GetDimCnt(int id)
        {
            // Demanem al servei que busqui per ID.
            var resultId = await _service.GetByIdAsync(id);

            // Si el servei retorna null, vol dir que no existeix.
            if (resultId == null)
            {
                return NotFound(); // Retornem 404 Not Found.
            }

            // Si existeix, retornem l'objecte trobat (200 OK implícit).
            return resultId;
        }

        // GET: api/DimCnt/planta/{nomPlanta}
        [HttpGet("planta/{planta}")]
        public async Task<ActionResult<List<DimCntDto>>> GetByPlanta(string planta)
        {
            // Demanem al servei que busqui per planta.
            var resultPlanta = await _service.GetByPlantaAsync(planta);
            // Retornem la llista trobada (pot ser buida).
            return Ok(resultPlanta);
        }

        // GET: api/DimCnt/plantes
        [HttpGet("plantes")]
        public async Task<ActionResult<List<string>>> GetPlantes()
        {
            // Demanem al servei que retorni les plantes disponibles.
            var resultPlantes = await _service.GetPlantesAsync();
            // Retornem la llista de plantes.
            return Ok(resultPlantes);
        }
    }
}
