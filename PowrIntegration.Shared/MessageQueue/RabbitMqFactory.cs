using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowrIntegration.Shared.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace PowrIntegration.Shared.MessageQueue;

public abstract class RabbitMqFactory<TPublisher, TConsumer>(
    IOptions<RabbitMqOptions> options,
    ILogger logger) : IAsyncDisposable
    where TPublisher : RabbitMqPublisher
    where TConsumer : RabbitMqConsumer
{
    protected readonly RabbitMqOptions Options = options.Value;
    private readonly ILogger _logger = logger;
    private readonly Dictionary<string, (IConnection Connection, IChannel Channel)> _connections = [];

    public abstract Task<TPublisher> CreatePublisher(CancellationToken cancellationToken);

    public abstract Task<TConsumer> CreateConsumer(CancellationToken cancellationToken);

    protected async Task<IChannel> GetChannel(MessageQueueOptions options, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(options.Host, out var connectionChannel))
        {
            connectionChannel = await CreateNewConnectionChannel(options, cancellationToken);
        }

        return connectionChannel.Channel;
    }

    private async Task<(IConnection, IChannel)> CreateNewConnectionChannel(MessageQueueOptions options, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = options.Host,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                var connection = await factory.CreateConnectionAsync(cancellationToken);

                connection.ConnectionShutdownAsync += OnConnectionShutdown;
                connection.RecoveringConsumerAsync += OnRecoveringConsumer;
                connection.RecoverySucceededAsync += OnRecoverySucceeded;
                connection.ConnectionRecoveryErrorAsync += OnConnectionRecoveryError;

                var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                _connections.Add(options.Host, (connection, channel));

                return (connection, channel);
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "RabbitMQ broker at host: {RabbitMqHost} couldn't be reached. Retrying after delay.", options.Host);

                await Task.Delay(options.ConnectionRetryTimeoutSeconds * 1000, cancellationToken);
            }
        }
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
