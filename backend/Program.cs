using CloudBackend.Data;
using Microsoft.EntityFrameworkCore;
using CloudBackend.Models;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- INTEGRACJA Z KEY VAULT Z OBSŁUGĄ BŁĘDÓW ---
if (builder.Environment.IsProduction())
{
    var vaultName = builder.Configuration["KeyVaultName"];
    if (!string.IsNullOrEmpty(vaultName))
    {
        var keyVaultEndpoint = new Uri($"https://{vaultName}.vault.azure.net/");
        try
        {
            builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
            Console.WriteLine("Key Vault configuration added successfully.");
        }
        catch (Exception ex)
        {
            // Logujemy błąd, ale nie przerywamy działania aplikacji
            Console.WriteLine($"Key Vault authentication failed: {ex.Message}. Falling back to local configuration.");
        }
    }
}

// --- REJESTRACJA USŁUG ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- POBIERANIE CONNECTION STRING Z FALLBACKIEM ---
string connectionString = builder.Configuration["DbConnectionString"];
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine("Using fallback connection string from app settings.");
}
else
{
    Console.WriteLine("Using connection string from Key Vault.");
}

// Rejestracja bazy danych
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    ));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// --- DANE STARTOWE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        if (!context.Tasks.Any())
        {
            context.Tasks.AddRange(
                new CloudTask { Name = "Zrobić kawę", IsCompleted = true },
                new CloudTask { Name = "Zabezpieczyć aplikację w Azure", IsCompleted = true }
            );
            context.SaveChanges();
            Console.WriteLine("Initial tasks added to database.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
}

// --- MIDDLEWARE ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cloud API V1");
    c.RoutePrefix = string.Empty;
});
app.UseCors();
app.MapControllers();

app.Run();