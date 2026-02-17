namespace WConsumsAPI.DTOs
{
    // DTO per Login (Autenticació)
    // Aquest DTO serà utilitzat per rebre les dades d'autenticació (usuari i contrasenya) des del client.
    // Lo que envía la App al intentar entrar
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
