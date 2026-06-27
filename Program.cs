using Microsoft.EntityFrameworkCore;
using NotificationHub00.Data;
using NotificationHub00.Interfaces;
using NotificationHub00.Services;

var builder = WebApplication.CreateBuilder(args);

//
// 1. CONEXÃO COM BANCO (Render + Local)
// O ASP.NET Core vai ler automaticamente:
// ConnectionStrings__DefaultConnection
//
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

//
// 2. REGISTRO DE SERVIÇOS (Polimorfismo)
//
builder.Services.AddScoped<INotificationService, EmailNotificationService>();
builder.Services.AddScoped<INotificationService, SmsNotificationService>();

//
// 3. CORS (Frontend externo/local)
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

// OpenAPI / Swagger (se estiver usando)
builder.Services.AddOpenApi();

var app = builder.Build();

//
// 4. MIGRATION AUTOMÁTICA (produção)
//
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

//
// 5. PIPELINE HTTP
//
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

//
// 6. FRONTEND ESTÁTICO (wwwroot)
//
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();