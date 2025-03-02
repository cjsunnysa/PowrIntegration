using Microsoft.EntityFrameworkCore;
using PowrIntegration.PowertillService;
using PowrIntegration.PowertillService.Data.Exporters;
using PowrIntegration.PowertillService.Data.Importers;
using PowrIntegration.PowertillService.MessageQueue;
using PowrIntegration.PowertillService.Observability;
using PowrIntegration.PowertillService.Powertill;
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
builder.Services.AddSingleton<PowertillServiceRabbitMqFactory>();
builder.Services.AddSingleton<PluItemsFileImport>();
builder.Services.AddSingleton<ClassificationCodesFileImport>();
builder.Services.AddSingleton<PurchaseFileExport>();
builder.Services.AddSingleton<Outbox>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Services.ApplyDatabaseMigrations();

host.Run();
