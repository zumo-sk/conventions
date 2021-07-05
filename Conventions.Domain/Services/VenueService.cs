namespace Conventions.Domain.Services
{
    using Conventions.Domain.Models;
    using Conventions.Domain.DataAccess;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class VenueService : IVenueService
    {
        private readonly IVenueDataStore venueDataStore;

        public VenueService(IVenueDataStore venueDataStore)
        {
            this.venueDataStore = venueDataStore;
        }

        public Task<Venue> CreateVenueAsync(VenueData venueData, CancellationToken token = default) => venueDataStore.CreateVenueAsync(venueData, token);

        public Task<Venue> UpdateVenueAsync(string venueId, VenueData venueData, CancellationToken token = default) => venueDataStore.UpdateVenueAsync(venueId, venueData, token);

        public Task DeleteVenueAsync(string venueId, CancellationToken token = default) => venueDataStore.DeleteVenueAsync(venueId, token);

        public Task<Venue> GetVenueAsync(string venueId, CancellationToken token = default) => venueDataStore.GetVenueAsync(venueId, token);

        public Task<IEnumerable<Venue>> ListVenuesAsync(int page, int pageSize, CancellationToken token = default) => venueDataStore.ListVenuesAsync(page, pageSize, token);
    }
}
