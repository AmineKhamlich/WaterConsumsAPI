namespace WConsumsAPI.DTOs
{
    // DTO per a resum d'usuari (sense contrasenya ni altres dades sensibles)
    // Lo que devolveré a la App después de un login correcto, o al consultar datos de usuario
    public class UsuariResumDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool? Actiu { get; set; }
        public bool CanviPasswordObligatori { get; set; }

        // Aquí guardarem el Token JWT només quan fem Login
        public string? Token { get; set; }
    }
}
