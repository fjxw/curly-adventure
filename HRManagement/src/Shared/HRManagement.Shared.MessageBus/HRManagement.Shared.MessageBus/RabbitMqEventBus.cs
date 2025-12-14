using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HRManagement.Shared.MessageBus;

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

                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _logger.LogInformation("Успешное подключение к RabbitMQ: {Host}:{Port}", 
                    _settings.HostName, _settings.Port);
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, 
                    "Ошибка подключения к RabbitMQ. Попытка {Attempt}/{MaxAttempts}. Повтор через {Delay}мс...",
                    retryCount, _settings.RetryCount, _settings.RetryDelayMs);

                if (retryCount >= _settings.RetryCount)
                {
                    _logger.LogError(ex, "Не удалось подключиться к RabbitMQ после {MaxAttempts} попыток", 
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

            _logger.LogDebug("Опубликовано событие {EventName} с MessageId {MessageId}", 
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

            _channel!.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: queueName,
                exchange: _settings.ExchangeName,
                routingKey: eventName);

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
                        _logger.LogDebug("Успешно обработано событие {EventName}", eventName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки события {EventName}", eventName);
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Подписка на событие {EventName}, очередь {QueueName}", 
                eventName, queueName);
        }
    }

    private void EnsureConnection()
    {
        if (_connection is not { IsOpen: true })
        {
            _logger.LogWarning("Соединение с RabbitMQ потеряно. Переподключение...");
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

        _logger.LogInformation("Соединение с RabbitMQ закрыто");
    }
}
