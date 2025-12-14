using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HRManagement.Shared.MessageBus;

/// <summary>
/// RabbitMQ implementation of IEventBus for inter-service communication.
/// </summary>
public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();
    private bool _disposed;

    public RabbitMqEventBus(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqEventBus> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            DispatchConsumersAsync = true
        };

        var retryCount = 0;
        while (retryCount < _settings.RetryCount)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare a topic exchange for event routing
                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("Successfully connected to RabbitMQ at {Host}:{Port}", 
                    _settings.HostName, _settings.Port);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, 
                    "Failed to connect to RabbitMQ. Attempt {Attempt}/{MaxAttempts}. Retrying in {Delay}ms...",
                    retryCount, _settings.RetryCount, _settings.RetryDelayMs);

                if (retryCount >= _settings.RetryCount)
                {
                    _logger.LogError(ex, "Could not connect to RabbitMQ after {MaxAttempts} attempts", 
                        _settings.RetryCount);
                    throw;
                }

                Thread.Sleep(_settings.RetryDelayMs);
            }
        }
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqEventBus));

        var eventName = typeof(T).Name;
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);

        lock (_lock)
        {
            EnsureConnection();

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Type = eventName;

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: eventName,
                mandatory: false,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Published event {EventName} with MessageId {MessageId}", 
                eventName, properties.MessageId);
        }

        return Task.CompletedTask;
    }

    public void Subscribe<T>(Func<T, Task> handler) where T : class
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqEventBus));

        var eventName = typeof(T).Name;
        var queueName = $"{eventName}_queue";

        lock (_lock)
        {
            EnsureConnection();

            // Declare a durable queue for this event type
            _channel!.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Bind queue to exchange with routing key matching event name
            _channel.QueueBind(
                queue: queueName,
                exchange: _settings.ExchangeName,
                routingKey: eventName);

            // Set prefetch count for fair dispatch
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var @event = JsonSerializer.Deserialize<T>(message);

                    if (@event != null)
                    {
                        await handler(@event);
                        _channel.BasicAck(ea.DeliveryTag, multiple: false);
                        _logger.LogDebug("Successfully processed event {EventName}", eventName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventName}", eventName);
                    // Negative acknowledgement - requeue the message
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Subscribed to event {EventName} on queue {QueueName}", 
                eventName, queueName);
        }
    }

    private void EnsureConnection()
    {
        if (_connection is not { IsOpen: true })
        {
            _logger.LogWarning("RabbitMQ connection lost. Attempting to reconnect...");
            InitializeConnection();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();

        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
