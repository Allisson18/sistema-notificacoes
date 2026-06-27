using Microsoft.EntityFrameworkCore;
using NotificationHub00.Data;
using NotificationHub00.Interfaces;
using NotificationHub00.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// --- CORREÇÃO DA CONEXÃO (BLINDADA PARA LOCAL E NUVEM) ---
// 1. Tenta ler a variável simplificada do Render
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

// 2. Se estiver vazio (Localhost), usa o appsettings.json
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// 3. Converte o formato do Render (postgresql://) para o formato que o .NET entende
if (connectionString != null && (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://")))
{
    // Remove o 'postgresql://' ou 'postgres://' para não quebrar o UriBuilder
    var scheme = connectionString.StartsWith("postgresql://") ? "postgresql://" : "postgres://";
    var cleanUrl = connectionString.Replace(scheme, "http://"); // Truque técnico para o Uri ler o host corretamente

    var databaseUri = new Uri(cleanUrl);
    var userInfo = databaseUri.UserInfo.Split(':');

    var npgsqlBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require
    };
    connectionString = npgsqlBuilder.ToString();
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
// ---------------------------------------------------------

// Registrar os serviços para habilitar o Polimorfismo
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<INotificationService, SmsNotificationService>();

// Habilitar CORS
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
app.UseAuthorization();

// Habilitar o Frontend integrado (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Executa as migrações e cria as tabelas automaticamente na inicialização
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("[DATABASE] Migrações aplicadas com sucesso no Render!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DATABASE ERROR] Erro ao aplicar migrações: {ex.Message}");
    }
}

app.Run();