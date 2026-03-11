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

        // La foto la podem enviar com a text Base64 (codificada) o com a URL si la pugem primer.
        // De moment ho deixem com a string per rebre el Base64 de la càmera.
        public string? FotoBase64 { get; set; }
    }
}