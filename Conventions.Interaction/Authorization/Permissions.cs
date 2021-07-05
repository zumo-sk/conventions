namespace Conventions.Interaction.Authorization
{
    public static class Permissions
    {
        public const string ReadUsers = "read:users";
        public const string UpdateUsers = "update:users";
        public const string DeleteUsers = "delete:users";

        public const string CreateVenues = "create:venues";
        public const string UpdateVenues = "update:venues";
        public const string DeleteVenues = "delete:venues";

        public const string CreateConventions = "create:conventions";
        public const string UpdateConventions = "update:conventions";
        public const string DeleteConventions = "delete:conventions";
        public const string SignUpConventions = "signup:conventions";
        public const string EjectConventions = "eject:conventions";

        public const string CreateTalks = "create:talks";
        public const string CreateTalksOnBehalf = "create:talks:onbehalf";
        public const string UpdateTalks = "update:talks";
        public const string DeleteTalks = "delete:talks";
        public const string SignUpTalks = "signup:talks";
        public const string EjectTalks = "eject:talks";
    }
}
