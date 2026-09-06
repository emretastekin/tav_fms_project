namespace FMS.ArchiveManager.Entities
{
    public class ArchivedFlight
    {
        public int Id { get; set; } // Arşivdeki benzersiz ID
        public string OriginalFlightId { get; set; } // FlightManager'daki ID
        public string FlightNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
    }
}