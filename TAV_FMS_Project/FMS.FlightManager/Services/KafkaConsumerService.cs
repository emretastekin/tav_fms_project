using Confluent.Kafka;
using Microsoft.Extensions.Caching.Distributed; // Redis için
using System.Text.Json; // JSON işlemleri için

namespace FMS.FlightManager.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache; // Cache eklendi

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger, IConfiguration configuration, IDistributedCache cache)
        {
            _logger = logger;
            _configuration = configuration;
            _cache = cache; // Atama yapıldı
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
            return Task.CompletedTask;
        }

        private void StartConsumerLoop(CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("reference.events");

            _logger.LogInformation("✈️ FlightManager Kafka Consumer başlatıldı. 'reference.events' dinleniyor...");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    var messageJson = consumeResult.Message.Value;

                    // 1. Gelen JSON mesajını parçala
                    using JsonDocument doc = JsonDocument.Parse(messageJson);
                    var entityType = doc.RootElement.GetProperty("entity").GetString(); // "AIRLINE" veya "STATION"
                    var payload = doc.RootElement.GetProperty("payload");
                    var code = payload.GetProperty("code").GetString();

                    // 2. Redis için anahtar oluştur (Örn: airline:THY veya station:IST)
                    var cacheKey = $"{entityType?.ToLower()}:{code?.ToUpper()}";

                    // 3. Sadece payload (içerik) kısmını Redis'e kaydet (24 saat ömürlü)
                    _cache.SetString(cacheKey, payload.GetRawText(), new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                    });

                    _logger.LogInformation($"[KAFKA -> REDIS] {cacheKey} verisi yakalandı ve başarıyla ön belleğe kopyalandı!");
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }
}