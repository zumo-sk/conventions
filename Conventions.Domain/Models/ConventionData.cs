namespace Conventions.Domain.Models
{
    public class ConventionData
    {
        public string Name { get; set; }

        public string VenueId { get; set; }

        public long? StartDate { get; set; }

        public long? EndDate { get; set; }
    }
}
