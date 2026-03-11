using Microsoft.EntityFrameworkCore;
using WConsumsAPI.Data;
using WConsumsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Configuració de Swagger per acceptar Tokens JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Introdueix el token JWT d'aquesta manera: Bearer {el_teu_token}\n\nExemple: Bearer eyJhbGciOiJIUzI1...",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configuracio de la Base de Dades (MySQL amb Pomelo)
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configuracio de la Base de Dades (SQL Server)
// 1. Connexió DW (Comptadors) -> De moment es diu AppDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContextDW>(options =>
    options.UseSqlServer(connectionString));
// 2. Connexió APP (Seguretat) -> El nou AppDbContextAPP
var appConnectionString = builder.Configuration.GetConnectionString("AppConnection");
builder.Services.AddDbContext<AppDbContextAPP>(options =>
    options.UseSqlServer(appConnectionString));

// --- CONFIGURACIÓ DE SEGURETAT JWT ---
var jwtKey = builder.Configuration["Jwt:Key"];
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false; // En producció hauria de ser true
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Registre de Serveis al contenidor d'Injeccio de Dependencies (DI)
builder.Services.AddScoped<IDimCntService, DimCntService>();
// Registre del servei d'Històrics
builder.Services.AddScoped<IFactCntHistorianService, FactCntHistorianService>();
// Registre del servei d'Incidències
builder.Services.AddScoped<IIncidenciaService, IncidenciaService>();
// Registre del servei de Plantes
builder.Services.AddScoped<IAppPlantaService, AppPlantaService>();


// Registre del servei d'Usuaris
builder.Services.AddScoped<IAppUsuariService, AppUsuariService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ATENCIÓ: L'ordre és vital. Primer Authentication (Qui ets?), després Authorization (Què pots fer?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
