using System.ComponentModel.DataAnnotations;

namespace WConsumsAPI.DTOs
{
    // DTO per a la creació d'un nou usuari
    // Lo que envía la App al intentar crear un nuevo usuario (Admin)
    public class CrearUsuariDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Rol { get; set; } = string.Empty; // Esperarem valors com "ADMIN", "SUPERVISOR", "TECNIC"
    }
}
