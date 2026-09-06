using System.ComponentModel.DataAnnotations;

namespace FMS.ReferenceManager.Entities
{
    public class Station
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3)] // Havalimanı kodu (Örn: IST, ESB)
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;
    }
}