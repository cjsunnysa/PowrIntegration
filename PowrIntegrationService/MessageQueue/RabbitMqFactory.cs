using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.Shared.MessageQueue;
using PowrIntegrationService.Data;
using PowrIntegrationService.Options;
using PowrIntegrationService.Powertill;
using PowrIntegrationService.Zra;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace PowrIntegrationService.MessageQueue;

public class RabbitMqFactory(
    IOptions<RabbitMqOptions> options,
    IServiceProvider services,
    ILogger<RabbitMqFactory> logger) : IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private readonly IServiceProvider _services = services;
    private readonly ILogger<RabbitMqFactory> _logger = logger;
    private readonly Dictionary<string, (IConnection Connection, IChannel Channel)> _connections = [];

    public async Task<RabbitMqConsumer> CreateConsumer<T>(CancellationToken cancellationToken)
        where T : RabbitMqConsumer
    {
        if (typeof(T) == typeof(BackOfficeQueueConsumer))
        {
            var publisher = await CreateBackOfficeConsumer(cancellationToken);

            return publisher as T ?? throw new InvalidCastException("An error occured creating a message queue consumer.");
        }

        if (typeof(T) == typeof(ZraQueueConsumer))
        {
            var publisher = await CreateZraConsumer(cancellationToken);

            return publisher as T ?? throw new InvalidCastException("An error occured creating a message queue consumer.");
        }

        throw new InvalidOperationException("An error occured creating a message queue publisher. Unknown message queue type requested.");
    }

    private async Task<ZraQueueConsumer> CreateZraConsumer(CancellationToken cancellationToken)
    {
        IChannel channel = await GetChannel(_options.ApiQueue.Host, cancellationToken);

        return new ZraQueueConsumer(
            channel,
            _options.ApiQueue,
            _services.GetRequiredService<ZraService>(),
            _services.GetRequiredService<ILogger<ZraQueueConsumer>>());
    }

    private async Task<BackOfficeQueueConsumer> CreateBackOfficeConsumer(CancellationToken cancellationToken)
    {
        IChannel channel = await GetChannel(_options.BackOfficeQueue.Host, cancellationToken);

        return new BackOfficeQueueConsumer(
            channel,
            _options.BackOfficeQueue,
            _services.GetRequiredService<IDbContextFactory<PowrIntegrationDbContext>>(),
            _services.GetRequiredService<PurchaseFileExport>(),
            _services.GetRequiredService<ILogger<BackOfficeQueueConsumer>>());
    }

    public async Task<T> CreatePublisher<T>(CancellationToken cancellationToken)
        where T : RabbitMqPublisher
    {
        if (typeof(T) == typeof(BackOfficeQueuePublisher))
        {
            var publisher = await CreateBackOfficePublisher(cancellationToken);

            return publisher as T ?? throw new InvalidCastException("An error occured creating a message queue publisher.");
        }

        if (typeof(T) == typeof(ZraQueuePublisher))
        {
            var publisher = await CreateZraPublisher(cancellationToken);

            return publisher as T ?? throw new InvalidCastException("An error occured creating a message queue publisher.");
        }

        throw new InvalidOperationException("An error occured creating a message queue publisher. Unknown message queue type requested.");
    }

    private async Task<RabbitMqPublisher> CreateZraPublisher(CancellationToken cancellationToken)
    {
        IChannel channel = await GetChannel(_options.ApiQueue.Host, cancellationToken);

        return new ZraQueuePublisher(
            channel,
            _options.ApiQueue,
            _services.GetRequiredService<ILogger<ZraQueuePublisher>>());
    }

    private async Task<BackOfficeQueuePublisher> CreateBackOfficePublisher(CancellationToken cancellationToken)
    {
        IChannel channel = await GetChannel(_options.BackOfficeQueue.Host, cancellationToken);

        return new BackOfficeQueuePublisher(
            channel,
            _options.BackOfficeQueue,
            _services.GetRequiredService<ILogger<BackOfficeQueuePublisher>>());
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
        while (true)
        {
            try
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
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "RabbitMQ broker at host: {RabbitMqHost} couldn't be reached. Retrying after delay.", host);

                await Task.Delay(_options.ConnectionRetryTimeoutSeconds * 1000, cancellationToken);
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
