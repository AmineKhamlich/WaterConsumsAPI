namespace WConsumsAPI.DTOs
{
    // DTO per a l'actualització d'un usuari
    // Per que el Admin pugui modificar dades d'un usuari existent, estat actiu o no, o canviar el rol o obligar a canviar la contrasenya
    public class UpdateUsuariDto
    {
        public int IdUsuari { get; set; }
        public string? NouRol { get; set; } // Opcional: Si és null, no es canvia
        public bool? Actiu { get; set; }    // Opcional: Si és null, no es canvia
        public bool? CanviPasswordObligatori { get; set; } // Opcional: Si és null, no es canvia
    }
}
