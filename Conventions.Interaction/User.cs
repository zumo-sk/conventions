namespace Conventions.Interaction
{
    using Conventions.Interaction.Requests;

    public class User : UpsertUserRequest
    {
        public string Id { get; set; }
    }
}
