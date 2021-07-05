namespace Conventions.Interaction
{
    using Conventions.Interaction.Requests;

    public class Venue : UpsertVenueRequest
    {
        public string Id { get; set; }
    }
}
