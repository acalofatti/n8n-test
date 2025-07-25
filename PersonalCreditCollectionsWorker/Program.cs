using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using PersonalCreditCollectionsWorker;
using PersonalCreditCollectionsWorker.Config;
using PersonalCreditCollectionsWorker.Contracts;
using PersonalCreditCollectionsWorker.HealthCheck;
using PersonalCreditCollectionsWorker.Infraestructure;
using PersonalCreditCollectionsWorker.Services;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://*:6000");


// Configuración del entorno
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables();

// Bind de configuración con validación automática
builder.Services
    .AddOptions<RabbitMqSettings>()
    .Bind(builder.Configuration.GetSection("RabbitMq"))
    .ValidateOnStart();

// IConnectionFactory correctamente construido desde IOptions
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

    return new ConnectionFactory
    {
        HostName = config.HostName,
        Port = config.AMQPPort,
        VirtualHost = config.VirtualHost,
        UserName = config.UserName,
        Password = config.Password,
        DispatchConsumersAsync = true
    };
});

// Servicios principales
builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IQueuePublisher, RabbitMqPublisher>();

builder.Services.AddSingleton<ISqlDataExtractor>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("defaultConnection");
    return new SqlDataExtractor(connectionString);
});

builder.Services.AddSingleton<IPostgresDataExtractor>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("postgresConnection");
    return new PostgresDataExtractor(connectionString);
});

builder.Services.AddSingleton<INovedadesProcessor, NovedadesProcessor>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("SQL Server", new SqlConnectionHealthCheck(builder.Configuration.GetConnectionString("defaultConnection")))
    .AddCheck("PostgreSQL", new PostgresConnectionHealthCheck(builder.Configuration.GetConnectionString("postgresConnection")))
    .AddCheck("RabbitMQ", new RabbitMqHealthCheck(builder.Configuration.GetSection("RabbitMq")));

// Construcción del app
var app = builder.Build();

// Exposición de endpoint de Health
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        try
        {
            context.Response.ContentType = "application/json";

            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception?.Message,
                    duration = entry.Value.Duration.TotalMilliseconds
                })
            };

            var json = JsonSerializer.Serialize(result);

            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"{{\"error\": \"{ex.Message}\"}}");
        }
    }
});

// Ejecutar aplicación
app.Run();
