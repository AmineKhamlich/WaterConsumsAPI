using System.ComponentModel.DataAnnotations;

namespace WConsumsAPI.DTOs
{
    // DTO per a la creació d'un nou usuari
    // Lo que envía la App al intentar crear un nuevo usuario (Admin)
    public class CrearUsuariDto
    {
        [Required(ErrorMessage = "El nom d'usuari és obligatori")]
        public string Username { get; set; } = string.Empty;

        // El password ja no ve de l'App, el posem fix (123456) al Service.
        // [Required]
        // public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nom real és obligatori")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "El cognom és obligatori")]
        public string Cognom { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol és obligatori")]
        public string Rol { get; set; } = string.Empty;

        // Llista d'IDs de les plantes que l'Admin li assigna a l'App
        public List<int>? IdsPlantes { get; set; }
    }
}
