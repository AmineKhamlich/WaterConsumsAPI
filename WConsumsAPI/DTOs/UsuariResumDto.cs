namespace WConsumsAPI.DTOs
{
    // DTO per a resum d'usuari (sense contrasenya ni altres dades sensibles)
    // Lo que devolveré a la App después de un login correcto, o al consultar dades de usuari
    public class UsuariResumDto
    {
        public int Id { get; set; }
        public string NomUsuari { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Cognom { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool? Actiu { get; set; }
        public bool CanviPasswordObligatori { get; set; }

        // Aquí guardarem el Token JWT només quan fem Login
        public string? Token { get; set; }

        // NOU: Text per a la pantalla de l'Admin (Ex: "Noel-1, Noel-3")
        public string? PlantesAssignadesText { get; set; }

        // NOU: Llista de números per a l'App Android (Ex: [1, 3])
        public List<int>? IdsPlantes { get; set; }
    }
}
