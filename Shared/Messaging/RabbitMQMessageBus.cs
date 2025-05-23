using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ConcurrentDictionary<string, EventingBasicConsumer> _consumers = new();
    public RabbitMQMessageBus(string connectionString = "amqp://user:password@rabbitmq:5672")
    {
        var factory = new ConnectionFactory { Uri = new Uri(connectionString) };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Создаем основной exchange для событий
        _channel.ExchangeDeclare("university.events", ExchangeType.Topic, durable: true);
    }

    public async Task PublishAsync<T>(T message, string routingKey) where T : class
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish("university.events", routingKey, null, body);
        await Task.CompletedTask;
    }

    public async Task SubscribeAsync<T>(string queueName, string routingKey, Func<T, Task> handler) where T : class
    {
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queueName, "university.events", routingKey);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                    await handler(message);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error processing message: {ex.Message}");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true); // Повтроная попытка
            }
        };

        _consumers[queueName] = consumer;
        await Task.CompletedTask;
    }

    public void StartConsuming()
    {
        foreach (var kvp in _consumers)
        {
            _channel.BasicConsume(kvp.Key, autoAck: false, kvp.Value);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}