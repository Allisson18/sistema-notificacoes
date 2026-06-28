using Microsoft.EntityFrameworkCore;
using NotificationHub00.Data;
using NotificationHub00.Interfaces;
using NotificationHub00.Services;

var builder = WebApplication.CreateBuilder(args);

//
// =========================
// 1. CONNECTION STRING (LOCAL + RENDER)
// =========================
//
string connectionString;

var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // PRODUÇÃO (Render - DATABASE_URL)
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    connectionString =
        $"Host={uri.Host};" +
        $"Port=5432;" +
        $"Database={uri.AbsolutePath.Trim('/')};" +
        $"Username={userInfo[0]};" +
        $"Password={userInfo[1]};" +
        $"SSL Mode=Require;Trust Server Certificate=true;";
}
else
{
    // DESENVOLVIMENTO LOCAL
    connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new Exception("Connection string 'DefaultConnection' não encontrada.");
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
// 4. MIDDLEWARE PIPELINE
// =========================
//

// Arquivos estáticos (frontend wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

//
// =========================
// 5. MIGRATIONS (AUTO APPLY SAFE)
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
// 6. CONTROLLERS
// =========================
//
app.MapControllers();

app.Run();