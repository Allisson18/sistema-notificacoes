using Microsoft.EntityFrameworkCore;
using NotificationHub00.Data;
using NotificationHub00.Interfaces;
using NotificationHub00.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Configurar Conexão do PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Registrar os serviços para habilitar o Polimorfismo
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<INotificationService, SmsNotificationService>();

// 3. Habilitar CORS para permitir que sua página HTML local acesse a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll"); // Habilita CORS

app.UseHttpsRedirection();

app.UseAuthorization();

// Habilitar o Frontend integrado (wwwroot). Para o .NET servir o seu HTML automaticamente
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
