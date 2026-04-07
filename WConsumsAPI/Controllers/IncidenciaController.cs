using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WConsumsAPI.DTOs;
using WConsumsAPI.Services;
using Microsoft.AspNetCore.SignalR;

namespace WConsumsAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IncidenciaController : ControllerBase
    {
        private readonly IIncidenciaService _service;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<WConsumsAPI.Hubs.NotificacioHub> _hubContext;

        public IncidenciaController(IIncidenciaService service, Microsoft.AspNetCore.SignalR.IHubContext<WConsumsAPI.Hubs.NotificacioHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        // AQUEST ÉS L'ENDPOINT INVISIBLE QUE TRUCA L'SQL SERVER TRIGGER!
        [AllowAnonymous]
        [HttpPost("SenyalTrigger")]
        public async Task<IActionResult> SenyalTrigger([FromQuery] int id)
        {
            // Quan rep el toc de la base de dades, envia automàticament un senyal a TOTS
            // els telèfons Android connectats al túnel WebSocket.
            // Nom: "RebreNotificacio" - El paràmetre és la ID de la incidència (Android s'encarregarà de llegir-la).
            await _hubContext.Clients.All.SendAsync("RebreNotificacio", id);
            return Ok(new { message = "Senyal escampada als dispositius." });
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
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetActives([FromQuery] string? plantaId = null)
        {
            // Si la App ens passa la planta, filtrem per aquesta. Si no, usem les del Token (per defecte).
            var idsFiltre = string.IsNullOrEmpty(plantaId) ? GetPlantesDelToken() : plantaId;
            var result = await _service.GetActivesAsync(idsFiltre);
            return Ok(result);
        }

        [HttpGet("historic")]
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetHistoric([FromQuery] string? plantaId = null)
        {
            // Mateixa lògica per a l'històric
            var idsFiltre = string.IsNullOrEmpty(plantaId) ? GetPlantesDelToken() : plantaId;
            var result = await _service.GetHistoricAsync(idsFiltre);
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
        public async Task<IActionResult> TancarIncidencia([FromForm] TancarIncidenciaDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int idUsuari))
            {
                return Unauthorized(new { message = "Token invàlid o usuari no trobat." });
            }

            try {
                var success = await _service.TancarIncidenciaAsync(dto, idUsuari);
                if (success)
                    return Ok(new { message = "Incidència tancada correctament." });
                else
                    return StatusCode(500, new { message = "Error desconegut al tancar la incidència." });
            } catch (Exception ex) {
                return StatusCode(500, new { message = $"Error real: {ex.Message}" });
            }
        }
    }
}