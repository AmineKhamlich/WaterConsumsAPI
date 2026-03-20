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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IncidenciaController : ControllerBase
    {
        private readonly IIncidenciaService _service;

        public IncidenciaController(IIncidenciaService service)
        {
            _service = service;
        }

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
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetActives(string? plantaId = null)
        {
            // Si plantaId té valor (ve de l'Android), usem aquest. Si és null, usem els del Token.
            var idsPlantes = !string.IsNullOrEmpty(plantaId) ? plantaId : GetPlantesDelToken();
            var result = await _service.GetActivesAsync(idsPlantes);
            return Ok(result);
        }

        [HttpGet("historic")]
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetHistoric(string? plantaId = null)
        {
            var idsPlantes = !string.IsNullOrEmpty(plantaId) ? plantaId : GetPlantesDelToken();
            var result = await _service.GetHistoricAsync(idsPlantes);
            return Ok(result);
        }

        // Retorna la foto d'una alarma tancada com a string Base64
        // La foto s'emmagatzema com a ruta relativa al servidor (ex: ImatgesIncidencies/file.jpg)
        [HttpGet("foto/{alarmaId}")]
        public async Task<IActionResult> GetFoto(int alarmaId)
        {
            try
            {
                // Obtenim la ruta de la foto des del SP (via el historic)
                var idsPlantes = GetPlantesDelToken();
                var historic = await _service.GetHistoricAsync(idsPlantes);
                var alarma = historic.FirstOrDefault(a => a.Id == alarmaId);

                if (alarma == null || string.IsNullOrEmpty(alarma.Foto))
                    return NotFound(new { message = "Foto no trobada." });

                // Construïm la ruta completa del fitxer
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), alarma.Foto.Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (!System.IO.File.Exists(fullPath))
                    return NotFound(new { message = "Fitxer de foto no trobat al servidor." });

                var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                var base64 = Convert.ToBase64String(bytes);
                return Ok(new { base64 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("tancar")]
        public async Task<IActionResult> TancarIncidencia([FromBody] TancarIncidenciaDto dto)
        {
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