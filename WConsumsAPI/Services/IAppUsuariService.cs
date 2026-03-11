using WConsumsAPI.DTOs;

namespace WConsumsAPI.Services
{
    public interface IAppUsuariService
    {
        // Retorna tota la llista d'usuaris (Ideal per a la pantalla d'Admin)
        Task<List<UsuariResumDto>> GetAllAsync();

        // Comprova credencials i retorna dades si l'usuari existeix i està actiu
        Task<UsuariResumDto?> LoginAsync(LoginDto loginDto);

        // Crea un nou usuari (Encripta el password)
        Task<bool> CreateAsync(CrearUsuariDto dto);

        // Actualitza rol, estat i obligatorietat de password
        Task<bool> UpdateAsync(UpdateUsuariDto dto);

        // Canvia el password d'un usuari existent
        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);

        // Reseteja el password d'un usuari a un valor per defecte (ex: "123456") i força el canvi al proper login
        Task<bool> ResetPasswordAsync(string username);
    }
}
