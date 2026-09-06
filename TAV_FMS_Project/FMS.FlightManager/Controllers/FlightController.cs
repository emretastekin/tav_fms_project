using FMS.FlightManager.Data;
using FMS.FlightManager.Entities;
using FMS.FlightManager.Services;
using FMS.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // Redis için eklendi

namespace FMS.FlightManager.Controllers
{
    [Authorize] // Bu kilit sayesinde tokensız kimse bu sınıftaki hiçbir metoda erişemez
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly FlightDbContext _context;
        private readonly IDistributedCache _cache; // Redis nesnesi eklendi
        private readonly KafkaProducerService _kafkaProducer; // <-- Kafka eklendi

        public FlightController(FlightDbContext context, IDistributedCache cache, KafkaProducerService kafkaProducer)
        {
            _context = context;
            _cache = cache;
            _kafkaProducer = kafkaProducer;
        }
        
        // Herkes (Viewer dahil) uçuşları listeleyebilir, o yüzden sadece [Authorize] kalıyor.
        // TÜM AKTİF UÇUŞLARI LİSTELEME (Vue Dashboard için)
        [HttpGet]
        [HasPermission("Flights.Read")] //-> (GET istekleri için)
        public async Task<IActionResult> GetAllFlights()
        {
            try
            {
                // Entity Framework üzerinden veritabanındaki uçuşları çekiyoruz
                // Not: _context.Flights kısmındaki "Flights" senin DbContext içindeki DbSet adın olmalı.
                var flights = await _context.Flights.ToListAsync(); 
        
                return Ok(flights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        // 1. YENİ UÇUŞ OLUŞTURMA (POST api/flight)
        [HttpPost]
        [HasPermission("Flights.Create")] // <--- Yeni etiketimiz buraya
        public async Task<IActionResult> CreateFlight([FromBody] Flight flight)
        {
            if (string.IsNullOrEmpty(flight.FlightNumber))
                return BadRequest("Uçuş numarası boş olamaz.");

            // DÜZELTME 1: "ref:airline:" formatına dönüştürüldü
            var airlineCacheKey = $"ref:airline:{flight.AirlineCode.ToUpper()}";
            var existingAirline = await _cache.GetStringAsync(airlineCacheKey);
            if (string.IsNullOrEmpty(existingAirline))
            {
                return BadRequest($"HATA: '{flight.AirlineCode}' kodlu havayolu sistemde bulunamadı. Önce Reference Manager'dan eklenmelidir.");
            }

            // DÜZELTME 1: "ref:station:" formatına dönüştürüldü
            var depStationKey = $"ref:station:{flight.DepartureStation.ToUpper()}";
            var existingDepStation = await _cache.GetStringAsync(depStationKey);
            if (string.IsNullOrEmpty(existingDepStation))
            {
                return BadRequest($"HATA: Kalkış istasyonu '{flight.DepartureStation}' sistemde bulunamadı.");
            }

            // DÜZELTME 1: "ref:station:" formatına dönüştürüldü
            var arrStationKey = $"ref:station:{flight.ArrivalStation.ToUpper()}";
            var existingArrStation = await _cache.GetStringAsync(arrStationKey);
            if (string.IsNullOrEmpty(existingArrStation))
            {
                return BadRequest($"HATA: Varış istasyonu '{flight.ArrivalStation}' sistemde bulunamadı.");
            }
            
            try
            {
                flight.ScheduleTime = DateTime.SpecifyKind(flight.ScheduleTime, DateTimeKind.Utc);

                _context.Flights.Add(flight);
                await _context.SaveChangesAsync(); 

                // DÜZELTME 2: Kafka'ya uçuşun yaratıldığını haber veriyoruz (FAS servisi için gerekli)[cite: 1]
                var flightEvent = new
                {
                    eventId = Guid.NewGuid().ToString(),
                    eventType = "CREATED",
                    entity = "FLIGHT",
                    payload = flight,
                    createdAt = DateTime.UtcNow.ToString("O"),
                    producer = "FlightManager"
                };

                // Mesajı "flight.events" kanalı üzerinden Kafka'ya fırlat
                await _kafkaProducer.PublishEventAsync("flight.events", flightEvent);

                return Ok(flight);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 2. UÇUŞ NUMARASI İLE SORGULAMA (GET api/flight/TK1903)
        [HttpGet("{flightNumber}")]
        [HasPermission("Flights.Read")] //-> (GET istekleri için)
        public async Task<IActionResult> GetFlight(string flightNumber)
        {
            var flight = await _context.Flights
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber.ToUpper());

            if (flight == null)
                return NotFound("Belirtilen numaraya ait uçuş bulunamadı.");

            return Ok(flight);
        }
        
        
        // 3. UÇUŞ STATÜSÜ GÜNCELLEME VE EVENT FIRLATMA (PUT api/flight/{flightNumber}/status)
        // SADECE Admin ve Operator iniş (LANDED) işlemi yapabilir
        [HttpPut("{flightNumber}/status")]
        [HasPermission("Flights.Update")] //-> (PUT/PATCH istekleri için)
        public async Task<IActionResult> UpdateFlightStatus(string flightNumber, [FromBody] string newStatus)
        {
            var flight = await _context.Flights
                .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber.ToUpper());

            if (flight == null)
                return NotFound("Güncellenecek uçuş bulunamadı.");

            // Statüyü güncelle ve veritabanına kaydet
            flight.Status = newStatus.ToUpper();
            await _context.SaveChangesAsync();

            // Kafka'ya uçuşun güncellendiğini haber ver
            var flightEvent = new
            {
                eventId = Guid.NewGuid().ToString(),
                eventType = "STATUS_UPDATED",
                entity = "FLIGHT",
                payload = flight,
                createdAt = DateTime.UtcNow.ToString("O"),
                producer = "FlightManager"
            };

            // Bu kez mesajı "flight.events" isimli yeni bir kanala atıyoruz
            // DİKKAT: _kafkaProducer nesnesini DI (Constructor) ile sınıfa eklemeyi unutma!
            await _kafkaProducer.PublishEventAsync("flight.events", flightEvent);

            return Ok(flight);
        }
        
        
        // 4. TOPLU UÇUŞ YÜKLEME (POST api/flight/upload)
        [HttpPost("upload")]
        [HasPermission("Flights.Create")] //-> (POST istekleri için - bunu zaten yaptık)
        public async Task<IActionResult> UploadFlights(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen geçerli bir CSV dosyası seçin.");

            var addedFlights = new List<Flight>();
            var errorLines = new List<string>();

            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                // İlk satırı (Başlıkları - Header) atla
                var header = await stream.ReadLineAsync();

                int lineNumber = 1;
                while (!stream.EndOfStream)
                {
                    lineNumber++;
                    var line = await stream.ReadLineAsync();
                    var values = line.Split(',');

                    // CSV formatımız: FlightNumber,AirlineCode,DepStation,ArrStation,ScheduleTime
                    // Örnek: TK1905,THY,IST,ESB,2026-07-25T14:30:00Z
                    if (values.Length < 5)
                    {
                        errorLines.Add($"Satır {lineNumber}: Eksik sütun.");
                        continue;
                    }

                    try
                    {
                        var flight = new Flight
                        {
                            FlightNumber = values[0].Trim(),
                            AirlineCode = values[1].Trim(),
                            DepartureStation = values[2].Trim(),
                            ArrivalStation = values[3].Trim(),
                            // Yeni ve Güvenli Kod:
                            ScheduleTime = DateTime.Parse(values[4].Trim(), null, System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime(),
                            Status = "SCHEDULED"
                        };

                        // Uçuşu listeye ekle (Henüz veritabanına kaydetmiyoruz, toplu kaydedeceğiz)
                        addedFlights.Add(flight);
                    }
                    catch (Exception ex)
                    {
                        errorLines.Add($"Satır {lineNumber}: Veri formatı hatalı. ({ex.Message})");
                    }
                }
            }

            if (addedFlights.Any())
            {
                _context.Flights.AddRange(addedFlights);
                await _context.SaveChangesAsync();

                // Her bir uçuş için Kafka'ya event fırlatıyoruz
                foreach (var flight in addedFlights)
                {
                    var flightEvent = new
                    {
                        eventId = Guid.NewGuid().ToString(),
                        eventType = "CREATED",
                        entity = "FLIGHT",
                        payload = flight,
                        createdAt = DateTime.UtcNow.ToString("O"),
                        producer = "FlightManager"
                    };
                    await _kafkaProducer.PublishEventAsync("flight.events", flightEvent);
                }
            }

            return Ok(new 
            { 
                message = $"{addedFlights.Count} uçuş başarıyla eklendi.", 
                errors = errorLines 
            });
        }
        
        /*
        // REDIS'İ TEST VERİLERİYLE DOLDURMA (Geçici Test Endpoint'i)
        [HttpGet("seed-redis")]
        [AllowAnonymous] // Token'a gerek kalmadan tarayıcıdan tetikleyebilmek için
        public async Task<IActionResult> SeedRedis()
        {
            try
            {
                // 1. Havayollarını Redis'e Yazıyoruz (Senin tablodaki kodlara göre)
                await _cache.SetStringAsync("ref:airline:THY", "Turkish Airlines");
                await _cache.SetStringAsync("ref:airline:PGS", "Pegasus");
                await _cache.SetStringAsync("ref:airline:SUN", "SunExpress");
                await _cache.SetStringAsync("ref:airline:AJT", "Ajet");
                await _cache.SetStringAsync("ref:airline:KLM", "KLM Royal Dutch Airlines");
                await _cache.SetStringAsync("ref:airline:DLH", "Lufthansa");

                // 2. İstasyonları Redis'e Yazıyoruz (Senin tablodaki kodlara göre)
                await _cache.SetStringAsync("ref:station:IST", "Istanbul Havalimani");
                await _cache.SetStringAsync("ref:station:ESB", "Esenboga Havalimani");
                await _cache.SetStringAsync("ref:station:AYT", "Antalya Havalimani");
                await _cache.SetStringAsync("ref:station:ADB", "Izmir Adnan Menderes");
                await _cache.SetStringAsync("ref:station:AMS", "Amsterdam Schiphol");
                await _cache.SetStringAsync("ref:station:JFK", "John F. Kennedy");

                return Ok("HARİKA! PostgreSQL'deki test verileri başarıyla Redis Cache'e aktarıldı.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Redis'e yazılırken hata oluştu: {ex.Message}");
            }
        }
        */
        
        
        
    }
}