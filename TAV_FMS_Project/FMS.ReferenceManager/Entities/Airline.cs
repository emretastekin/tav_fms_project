using System.ComponentModel.DataAnnotations;

namespace FMS.ReferenceManager.Entities
{
    public class Airline
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3)] // ICAO/IATA kodu (Örn: THY, PGT)
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;
    }
}