using WConsumsAPI.Data;
using WConsumsAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration; // Per llegir appsettings.json
using System.Text;

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

        // Per a generar el JWT al fer login, necessitarem accedir a la clau secreta i altres paràmetres de configuració, que es poden llegir des d'appsettings.json mitjançant IConfiguration.
        private readonly IConfiguration _configuration; // Per accedir a la clau secreta del JWT

        public AppUsuariService(AppDbContextAPP context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Retorna tota la llista d'usuaris (Ideal per a la pantalla d'Admin)
        public async Task<List<UsuariResumDto>> GetAllAsync()
        {
            var usuaris = new List<UsuariResumDto>();
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "EXEC sp_AppUsuari_GetAll";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Llegim el text (Ex: "Noel-1, Noel-7")
                        var ordinalPlantes = reader.GetOrdinal("PlantesAssignadesText");
                        string plantesText = reader.IsDBNull(ordinalPlantes) ? "" : reader.GetString(ordinalPlantes);

                        // Llegim Nom i Cognom
                        var ordinalNom = reader.GetOrdinal("Nom");
                        var ordinalCognom = reader.GetOrdinal("Cognom");

                        usuaris.Add(new UsuariResumDto
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id_usuari")),
                            NomUsuari = reader.GetString(reader.GetOrdinal("Nom_usuari")),
                            Nom = reader.IsDBNull(ordinalNom) ? "" : reader.GetString(ordinalNom),
                            Cognom = reader.IsDBNull(ordinalCognom) ? "" : reader.GetString(ordinalCognom),
                            Rol = reader.GetString(reader.GetOrdinal("Nom_Rol")),
                            Actiu = reader.GetBoolean(reader.GetOrdinal("Actiu")),
                            CanviPasswordObligatori = reader.GetBoolean(reader.GetOrdinal("CanviPasswordObligatori")),
                            PlantesAssignadesText = plantesText,
                            IdsPlantes = new List<int>() // HO DEIXEM BUIT PERQUÈ NO PETI! L'Android farà la màgia.
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
                // Utilitzem l'Stored Procedure actualitzat que retorna Nom, Cognom i ID_ROL
                command.CommandText = "EXEC sp_AppUsuari_GetByName @NomUsuari";
                command.Parameters.Add(new SqlParameter("@NomUsuari", loginDto.Username));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // 1. Llegim les dades bàsiques d'identitat
                        var dbId = reader.GetInt32(reader.GetOrdinal("Id_usuari"));
                        var dbNomUsuari = reader.GetString(reader.GetOrdinal("Nom_usuari"));
                        var dbPassHash = reader.GetString(reader.GetOrdinal("password_hash"));
                        var dbRol = reader.GetString(reader.GetOrdinal("Nom_Rol"));
                        var dbIdRol = reader.GetInt32(reader.GetOrdinal("ID_ROL")); // Important per a permisos futurs
                        bool canviPass = reader.GetBoolean(reader.GetOrdinal("CanviPasswordObligatori"));

                        // 2. LÒGICA SEGURA PER A NOM I COGNOM (Evitem l'error IndexOutOfRangeException)
                        // Inicialitzem com a buit per defecte (cas de l'usuari Admin)
                        string dbNomReal = "";
                        string dbCognom = "";

                        // Mirem quins són els índexs de les columnes
                        int idxNom = reader.GetOrdinal("Nom");
                        int idxCognom = reader.GetOrdinal("Cognom");

                        // Només llegim si el valor a la DB no és NULL
                        if (!reader.IsDBNull(idxNom)) dbNomReal = reader.GetString(idxNom);
                        if (!reader.IsDBNull(idxCognom)) dbCognom = reader.GetString(idxCognom);

                        // 3. Llegim els IDs de les plantes assignades (vénen en format text "1,3,4")
                        var ordinalIds = reader.GetOrdinal("IdsPlantesAssignades");
                        string stringIds = reader.IsDBNull(ordinalIds) ? "" : reader.GetString(ordinalIds);

                        // Convertim el text de IDs a una llista d'enters de C#
                        List<int> idsPlantesList = new List<int>();
                        if (!string.IsNullOrEmpty(stringIds))
                        {
                            idsPlantesList = stringIds.Split(',').Select(int.Parse).ToList();
                        }

                        // 4. VERIFICACIÓ DE LA CONTRASENYA AMB BCRYPT
                        if (BCrypt.Net.BCrypt.Verify(loginDto.Password, dbPassHash))
                        {
                            // 5. GENERACIÓ DEL TOKEN JWT
                            var tokenHandler = new JwtSecurityTokenHandler();
                            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

                            var tokenDescriptor = new SecurityTokenDescriptor
                            {
                                Subject = new ClaimsIdentity(new[]
                                {
                            new Claim(ClaimTypes.NameIdentifier, dbId.ToString()),
                            new Claim(ClaimTypes.Name, dbNomUsuari),
                            new Claim(ClaimTypes.Role, dbRol),
                            new Claim("IdRol", dbIdRol.ToString()), // Guardem l'ID del rol dins del Token
                            new Claim("PlantesAssignades", stringIds)
                        }),
                                Expires = DateTime.UtcNow.AddDays(7),
                                Issuer = _configuration["Jwt:Issuer"],
                                Audience = _configuration["Jwt:Audience"],
                                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                            };

                            var token = tokenHandler.CreateToken(tokenDescriptor);
                            string tokenString = tokenHandler.WriteToken(token);

                            // 6. RETORNEM EL DTO AMB TOTA LA INFORMACIÓ PER A L'APP
                            return new UsuariResumDto
                            {
                                Id = dbId,
                                NomUsuari = dbNomUsuari,
                                Nom = dbNomReal,    // Si era NULL a la DB, aquí arriba com a ""
                                Cognom = dbCognom,  // Si era NULL a la DB, aquí arriba com a ""
                                Rol = dbRol,
                                IdRol = dbIdRol,
                                Actiu = true,
                                CanviPasswordObligatori = canviPass,
                                Token = tokenString,
                                IdsPlantes = idsPlantesList
                            };
                        }
                    }
                }
            }
            // Si arribem aquí és que les credencials són incorrectes o l'usuari no existeix/està inactiu
            return null;
        }

        // 2. CREATE (Actualitzat per fixar el password a 123456 i llegir duplicats)
        public async Task<bool> CreateAsync(CrearUsuariDto dto)
        {
            try
            {
                // Fixem la contrasenya per defecte i l'encriptem aquí mateix
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");

                string? llistaPlantesText = dto.IdsPlantes != null && dto.IdsPlantes.Count > 0
                                            ? string.Join(",", dto.IdsPlantes)
                                            : null;

                // Preparem els paràmetres afegint el Nom i Cognom
                var parameters = new[] {
                    new SqlParameter("@NomUsuari", dto.Username),
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@NomRol", dto.Rol),
                    new SqlParameter("@Nom", dto.Nom),          // NOU
                    new SqlParameter("@Cognom", dto.Cognom),    // NOU
                    new SqlParameter("@LlistaIdsPlantes", (object?)llistaPlantesText ?? DBNull.Value)
                };

                // Com que l'SP ara fa un "SELECT @NouIdUsuari", utilitzem SqlQueryRaw per llegir el resultat
                var result = await _context.Database.SqlQueryRaw<int>(
                    "EXEC sp_AppUsuari_Insert @NomUsuari, @PasswordHash, @NomRol, @Nom, @Cognom, @LlistaIdsPlantes",
                    parameters
                ).ToListAsync();

                int newId = result.FirstOrDefault();

                // LÒGICA DE DUPLICATS: Si l'SP retorna -1, el nom ja existia!
                if (newId == -1)
                {
                    return false;
                }

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
                // NOU: Traduïm l'Array de C# a text separat per comes
                // (Si envien una llista buida, esdevé un text buit "", i l'SP esborrarà les plantes assignades)
                string? llistaPlantesText = dto.IdsPlantes != null ? string.Join(",", dto.IdsPlantes) : null;

                var sql = "EXEC sp_AppUsuari_Update @IdUsuari, @NomRol, @Actiu, @CanviPasswordObligatori, @LlistaIdsPlantes";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new SqlParameter("@IdUsuari", dto.IdUsuari),
                    new SqlParameter("@NomRol", (object?)dto.NouRol ?? DBNull.Value),
                    new SqlParameter("@Actiu", (object?)dto.Actiu ?? DBNull.Value),
                    new SqlParameter("@CanviPasswordObligatori", (object?)dto.CanviPasswordObligatori ?? DBNull.Value),
                    // Si el DTO no envia res (null), passem DBNull per no tocar-ho. Si envia llista (encara que sigui buida), passem el text
                    new SqlParameter("@LlistaIdsPlantes", dto.IdsPlantes != null ? (object)llistaPlantesText! : DBNull.Value)
                );

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Mètode per a canviar la contrasenya d'un usuari existent, verificant la contrasenya antiga abans de permetre el canvi.
        // No ha de permetre que el usuari posi la mateixa contrasenya que posem per defecte que es la 123456
        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            // No permetem que la nova contrasenya sigui la de "fàbrica" (123456)
            // REGLA DE SEGURETAT: Mínim 6 caràcters i que no sigui la de defecte
            if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 6 || dto.NewPassword == "123456")
            {
                return false;
            }
            // ------------------------

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

        // Mètode per a resetar la contrasenya d'un usuari a la contrasenya per defecte "123456".
        // Aquest mètode és ideal per resetar la contrasenya d'un usuari que ha oblidat la seva contrasenya.
        public async Task<bool> ResetPasswordAsync(string username)
        {
            try
            {
                // 1. Generem el Hash de la contrasenya per defecte "123456"
                string defaultPassword = "123456";
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);

                // 2. Preparem els paràmetres per a l'Stored Procedure
                var parameters = new[] {
                    new SqlParameter("@Username", username),
                    new SqlParameter("@NewPasswordHash", passwordHash)
                };

                // 3. Executem l'SP. 
                // Com que l'SP retorna un SELECT @@ROWCOUNT, el llegim per saber si s'ha trobat l'usuari.
                var result = await _context.Database.SqlQueryRaw<int>(
                    "EXEC sp_AppUsuari_ResetPassword @Username, @NewPasswordHash",
                    parameters
                ).ToListAsync();

                return result.FirstOrDefault() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}