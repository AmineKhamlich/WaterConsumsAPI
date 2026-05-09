using Microsoft.EntityFrameworkCore;
using WConsumsAPI.Data;
using WConsumsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Configuraci� de Swagger per acceptar Tokens JWT
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
// 1. Connexi� DW (Comptadors) -> De moment es diu AppDbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContextDW>(options =>
    options.UseSqlServer(connectionString));
// 2. Connexi� APP (Seguretat) -> El nou AppDbContextAPP
var appConnectionString = builder.Configuration.GetConnectionString("AppConnection");
builder.Services.AddDbContext<AppDbContextAPP>(options =>
    options.UseSqlServer(appConnectionString));

// --- CONFIGURACI� DE SEGURETAT JWT ---
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("Falta configuracio JWT. Revisa Jwt:Key, Jwt:Issuer i Jwt:Audience a appsettings.json o a les variables d'entorn del servei.");
}

var keyBytes = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false; // En producci� hauria de ser true
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Registre de Serveis al contenidor d'Injeccio de Dependencies (DI)
builder.Services.AddSignalR();
builder.Services.AddScoped<IDimCntService, DimCntService>();
// Registre del servei d'Hist�rics
builder.Services.AddScoped<IFactCntHistorianService, FactCntHistorianService>();
// Registre del servei d'Incid�ncies
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

var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "ImatgesIncidencies");
if (!Directory.Exists(imagesPath)) { Directory.CreateDirectory(imagesPath); }
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesPath),
    RequestPath = "/api/imatges"
});

// ATENCI: L'ordre s vital. Primer Authentication (Qui ets?), desprs Authorization (Qu pots fer?)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WConsumsAPI.Hubs.NotificacioHub>("/api/hubs/notificacions");

app.Run();
