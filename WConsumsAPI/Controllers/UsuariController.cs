using Microsoft.AspNetCore.Mvc;
using WConsumsAPI.DTOs;
using WConsumsAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WConsumsAPI.Controllers
{
    // Controlador per a la gestió d'usuaris, que exposa les operacions relacionades amb els usuaris a través d'API endpoints.
    // El controlador utilitza el servei AppUsuariService per a realitzar les operacions necessàries, com ara el login, la creació, l'actualització i el canvi de contrasenya.
    // EL controlador es la "porta d'entrada" a la API. Es l'encarregat de rebre les peticions HTTP
    // (GET, POST, PUT, DELETE) que enviara la APP de Android, passar-les a Services de AppUsuariService.cs
    // i retornar la resposta corresponent a la APP. (200 OK si tot OK, 400 Bad Request/401 Unauthorized)

    [Route("api/[controller]")]
    [ApiController]
    public class UsuariController : ControllerBase
    {
        private readonly IAppUsuariService _service;

        // Injectem el servei que hem creat al pas anterior
        public UsuariController(IAppUsuariService service)
        {
            _service = service;
        }

        private bool EsSupervisorSenseSerAdmin()
        {
            return User.IsInRole("SUPERVISOR") && !User.IsInRole("ADMIN");
        }

        private async Task<bool> EsUsuariAdminAsync(int idUsuari)
        {
            var usuaris = await _service.GetAllAsync();
            return usuaris.Any(u => u.Id == idUsuari && u.Rol.ToUpperInvariant() == "ADMIN");
        }

        private async Task<bool> EsUsuariAdminAsync(string username)
        {
            var usuaris = await _service.GetAllAsync();
            return usuaris.Any(u => u.NomUsuari.ToUpperInvariant() == username.ToUpperInvariant()
                                   && u.Rol.ToUpperInvariant() == "ADMIN");
        }

        // ADMIN i SUPERVISOR poden consultar usuaris.
        // El supervisor necessita aquest llistat per assignar plantes i restablir contrasenyes.
        // GET: api/usuari
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        [HttpGet]
        public async Task<ActionResult<List<UsuariResumDto>>> GetUsuarios()
        {
            var usuaris = await _service.GetAllAsync();
            return Ok(usuaris); // Retorna codi 200 amb la llista JSON
        }

        // Obert a tothom (no requereix token) perquè és el que s'utilitzarà per fer login des de l'App Android.
        // 2. LOGIN (Per entrar a l'App Android)
        // POST: api/usuari/login
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UsuariResumDto>> Login(LoginDto loginDto)
        {
            var user = await _service.LoginAsync(loginDto);

            // Si el servei retorna null (password malament o usuari inactiu)
            if (user == null)
            {
                // Retorna codi 401: No autoritzat
                return Unauthorized(new { message = "Usuari o contrasenya incorrectes, o l'usuari està inactiu." });
            }

            // Retorna codi 200 i les dades de l'usuari (on la App veurà si ha de forçar el canvi de password)
            return Ok(user);
        }

        // NOMÉS ADMIN pot crear usuaris
        // 3. CREAR USUARI (L'Admin crea des de l'App Android)
        // POST: api/usuari/crear
        [Authorize(Roles = "ADMIN")]
        [HttpPost("crear")]
        public async Task<IActionResult> CrearUsuari(CrearUsuariDto dto)
        {
            var success = await _service.CreateAsync(dto);

            if (!success)
            {
                // Retorna codi 400: Hi ha hagut un error (ex: usuari duplicat)
                return BadRequest(new { message = "Error al crear l'usuari. És possible que el nom ja existeixi o el Rol no sigui vàlid." });
            }

            // Retorna codi 200: Èxit
            return Ok(new { message = "Usuari creat correctament." });
        }

        // NOMÉS ADMIN pot actualitzar usuaris
        // 4. ACTUALITZAR USUARI (L'Admin canvia rol, estat o força canvi de password)
        // PUT: api/usuari/actualitzar
        [Authorize(Roles = "ADMIN")]
        [HttpPut("actualitzar")]
        public async Task<IActionResult> ActualitzarUsuari(UpdateUsuariDto dto)
        {
            var success = await _service.UpdateAsync(dto);

            if (!success)
            {
                return BadRequest(new { message = "Error al actualitzar l'usuari. Comprova que l'ID sigui correcte." });
            }

            return Ok(new { message = "Usuari actualitzat correctament." });
        }

        // ADMIN i SUPERVISOR poden modificar únicament les plantes assignades d'un usuari.
        // No permet canviar rol, estat actiu ni cap altre camp sensible.
        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        [HttpPut("actualitzar-plantes")]
        public async Task<IActionResult> ActualitzarPlantesUsuari(UpdateUsuariPlantesDto dto)
        {
            if (EsSupervisorSenseSerAdmin() && await EsUsuariAdminAsync(dto.IdUsuari))
            {
                return Forbid();
            }

            var updateDto = new UpdateUsuariDto
            {
                IdUsuari = dto.IdUsuari,
                NouRol = null,
                Actiu = null,
                CanviPasswordObligatori = null,
                IdsPlantes = dto.IdsPlantes
            };

            var success = await _service.UpdateAsync(updateDto);

            if (!success)
            {
                return BadRequest(new { message = "Error al actualitzar les plantes de l'usuari." });
            }

            return Ok(new { message = "Plantes assignades actualitzades correctament." });
        }

        // QUALSEVOL USUARI LOGEJAT pot canviar la seva contrasenya
        // 5. CANVIAR CONTRASENYA (L'usuari canvia la seva pròpia contrasenya)
        // POST: api/usuari/change-password
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var success = await _service.ChangePasswordAsync(dto);

            if (!success)
            {
                return BadRequest(new { message = "No s'ha pogut canviar la contrasenya. Comprova que la contrasenya actual sigui correcta." });
            }

            return Ok(new { message = "Contrasenya actualitzada correctament." });
        }

        [Authorize(Roles = "ADMIN,SUPERVISOR")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Dictionary<string, string> data)
        {
            // Com que des d'Android enviem un Map, aquí rebem un Dictionary o un DTO
            if (!data.ContainsKey("username")) return BadRequest();

            if (EsSupervisorSenseSerAdmin() && await EsUsuariAdminAsync(data["username"]))
            {
                return Forbid();
            }

            var success = await _service.ResetPasswordAsync(data["username"]);
            if (!success) return BadRequest(new { message = "L'usuari no existeix" });

            return Ok(new { message = "Contrasenya restablerta" });
        }
    }
}
