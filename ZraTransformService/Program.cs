using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration;
using PowrIntegration.Data;
using PowrIntegration.Data.Exporters;
using PowrIntegration.Data.Importers;
using PowrIntegration.Extensions;
using PowrIntegration.MessageQueue;
using PowrIntegration.Options;
using PowrIntegration.Zra;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.AddSerilog(Log.Logger, true);

builder.Services.Configure<ServicesOptions>(builder.Configuration.GetSection(ServicesOptions.KEY));
builder.Services.Configure<ApiOptions>(builder.Configuration.GetSection(ApiOptions.KEY));
builder.Services.Configure<PowertillOptions>(builder.Configuration.GetSection(PowertillOptions.KEY));

builder.Services.ConfigureHttpClients();

builder.Services
    .AddDbContextFactory<PowrIntegrationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("PowrIntegrationDatabase")));

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

host.Run();
