using Microsoft.EntityFrameworkCore;
using WConsumsAPI.Data;
using WConsumsAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Registre de Serveis al contenidor d'Injeccio de Dependencies (DI)
builder.Services.AddScoped<IDimCntService, DimCntService>();
// Registre del servei d'Històrics
builder.Services.AddScoped<IFactCntHistorianService, FactCntHistorianService>();
// Registre del servei d'Incidències
builder.Services.AddScoped<IIncidenciaService, IncidenciaService>();


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

app.UseAuthorization();

app.MapControllers();

app.Run();
