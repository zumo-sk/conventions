namespace Conventions.Domain.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.DataAccess;
    using Conventions.Domain.Exceptions;
    using Conventions.Domain.Models;

    public class ConventionService : IConventionService
    {
        private readonly IConventionDataStore conventionDataStore;
        private readonly IConventionRegistrationDataStore conventionRegistrationDataStore;
        private readonly IVenueService venueService;
        private readonly IUserService userService;

        public ConventionService(
            IConventionDataStore conventionDataStore,
            IConventionRegistrationDataStore conventionRegistrationDataStore,
            IVenueService venueService,
            IUserService userService)
        {
            this.conventionDataStore = conventionDataStore;
            this.conventionRegistrationDataStore = conventionRegistrationDataStore;
            this.venueService = venueService;
            this.userService = userService;
        }

        public async Task<Convention> CreateConventionAsync(ConventionData conventionData, CancellationToken token = default)
        {
            var venue = await venueService.GetVenueAsync(conventionData.VenueId, token);
            if (venue == null)
            {
                throw new VenueNotFoundException($"Venue `{conventionData.VenueId}` not found.");
            }

            return await conventionDataStore.CreateConventionAsync(conventionData, token);
        }

        public async Task<Convention> UpdateConventionAsync(string conventionId, ConventionData conventionData, CancellationToken token = default)
        {
            var venue = await venueService.GetVenueAsync(conventionData.VenueId, token);
            if (venue == null)
            {
                throw new VenueNotFoundException($"Venue `{conventionData.VenueId}` not found.");
            }

            return await conventionDataStore.UpdateConventionAsync(conventionId, conventionData, token);
        }

        public Task DeleteConventionAsync(string conventionId, CancellationToken token = default) =>
            conventionDataStore.DeleteConventionAsync(conventionId, token);

        public Task<Convention> GetConventionAsync(string conventionId, CancellationToken token = default) =>
            conventionDataStore.GetConventionAsync(conventionId, token);

        public Task<IEnumerable<Convention>> ListConventionsAsync(int page, int pageSize, CancellationToken token = default) =>
            conventionDataStore.ListConventionsAsync(page, pageSize, token);

        public async Task JoinConventionAsync(string conventionId, string userId, CancellationToken token = default)
        {
            var conventionTask = conventionDataStore.GetConventionAsync(conventionId, token);
            var userTask = userService.GetUserAsync(userId, token);
            var alreadyJoined = conventionRegistrationDataStore.HasAlreadyJoinedAsync(conventionId, userId, token);

            if (await conventionTask == null)
            {
                throw new ConventionNotFoundException($"Convention `{conventionId}` not found.");
            }

            if (await userTask == null)
            {
                throw new UserNotFoundException($"User `{userId}` not found.");
            }

            if (await alreadyJoined)
            {
                throw new AlreadyJoinedException($"User `{userId}` already joined convention `{conventionId}`.");
            }

            await conventionRegistrationDataStore.JoinConventionAsync(conventionId, userId, token);
        }

        public async Task LeaveConventionAsync(string conventionId, string userId, CancellationToken token) =>
            await conventionRegistrationDataStore.LeaveConventionAsync(conventionId, userId, token);

        public async Task<IEnumerable<Convention>> ListConventionsForUserAsync(string userId, int page, int pageSize, CancellationToken token = default) =>
            await conventionRegistrationDataStore.ListConventionsForUserAsync(userId, page, pageSize, token);
    }
}
