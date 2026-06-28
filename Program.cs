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
// 2. SERVICES (POLIMORFISMO)
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
// 4. MIGRATIONS (SAFE + CLOUD READY)
// =========================
//
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Erro ao aplicar migrations: " + ex.Message);
    }
}

//
// =========================
// 5. PIPELINE HTTP
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
// 6. CONTROLLERS
// =========================
//
app.MapControllers();

app.Run();