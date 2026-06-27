using Microsoft.EntityFrameworkCore;
using NotificationHub00.Data;
using NotificationHub00.Interfaces;
using NotificationHub00.Services;

var builder = WebApplication.CreateBuilder(args);

//
// =========================
// 1. CONNECTION STRING
// =========================
// Render: ConnectionStrings__DefaultConnection
// Local: appsettings.json
//
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Connection string 'DefaultConnection' não encontrada.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

//
// =========================
// 2. SERVICES (Polimorfismo)
// =========================
//
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<INotificationService, SmsNotificationService>();

//
// =========================
// 3. CORS
// =========================
//
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

//
// =========================
// 4. MIGRATIONS (SAFE)
// =========================
//
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro ao aplicar migrations: " + ex.Message);
    }
}

//
// =========================
// 5. HTTP PIPELINE
// =========================
//
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

//
// =========================
// 6. STATIC FRONTEND (wwwroot)
// =========================
//
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();