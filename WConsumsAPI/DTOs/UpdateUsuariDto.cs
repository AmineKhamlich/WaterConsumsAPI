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

        // NOU: Llista d'IDs actualitzats (si és null, l'API no tocarà les plantes)
        public List<int>? IdsPlantes { get; set; }
    }

    // DTO limitat per permetre que ADMIN i SUPERVISOR només modifiquin plantes assignades.
    public class UpdateUsuariPlantesDto
    {
        public int IdUsuari { get; set; }
        public List<int> IdsPlantes { get; set; } = new();
    }
}
