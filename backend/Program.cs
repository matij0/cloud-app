using CloudBackend.Data;
using Microsoft.EntityFrameworkCore;
using CloudBackend.Models;
var builder = WebApplication.CreateBuilder(args);
// --- SEKCJA USŁUG (Dependency Injection) ---
// 1. Rejestracja Kontrolerów (potrzebne, aby nasze API działało)
builder.Services.AddControllers();
// 2. Dokumentacja API (Swagger/OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 3. Pobranie Connection Stringa (zmiennej środowiskowej z Dockera)
var connectionString= Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
?? builder.Configuration.GetConnectionString("DefaultConnection");
// 4. Rejestracja bazy danych MS SQL Server
// Rejestracja bazy danych z mechanizmem ponawiania prób (Retry Logic)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    ));
 
// 5. Konfiguracja CORS - pozwala Reactowi (port 8080) na dostęp do API
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();
// --- AUTOMATYCZNE TWORZENIE BAZY I DANYCH ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // 1. Tworzy bazę i tabele, jeśli ich nie ma
        // context.Database.EnsureCreated(); 
        // 2. Dodaje startowe dane, jeśli tabela jest pusta (opcjonalne, ale fajne)
        if (!context.Tasks.Any())
        {
            context.Tasks.AddRange(
                new CloudTask { Name = "Zrobić kawę", IsCompleted = true },
                new CloudTask { Name = "Uruchomić projekt w Dockerze", IsCompleted = false }
            );
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Błąd podczas tworzenia bazy: {ex.Message}");
    }
}
// --- SEKCJA POTOKU HTTP (Middleware) ---
// Uruchamiamy Swaggera zawsze w fazie deweloperskiej i testowej
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cloud API V1");
    c.RoutePrefix = string.Empty; 
});
// Ważne: W Dockerze często używamy HTTP wewnątrz sieci, 
// więc wyłączamy wymuszone przekierowanie na HTTPS dla uproszczenia nauki
// app.UseHttpsRedirection(); 
app.UseCors();
// Mapowanie kontrolerów (to sprawi, że TasksController zacznie działać)
app.MapControllers();
app.Run();
