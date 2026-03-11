using Microsoft.AspNetCore.Mvc;
using WConsumsAPI.DTOs;
using WConsumsAPI.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // NOU: Necessari per l'AllowAnonymous
using System.Linq; // NOU: Necessari per llegir el Token (Claims)

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

        // GET: api/incidencia/actives
        [AllowAnonymous] // Permetem que els telèfons comunitaris entrin sense estar logejats
        [HttpGet("actives")]
        public async Task<ActionResult<List<IncidenciaVistaDto>>> GetActives()
        {
            // 1. Per defecte, assumim que és un telèfon comunitari (les ensenya totes)
            string idsPlantes = "ALL";

            // 2. Verifiquem si qui truca porta un Token de sessió vàlid
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var claimPlantes = User.Claims.FirstOrDefault(c => c.Type == "PlantesAssignades");

                if (claimPlantes != null && !string.IsNullOrWhiteSpace(claimPlantes.Value))
                {
                    // L'usuari està logejat i té plantes (ex: "1,3").
                    idsPlantes = claimPlantes.Value;
                }
                else
                {
                    // L'usuari està logejat però NO té cap planta assignada.
                    // Li enviem 'NONE' perquè no vegi alarmes que no li toquen.
                    idsPlantes = "NONE";
                }
            }

            // 3. Cridem al servei passant-li 'ALL', 'NONE' o la llista d'IDs ('1,3')
            var result = await _service.GetIncidenciesFiltradesAsync(idsPlantes);

            return Ok(result);
        }
    }
}