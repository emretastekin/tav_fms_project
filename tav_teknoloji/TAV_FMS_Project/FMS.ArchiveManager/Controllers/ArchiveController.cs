using FMS.ArchiveManager.Data;
using FMS.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FMS.ArchiveManager.Controllers
{
    [Authorize] // Bu kilit sayesinde tokensız kimse bu sınıftaki hiçbir metoda erişemez
    [ApiController]
    [Route("api/[controller]")]
    public class ArchiveController : ControllerBase
    {
        private readonly ArchiveDbContext _context;

        public ArchiveController(ArchiveDbContext context)
        {
            _context = context;
        }

        // Tüm arşivlenmiş uçuşları listele
        [HttpGet]
        [HasPermission("Archives.Read")] //-> (Arşivi görüntüleme)
        public async Task<IActionResult> GetAllArchivedFlights()
        {
            var archives = await _context.ArchivedFlights.ToListAsync();
            return Ok(archives);
        }

        // Belirli bir uçuş numarasına ait arşiv kayıtlarını getir
        [HttpGet("{flightNumber}")]
        [HasPermission("Archives.Read")] //-> (Arşivi görüntüleme)
        public async Task<IActionResult> GetByFlightNumber(string flightNumber)
        {
            var records = await _context.ArchivedFlights
                .Where(f => f.FlightNumber == flightNumber.ToUpper())
                .ToListAsync();

            if (!records.Any())
                return NotFound("Bu uçuş numarasına ait arşiv kaydı bulunamadı.");

            return Ok(records);
        }
    }
}