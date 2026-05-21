using API.Abstractions.Extensions;
using API.Abstractions.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.AzureAppServices;
using TradeX.API.Extensions;
using TradeX.Repository;

namespace TradeX.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- LOGGING CONFIGURATION ---
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.AddAzureWebAppDiagnostics();

            Console.WriteLine("=== STARTING TradeX API ===");
            
            Console.WriteLine($"EnvironmentName: {builder.Environment.EnvironmentName}");

            builder.Services.Configure<AzureFileLoggerOptions>(options =>
            {
                options.FileName = "azure-diagnostics-";
                options.FileSizeLimit = 50 * 1024;
                options.RetainedFileCountLimit = 5;
            });

            builder.Services.Configure<AzureBlobLoggerOptions>(options =>
            {
                options.BlobName = "log.txt";
            });

            // --- SERVICES CONFIGURATION ---
            var connectionString = builder.Configuration["ConnectionStrings:Db"]
                                   ?? builder.Configuration["Db"]
                                   ?? builder.Configuration.GetConnectionString("Db");
            try
            {
                builder.Services.AddCoreServices(builder.Configuration);               

                builder.Services.AddControllersWithJsonOptions();
                builder.Services.AddApiVersioningAndExplorer();
                builder.Services.AddSwaggerDocumentation("TradeX API");

                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
                });
            }
            catch (Exception)
            {
                throw;
            }

            WebApplication app;
            ILogger logger = null!;

            try
            {
                app = builder.Build();
                logger = app.Logger;

                logger.LogInformation("TradeX API STARTING...");
                logger.LogInformation("Content Root: {data}", builder.Environment.ContentRootPath);
                logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
                logger.LogInformation("ConnString found: {data}", string.IsNullOrEmpty(connectionString));
                logger.LogInformation("WebApplication built successfully");
            }
            catch (Exception)
            {
                logger?.LogInformation("ERROR DURING APP BUILD");
                throw;
            }

            // --- DATABASE MIGRATIONS ---
            ApplyMigrations(app, logger);
            // --- MIDDLEWARE CONFIGURATION ---
            try
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradeX API V1");
                    c.RoutePrefix = "swagger";
                });

                app.UseHttpsRedirection();
                app.UseRouting();
                app.UseCors();
                app.UseAuthorization();

                app.MapControllers();
                app.UseMiddleware<CorrelationIdMiddleware>();
            }
            catch (Exception)
            {
                logger.LogCritical("ERROR CONFIGURING MIDDLEWARE");
                throw;
            }

            logger.LogInformation("TradeX API is starting - logger");

            // --- MINIMAL API ENDPOINTS ---
            app.MapGet("/health-check", () => Results.Ok(new
            {
                Status = "Alive",
                Env = builder.Environment.EnvironmentName,
                Time = DateTime.Now
            }));

            app.Run();
        }

        private static void ApplyMigrations(WebApplication app, ILogger logger)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TradeXDbContext>();

            const int maxRetries = 20;
            const int delaySeconds = 20;

            for (int i = 1; i <= maxRetries; i++)
            {
                try
                {
                    logger.LogInformation("Database migration attempt {Attempt} of {MaxRetries}...", i, maxRetries);
                    db.Database.Migrate();
                    logger.LogInformation("DATABASE MIGRATION SUCCESSFUL.");
                    return; // Izlazimo iz funkcije ako je uspješno
                }
                catch (Exception ex)
                {
                    if (i == maxRetries)
                    {
                        logger.LogCritical(ex, "DATABASE MIGRATION FAILED after {MaxRetries} attempts.", maxRetries);
                        throw;
                    }

                    logger.LogWarning("Database not ready yet (Attempt {Attempt}): {Message}. Retrying in {Delay}s...", i, ex.Message, delaySeconds);
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }
    }
}
