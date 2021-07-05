namespace Conventions.Interaction
{
    using Conventions.Interaction.Requests;

    public class Talk : UpsertTalkRequest
    {
        public string Id { get; set; }
    }
}
