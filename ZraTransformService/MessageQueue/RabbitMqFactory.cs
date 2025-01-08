using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PowrIntegration.MessageQueue;

public class RabbitMqFactory(IServiceProvider services, ILogger<RabbitMqFactory> logger) : IAsyncDisposable
{
    private readonly IServiceProvider _services = services;
    private readonly ILogger<RabbitMqFactory> _logger = logger;
    private readonly Dictionary<string, IConnection> _connections = [];

    public async Task<RabbitMqConsumer> CreateConsumer(string host, string queueName, Func<BasicDeliverEventArgs, CancellationToken, Task<bool>> messageHandler, CancellationToken cancellationToken)
    {
        var connection = await GetConnection(host, cancellationToken);

        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        return new RabbitMqConsumer(channel, queueName, messageHandler, _services.GetRequiredService<ILogger<RabbitMqConsumer>>());
    }

    public async Task<RabbitMqPublisher> CreatePublisher(string host, string queueName, CancellationToken cancellationToken)
    {
        var connection = await GetConnection(host, cancellationToken);

        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        return new RabbitMqPublisher(channel, queueName, _services.GetRequiredService<ILogger<RabbitMqPublisher>>());
    }

    private async Task<IConnection> GetConnection(string host, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(host, out var connection))
        {
            connection = await CreateNewConnection(host, cancellationToken);
        }

        return connection!;
    }

    private async Task<IConnection?> CreateNewConnection(string host, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = host,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        var connection = await factory.CreateConnectionAsync(cancellationToken);

        connection.ConnectionShutdownAsync += OnConnectionShutdown;
        connection.RecoveringConsumerAsync += OnRecoveringConsumer;
        connection.RecoverySucceededAsync += OnRecoverySucceeded;
        connection.ConnectionRecoveryErrorAsync += OnConnectionRecoveryError;

        _connections.Add(host, connection);

        return connection;
    }

    private async Task OnRecoverySucceeded(object sender, AsyncEventArgs args)
    {
        _logger.LogInformation("Message queue connection recovery succeeded.");

        await Task.CompletedTask;
    }

    private async Task OnRecoveringConsumer(object sender, RecoveringConsumerEventArgs args)
    {
        _logger.LogInformation("Recovering message queue consumer: {ConsumerTag}", args.ConsumerTag);

        await Task.CompletedTask;
    }

    private async Task OnConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Message queue connection recovery error caused by exception.");

        await Task.CompletedTask;
    }

    private async Task OnConnectionShutdown(object sender, ShutdownEventArgs args)
    {
        _logger.LogInformation("Message queue connection shutdown. Initiator: {Initiator}, ReplyCode: {ReplyCode}, ReplyText: {ReplyText}", args.Initiator, args.ReplyCode, args.ReplyText);
        
        if (args.Exception is not null)
        {
            _logger.LogError(args.Exception, "Message queue connection shutdown caused by exception.");
        }

        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var connection in _connections.Values)
        {
            try
            {
                await connection.CloseAsync();
                connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred disposing of message queue connection.");
            }
        }
    }
}
