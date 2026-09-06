using System.ComponentModel.DataAnnotations;

namespace FMS.FlightManager.Entities
{
    public class Flight
    {
        public int Id { get; set; }

        [Required]
        public string FlightNumber { get; set; } = string.Empty; // Örn: TK1903

        [Required]
        public string AirlineCode { get; set; } = string.Empty; // ReferenceManager'dan gelecek kod

        [Required]
        public string DepartureStation { get; set; } = string.Empty; // Örn: IST

        [Required]
        public string ArrivalStation { get; set; } = string.Empty; // Örn: JFK

        public DateTime ScheduleTime { get; set; } // Planlanan Uçuş Zamanı
        
        public string Status { get; set; } = "SCHEDULED"; // SCHEDULED, DELAYED, BOARDING, DEPARTED, LANDED
    }
}