using System.ComponentModel.DataAnnotations;

namespace WConsumsAPI.DTOs
{
    // DTO per a canvi de contrasenya
    // Aquest DTO serà utilitzat per rebre les dades necessàries per a canviar la contrasenya d'un usuari.
    // Forçat per la App quan es connecti per primera vegada
    public class ChangePasswordDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
