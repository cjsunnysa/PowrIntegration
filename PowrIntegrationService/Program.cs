using Microsoft.EntityFrameworkCore;
using PowrIntegrationService;
using PowrIntegrationService.Data.Importers;
using PowrIntegrationService.Extensions;
using PowrIntegrationService.MessageQueue;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

string databaseConnectionString = builder.Configuration.GetConnectionString("PowrIntegrationDatabase") ?? string.Empty;

builder.Services.ConfigureOpenTelemetry();
builder.Services.ConfigureHttpClients();
builder.Services.ConfigureEntityFramework(databaseConnectionString);
builder.Services.ConfigurePowrIntegrationOptions(builder.Configuration);
builder.Services.AddSingleton<RabbitMqFactory>();
builder.Services.AddSingleton<PluItemsImport>();
builder.Services.AddSingleton<ClassificationCodesImport>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Services.ApplyDatabaseMigrations();

host.Run();
