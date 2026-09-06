using Confluent.Kafka;
using FMS.ArchiveManager.Data;
using FMS.ArchiveManager.Entities;
using System.Text.Json;

namespace FMS.ArchiveManager.Services
{
    public class KafkaArchiveConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<KafkaArchiveConsumer> _logger;

        public KafkaArchiveConsumer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<KafkaArchiveConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            // Web sunucusunun ayağa kalkması için kısa bir süre bekle
            await Task.Delay(2000, stoppingToken);
            
            var config = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = "archive-manager-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe("flight.events");

            _logger.LogInformation("ArchiveManager Consumer başlatıldı...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    var messageJson = result.Message.Value;

                    using var doc = JsonDocument.Parse(messageJson);
                    var root = doc.RootElement;

                    // Güvenli erişim: Önce 'payload' var mı bak
                    if (root.TryGetProperty("payload", out var payload))
                    {
                        // 'status' veya 'Status' olma ihtimaline karşı güvenli kontrol
                        var statusElement = payload.TryGetProperty("status", out var s) ? s : 
                            (payload.TryGetProperty("Status", out var S) ? S : default);

                        var status = statusElement.GetString();
                        
                        if (status == "LANDED")
                        {
                            _logger.LogInformation("LANDED statüsü yakalandı, arşivleniyor...");

                            using var scope = _serviceProvider.CreateScope();
                            var db = scope.ServiceProvider.GetRequiredService<ArchiveDbContext>();
                            
                            // flightNumber bilgisini güvenli al
                            // payload içindeki verileri doğru key'lerle (büyük/küçük harfe dikkat ederek) alalım
                            var flightNum = payload.TryGetProperty("FlightNumber", out var fn) ? fn.GetString() : "Bilinmiyor";
                            var id = payload.TryGetProperty("Id", out var i) ? i.ToString() : "0";

                            db.ArchivedFlights.Add(new ArchivedFlight
                            {
                                OriginalFlightId = id,
                                FlightNumber = flightNum,
                                ArchivedAt = DateTime.UtcNow
                                // Not: DepartureTime ve ArrivalTime'ı da JSON'dan bu şekilde çekebilirsin
                            });
                            
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Uçuş başarıyla arşivlendi: {FlightNumber}", flightNum);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kafka mesajı işlenirken hata oluştu.");
                }
            }
        }
    }
}