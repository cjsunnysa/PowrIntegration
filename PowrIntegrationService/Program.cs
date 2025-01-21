using Microsoft.EntityFrameworkCore;
using PowrIntegrationService;
using PowrIntegrationService.Data.Exporters;
using PowrIntegrationService.Data.Importers;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.MessageQueue;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure Serilog from appsettings.json
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .CreateLogger();

//builder.Logging.AddSerilog(Log.Logger, true);

string databaseConnectionString = builder.Configuration.GetConnectionString("PowrIntegrationDatabase") ?? string.Empty;

builder.Services.ConfigureOpenTelemetry();
builder.Services.ConfigureHttpClients();
builder.Services.ConfigureEntityFramework(databaseConnectionString);
builder.Services.ConfigurePowrIntegrationOptions(builder.Configuration);
builder.Services.AddSingleton<RabbitMqFactory>();
builder.Services.AddSingleton<ZraQueuePublisher>();
builder.Services.AddSingleton<ZraQueueConsumer>();
builder.Services.AddSingleton<PowertillQueuePublisher>();
builder.Services.AddSingleton<PowertillQueueConsumer>();
builder.Services.AddSingleton<Outbox>();
builder.Services.AddSingleton<PluItemsImport>();
builder.Services.AddSingleton<IngredientsImport>();
builder.Services.AddSingleton<ClassificationCodesImport>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Services.ApplyDatabaseMigrations();

host.Run();
