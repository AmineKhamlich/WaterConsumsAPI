using Microsoft.AspNetCore.Mvc;
using WConsumsAPI.DTOs;
using WConsumsAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WConsumsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Aquest controlador sencer està blindat només per a Administradors
    [Authorize]
    public class PlantaController : ControllerBase
    {
        private readonly IAppPlantaService _service;

        public PlantaController(IAppPlantaService service)
        {
            _service = service;
        }

        // GET: api/planta
        // L'App Android cridarà aquí per pintar la llista de plantes amb els seus interruptors
        // Accessible per a TOTHOM. El Dashboard d'Android ja s'encarrega de filtrar.
        [HttpGet]
        public async Task<ActionResult<List<PlantaDto>>> GetPlantes()
        {
            var plantes = await _service.GetAllPlantesAsync();
            return Ok(plantes);
        }

        // PUT: api/planta/estat
        [Authorize(Roles = "ADMIN")] // NOMÉS l'Admin pot modificar l'estat
        // L'Admin clica "Guardar" a l'App i envia les que estan marcades
        [HttpPut("estat")]
        public async Task<IActionResult> UpdateEstatMassiu(UpdatePlantesActivesDto dto)
        {
            var success = await _service.UpdateStatusMassiveAsync(dto.IdsPlantesActives);

            if (!success)
            {
                return BadRequest(new { message = "S'ha produït un error al guardar l'estat de les plantes." });
            }

            return Ok(new { message = "L'estat de les plantes s'ha actualitzat correctament." });
        }
    }
}