using CloudBackend.Data;
using Microsoft.EntityFrameworkCore;
using CloudBackend.Models;
using Azure.Identity; // Potrzebne do DefaultAzureCredential

var builder = WebApplication.CreateBuilder(args);

// --- NOWA SEKCJA: INTEGRACJA Z MAGAZYNEM KLUCZY (KEY VAULT) ---

// Jeśli aplikacja działa w chmurze (Production), pobieramy hasła z sejfu

if (builder.Environment.IsProduction())

{

    var vaultName = builder.Configuration["KeyVaultName"];

    if (!string.IsNullOrEmpty(vaultName))

    {

        var keyVaultEndpoint = new Uri($"https://{vaultName}.vault.azure.net/");

        // DefaultAzureCredential automatycznie użyje Tożsamości Zarządzanej w Azure

        builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

    }

}



// --- SEKCJA USŁUG (Dependency Injection) ---



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();



// Pobieramy Connection String. 

// Jeśli jesteśmy w Azure, nazwa "DbConnectionString" zostanie automatycznie 

// pobrana z Magazynu Kluczy dzięki powyższej konfiguracji.

var connectionString = builder.Configuration["DbConnectionString"] 

                       ?? builder.Configuration.GetConnectionString("DefaultConnection");



// Rejestracja bazy danych z mechanizmem ponawiania prób (Retry Logic)

builder.Services.AddDbContext<AppDbContext>(options =>

    options.UseSqlServer(connectionString,

        sqlOptions => sqlOptions.EnableRetryOnFailure(

            maxRetryCount: 5,

            maxRetryDelay: TimeSpan.FromSeconds(30),

            errorNumbersToAdd: null)

    ));



builder.Services.AddCors(options => {

    options.AddDefaultPolicy(policy => {

        policy.AllowAnyOrigin()

              .AllowAnyMethod()

              .AllowAnyHeader();

    });

});



var app = builder.Build();



// --- AUTOMATYCZNE DANE STARTOWE ---

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

        }

    }

    catch (Exception ex)

    {

        Console.WriteLine($"Błąd bazy: {ex.Message}");

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
 