public interface IMessageBus
{
    Task PublishAsync<T>(T message, string routingKey) where T : class;
    Task SubscribeAsync<T>(string queueName, string routingKey, Func<T, Task> handler) where T : class;
    void StartConsuming();
}