using Confluent.Kafka;
using System.Text.Json;

namespace FMS.ReferenceManager.Services
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

        // Kafka'ya olay fırlatacak metodumuz
        public async Task PublishEventAsync(string topic, object message)
        {
            var payload = JsonSerializer.Serialize(message);
            
            // Null, string -> Anahtar kullanmıyoruz, sadece JSON string mesaj gönderiyoruz
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = payload });
        }
    }
}