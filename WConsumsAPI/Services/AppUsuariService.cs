using WConsumsAPI.Data;
using WConsumsAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System;

namespace WConsumsAPI.Services
{
    // Implementació del servei d'usuaris, que interactua amb la base de dades a través d'stored procedures.
    // Aquest servei és responsable de gestionar totes les operacions relacionades amb els usuaris, com ara el login, la creació, l'actualització i el canvi de contrasenya.
    // Utilitza BCrypt per a l'encriptació de contrasenyes, assegurant que les contrasenyes es guardin de manera segura a la base de dades.
    // Cada mètode del servei correspon a una operació específica i fa ús dels stored procedures definits a la base de dades per a realitzar les operacions necessàries.
    // 
    public class AppUsuariService : IAppUsuariService
    {
        private readonly AppDbContextAPP _context;

        public AppUsuariService(AppDbContextAPP context)
        {
            _context = context;
        }

        // Retorna tota la llista d'usuaris (Ideal per a la pantalla d'Admin)
        public async Task<List<UsuariResumDto>> GetAllAsync()
        {
            var usuaris = new List<UsuariResumDto>();
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                // CRIDA A L'SP EXISTENT
                command.CommandText = "EXEC sp_AppUsuari_GetAll";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usuaris.Add(new UsuariResumDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id_usuari")),
                            Nom = reader.GetString(reader.GetOrdinal("Nom_usuari")),
                            Rol = reader.GetString(reader.GetOrdinal("Nom_Rol")),
                            Actiu = reader.GetBoolean(reader.GetOrdinal("Actiu")),
                            CanviPasswordObligatori = reader.GetBoolean(reader.GetOrdinal("CanviPasswordObligatori"))
                        });
                    }
                }
            }
            return usuaris;
        }

        // Mètode de login que verifica les credencials de l'usuari i retorna un resum de l'usuari si el login és correcte.
        public async Task<UsuariResumDto?> LoginAsync(LoginDto loginDto)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                // CRIDA A L'SP EXISTENT
                command.CommandText = "EXEC sp_AppUsuari_GetByName @NomUsuari";
                command.Parameters.Add(new SqlParameter("@NomUsuari", loginDto.Username));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var dbId = reader.GetInt32(reader.GetOrdinal("Id_usuari"));
                        var dbNom = reader.GetString(reader.GetOrdinal("Nom_usuari"));
                        var dbPassHash = reader.GetString(reader.GetOrdinal("password_hash"));
                        var dbRol = reader.GetString(reader.GetOrdinal("Nom_Rol"));

                        // En aquest SP (sp_AppUsuari_GetByName) ja filtrem per Actiu = 1,
                        // per tant si arriba aquí, l'usuari està actiu segur.
                        bool canviPass = reader.GetBoolean(reader.GetOrdinal("CanviPasswordObligatori"));

                        // Verifiquem el Password amb BCrypt
                        if (BCrypt.Net.BCrypt.Verify(loginDto.Password, dbPassHash))
                        {
                            return new UsuariResumDto
                            {
                                Id = dbId,
                                Nom = dbNom,
                                Rol = dbRol,
                                Actiu = true,
                                CanviPasswordObligatori = canviPass
                            };
                        }
                    }
                }
            }
            return null; // Login incorrecte o usuari inactiu
        }

        // Mètode per a crear un nou usuari, que encripta la contrasenya abans de guardar-la a la base de dades.
        public async Task<bool> CreateAsync(CrearUsuariDto dto)
        {
            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                // CRIDA A L'SP EXISTENT
                var sql = "EXEC sp_AppUsuari_Insert @NomUsuari, @PasswordHash, @NomRol";
                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@NomUsuari", dto.Username),
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@NomRol", dto.Rol));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        // Mètode per a actualitzar un usuari existent, permet modificar el rol, l'estat i l'obligatorietat de canvi de contrasenya.
        public async Task<bool> UpdateAsync(UpdateUsuariDto dto)
        {
            try
            {
                // CRIDA A L'SP EXISTENT (Modificat per tu al pas anterior)
                var sql = "EXEC sp_AppUsuari_Update @IdUsuari, @NomRol, @Actiu, @CanviPasswordObligatori";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@IdUsuari", dto.IdUsuari),
                    new SqlParameter("@NomRol", (object?)dto.NouRol ?? DBNull.Value),
                    new SqlParameter("@Actiu", (object?)dto.Actiu ?? DBNull.Value),
                    new SqlParameter("@CanviPasswordObligatori", (object?)dto.CanviPasswordObligatori ?? DBNull.Value)
                );

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        // Mètode per a canviar la contrasenya d'un usuari existent, verificant la contrasenya antiga abans de permetre el canvi.
        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            string currentHash = "";

            using (var cmdCheck = connection.CreateCommand())
            {
                // CRIDA A L'SP NOU (Per no posar SELECT al codi)
                cmdCheck.CommandText = "EXEC sp_AppUsuari_GetPasswordHashById @IdUsuari";
                cmdCheck.Parameters.Add(new SqlParameter("@IdUsuari", dto.UserId));

                var result = await cmdCheck.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value) return false;

                currentHash = result.ToString()!;
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, currentHash))
            {
                return false; // Password antic incorrecte
            }

            string newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            using (var cmdUpdate = connection.CreateCommand())
            {
                // CRIDA A L'SP EXISTENT
                cmdUpdate.CommandText = "EXEC sp_AppUsuari_UpdatePassword @IdUsuari, @NewPasswordHash";
                cmdUpdate.Parameters.Add(new SqlParameter("@IdUsuari", dto.UserId));
                cmdUpdate.Parameters.Add(new SqlParameter("@NewPasswordHash", newHash));

                await cmdUpdate.ExecuteNonQueryAsync();
            }

            return true;
        }
    }
}