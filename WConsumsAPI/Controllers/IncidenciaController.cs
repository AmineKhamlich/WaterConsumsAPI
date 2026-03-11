using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WConsumsAPI.DTOs;
using WConsumsAPI.Services;

namespace WConsumsAPI.Controllers
{
    [Authorize] // Seguretat activada! Cal Token JWT per veure o tancar alarmes
    [Route("api/[controller]")]
    [ApiController]
    public class IncidenciaController : ControllerBase
    {
        private readonly IIncidenciaService _service;

        public IncidenciaController(IIncidenciaService service)
        {
            _service = service;
        }

        // Llegeix quines plantes té assignades l'usuari que fa la petició
        private string GetPlantesDelToken()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimPlantes = User.Claims.FirstOrDefault(c => c.Type == "PlantesAssignades");
                if (claimPlantes != null && !string.IsNullOrWhiteSpace(claimPlantes.Value))
                    return claimPlantes.Value;
                return "NONE";
            }
            return "ALL";
        }

        [HttpGet("actives")]
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetActives()
        {
            var idsPlantes = GetPlantesDelToken();
            var result = await _service.GetActivesAsync(idsPlantes);
            return Ok(result);
        }

        [HttpGet("historic")]
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetHistoric()
        {
            var idsPlantes = GetPlantesDelToken();
            var result = await _service.GetHistoricAsync(idsPlantes);
            return Ok(result);
        }

        [HttpPost("tancar")]
        public async Task<IActionResult> TancarIncidencia([FromBody] TancarIncidenciaDto dto)
        {
            // L'ID del tècnic s'extreu del Token, no ens en fiem de l'Android!
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int idUsuari))
            {
                return Unauthorized(new { message = "Token invàlid o usuari no trobat." });
            }

            var success = await _service.TancarIncidenciaAsync(dto, idUsuari);

            if (success)
                return Ok(new { message = "Incidència tancada correctament." });
            else
                return StatusCode(500, new { message = "Error intern al tancar la incidència." });
        }
    }
}