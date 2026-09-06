using Confluent.Kafka;
using System.Text.Json;

namespace FMS.FlightManager.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;

        public KafkaProducerService(IConfiguration configuration)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };
            
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task PublishEventAsync(string topic, object message)
        {
            var payload = JsonSerializer.Serialize(message);
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = payload });
        }
    }
}