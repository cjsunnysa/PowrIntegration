using FluentResults;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static PowrIntegration.MessageQueue.RabbitMqConsumer;

namespace PowrIntegration.MessageQueue;

public class RabbitMqFactory(IServiceProvider services, ILogger<RabbitMqFactory> logger) : IAsyncDisposable
{
    private readonly IServiceProvider _services = services;
    private readonly ILogger<RabbitMqFactory> _logger = logger;
    private readonly Dictionary<string, (IConnection Connection, IChannel Channel)> _connections = [];

    public async Task<RabbitMqConsumer> CreateConsumer(string host, string queueName, Func<BasicDeliverEventArgs, CancellationToken, Task<MessageAction>> messageHandler, CancellationToken cancellationToken)
    {
        IChannel channel = await GetChannel(host, cancellationToken);

        return new RabbitMqConsumer(channel, queueName, messageHandler, _services.GetRequiredService<ILogger<RabbitMqConsumer>>());
    }

    public async Task<RabbitMqPublisher> CreatePublisher(string host, string queueName, CancellationToken cancellationToken)
    {
        IChannel channel = await GetChannel(host, cancellationToken);

        return new RabbitMqPublisher(channel, queueName, _services.GetRequiredService<ILogger<RabbitMqPublisher>>());
    }

    private async Task<IChannel> GetChannel(string host, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(host, out var connectionChannel))
        {
            connectionChannel = await CreateNewConnectionChannel(host, cancellationToken);
        }

        return connectionChannel.Channel;
    }

    private async Task<(IConnection, IChannel)> CreateNewConnectionChannel(string host, CancellationToken cancellationToken)
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

        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        _connections.Add(host, (connection, channel));

        return (connection, channel);
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
        foreach (var (connection, channel) in _connections.Values)
        {
            try
            {
                await channel.CloseAsync();
                channel.Dispose();
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
