using Microsoft.AspNetCore.Mvc;
using WConsumsAPI.DTOs;
using WConsumsAPI.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WConsumsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidenciaController : ControllerBase
    {
        private readonly IIncidenciaService _service;

        public IncidenciaController(IIncidenciaService service)
        {
            _service = service;
        }

        // GET: api/incidencia/actives/Noel-1
        [HttpGet("actives/{planta}")]
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetActives(string planta)
        {
            var result = await _service.GetActivesByPlantaAsync(planta);
            return Ok(result);
        }
    }
}
