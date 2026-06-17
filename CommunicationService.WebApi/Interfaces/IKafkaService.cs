namespace CommunicationService.WebApi.Interfaces
{
    public interface IKafkaService
    {
        public Task ProduceAsync<TKey, TValue>(string topic, TKey key, TValue value, CancellationToken cancellationToken);
    }
}
