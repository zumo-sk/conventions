namespace Conventions.Interaction.Requests
{
    using System;

    public class UpsertConventionRequest
    {
        public string Name { get; set; }

        public string VenueId { get; set; }

        public long? StartDate { get; set; }

        public long? EndDate { get; set; }
    }
}
