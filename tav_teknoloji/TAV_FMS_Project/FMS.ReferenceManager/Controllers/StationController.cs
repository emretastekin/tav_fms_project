using FMS.ReferenceManager.Data;
using FMS.ReferenceManager.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using FMS.ReferenceManager.Services;
using FMS.Security.Authorization;
using Microsoft.AspNetCore.Authorization; 

namespace FMS.ReferenceManager.Controllers
{
    [Authorize] // Sınıf genelinde tokensız erişimi engelliyoruz
    [ApiController]
    [Route("api/[controller]")]
    public class StationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly KafkaProducerService _kafkaProducer; 

        public StationController(AppDbContext context, IDistributedCache cache, KafkaProducerService kafkaProducer)
        {
            _context = context;
            _cache = cache;
            _kafkaProducer = kafkaProducer;
        }

        // SADECE Admin yeni istasyon/havalimanı ekleyebilir
        [HttpPost]
        [HasPermission("Stations.Create")]
        public async Task<IActionResult> Create([FromBody] Station station)
        {
            if (string.IsNullOrEmpty(station.Code) || station.Code.Length > 3)
                return BadRequest("Geçersiz istasyon kodu.");

            _context.Stations.Add(station);
            await _context.SaveChangesAsync();

            // DÜZELTME 1: FlightManager'ın aradığı tam format[cite: 1]
            var cacheKey = $"ref:station:{station.Code.ToUpper()}";
            var cacheValue = JsonSerializer.Serialize(station);
            
            await _cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });
            
            var referenceEvent = new
            {
                eventId = Guid.NewGuid().ToString(),
                eventType = "CREATED",
                entity = "STATION",
                payload = station,
                createdAt = DateTime.UtcNow.ToString("O"), 
                producer = "ReferenceManager"
            };

            await _kafkaProducer.PublishEventAsync("reference.events", referenceEvent);

            return Ok(station);
        }

        [HttpGet("{code}")]
        [HasPermission("Stations.Read")]
        public async Task<IActionResult> GetByCode(string code)
        {
            // DÜZELTME 1: Aynı anahtar formatı
            var cacheKey = $"ref:station:{code.ToUpper()}";

            var cachedStation = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedStation))
            {
                var stationFromCache = JsonSerializer.Deserialize<Station>(cachedStation);
                // DÜZELTME 2: Doğrudan nesneyi dön
                return Ok(stationFromCache);
            }

            var stationFromDb = await _context.Stations
                .FirstOrDefaultAsync(s => s.Code == code.ToUpper());

            if (stationFromDb == null)
                return NotFound("İstasyon bulunamadı.");

            var cacheValue = JsonSerializer.Serialize(stationFromDb);
            await _cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            // DÜZELTME 2: Doğrudan nesneyi dön
            return Ok(stationFromDb);
        }
    }
}