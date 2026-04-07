using System.ComponentModel.DataAnnotations;

namespace WConsumsAPI.DTOs
{
    // Aquest és el "paquet" que ens enviarà l'App Android quan el tècnic
    // ompli el formulari i cliqui "Tancar la incidència"
    public class TancarIncidenciaDto
    {
        [Required]
        public int IdIncidencia { get; set; }

        [Required(ErrorMessage = "La descripció és obligatòria")]
        public string DescripcioIncidencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "La solució aplicada és obligatòria")]
        public string SolucioAdaptada { get; set; } = string.Empty;

        // Hem eliminat FotoBase64. A partir d'ara l'App Android pujarà els píxels en format
        // binari real via Multipart/Form-Data gràcies a aquest IFormFile.
        public Microsoft.AspNetCore.Http.IFormFile? FotoFile { get; set; }
    }
}