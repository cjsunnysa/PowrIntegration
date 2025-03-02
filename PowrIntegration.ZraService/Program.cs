using PowrIntegration.Shared;
using PowrIntegration.Shared.Observability;
using PowrIntegration.ZraService;
using PowrIntegration.ZraService.MessageQueue;
using PowrIntegration.ZraService.Observability;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var metrics = new MetricsVariables();

builder.Services.ConfigureOpenTelemetry(metrics);
builder.Services.ConfigureServiceOptions(builder.Configuration);
builder.Services.ConfigureHttpClients();
builder.Services.AddSingleton<IMetrics>(metrics);
builder.Services.AddSingleton<ZraServiceRabbitMqFactory>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();