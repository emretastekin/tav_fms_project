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
    public class AirlineController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly KafkaProducerService _kafkaProducer; 

        public AirlineController(AppDbContext context, IDistributedCache cache, KafkaProducerService kafkaProducer)
        {
            _context = context;
            _cache = cache;
            _kafkaProducer = kafkaProducer; 
        }

        // SADECE Admin yeni havayolu ekleyebilir (Operator veya Viewer ekleyemez)
        [HttpPost]
        [HasPermission("Airlines.Create")]
        public async Task<IActionResult> Create([FromBody] Airline airline)
        {
            if (string.IsNullOrEmpty(airline.Code) || airline.Code.Length > 3)
                return BadRequest("Geçersiz havayolu kodu.");

            _context.Airlines.Add(airline);
            await _context.SaveChangesAsync();

            // DÜZELTME 1: Analiz dokümanındaki şemaya tam uyum (ref:airline:THY)
            var cacheKey = $"ref:airline:{airline.Code.ToUpper()}";
            var cacheValue = JsonSerializer.Serialize(airline);
            
            await _cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // 24 saat TTL kuralı[cite: 1]
            });

            var referenceEvent = new
            {
                eventId = Guid.NewGuid().ToString(),
                eventType = "CREATED",
                entity = "AIRLINE",
                payload = airline,
                createdAt = DateTime.UtcNow.ToString("O"), 
                producer = "ReferenceManager"
            };

            await _kafkaProducer.PublishEventAsync("reference.events", referenceEvent);

            return Ok(airline);
        }

        // Giriş yapmış tüm yetkili roller (Admin, Operator, Viewer) sorgulama yapabilir
        [HttpGet("{code}")]
        [HasPermission("Airlines.Read")]
        public async Task<IActionResult> GetByCode(string code)
        {
            // DÜZELTME 1: Aynı anahtar formatı
            var cacheKey = $"ref:airline:{code.ToUpper()}";

            var cachedAirline = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedAirline))
            {
                var airlineFromCache = JsonSerializer.Deserialize<Airline>(cachedAirline);
                // DÜZELTME 2: Frontend'i bozmamak için doğrudan objeyi dönüyoruz
                return Ok(airlineFromCache);
            }

            var airlineFromDb = await _context.Airlines
                .FirstOrDefaultAsync(a => a.Code == code.ToUpper());

            if (airlineFromDb == null)
                return NotFound("Havayolu bulunamadı.");

            var cacheValue = JsonSerializer.Serialize(airlineFromDb);
            await _cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

            // DÜZELTME 2: Doğrudan objeyi dönüyoruz
            return Ok(airlineFromDb);
        }
    }
}