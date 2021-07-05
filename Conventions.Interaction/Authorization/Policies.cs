namespace Conventions.Interaction.Authorization
{
    using System.Security.Claims;

    public static class Policies
    {
        public const string ReadUsers = nameof(ReadUsers);
        public const string UpdateUsers = nameof(UpdateUsers);
        public const string DeleteUsers = nameof(DeleteUsers);

        public const string CreateVenues = nameof(CreateVenues);
        public const string UpdateVenues = nameof(UpdateVenues);
        public const string DeleteVenues = nameof(DeleteVenues);

        public const string CreateConventions = nameof(CreateConventions);
        public const string UpdateConventions = nameof(UpdateConventions);
        public const string DeleteConventions = nameof(DeleteConventions);
        public const string SignUpConventions = nameof(SignUpConventions);
        public const string EjectConventions = nameof(EjectConventions);

        public const string CreateTalks = nameof(CreateTalks);
        public const string UpdateTalks = nameof(UpdateTalks);
        public const string DeleteTalks = nameof(DeleteTalks);
        public const string SignUpTalks = nameof(SignUpTalks);
        public const string EjectTalks = nameof(EjectTalks);

        public static bool CanAccess(this ClaimsPrincipal claimsPrincipal, string userId, string permission)
        {
            // if `xxx:users` permission not assigned, compare whether user is accessing their own data
            if (claimsPrincipal.HasClaim(c => c.Type == "permissions" && c.Value == permission) == false)
            {
                var identity = claimsPrincipal.Identity?.Name;
                if (identity != userId)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
