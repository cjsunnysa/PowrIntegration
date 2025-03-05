using Microsoft.EntityFrameworkCore;
using PowrIntegration.BackOfficeService;
using PowrIntegration.BackOfficeService.Data.Exporters;
using PowrIntegration.BackOfficeService.Data.Importers;
using PowrIntegration.BackOfficeService.MessageQueue;
using PowrIntegration.BackOfficeService.Observability;
using PowrIntegration.BackOfficeService.Powertill;
using PowrIntegration.Shared;
using PowrIntegration.Shared.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

string databaseConnectionString = builder.Configuration.GetConnectionString("PowrIntegrationDatabase") ?? string.Empty;

var metrics = new MetricsVariables();

builder.Services.ConfigureOpenTelemetry(metrics);
builder.Services.ConfigureEntityFramework(databaseConnectionString);
builder.Services.ConfigureServiceOptions(builder.Configuration);
builder.Services.AddSingleton<IMetrics>(metrics);
builder.Services.AddSingleton<BackOfficeServiceRabbitMqFactory>();
builder.Services.AddSingleton<PluItemsFileImport>();
builder.Services.AddSingleton<ClassificationCodesFileImport>();
builder.Services.AddSingleton<PurchaseFileExport>();
builder.Services.AddSingleton<Outbox>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Services.ApplyDatabaseMigrations();

host.Run();
