using Serilog;
using ZraShared.MessageQueue;
using ZraSyncService;
using ZraSyncService.Zra;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.AddSerilog(Log.Logger, true);
builder.Services.AddHttpClient<ZraService>(client => client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ZraApi:BaseUrl")!));
builder.Services.AddSingleton<IMessageQueueFactory, RabbitMqFactory>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
