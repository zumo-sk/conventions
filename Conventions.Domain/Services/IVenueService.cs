namespace Conventions.Domain.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;

    public interface IVenueService
    {
        Task<Venue> CreateVenueAsync(VenueData venueData, CancellationToken token = default);

        Task<Venue> UpdateVenueAsync(string venueId, VenueData venueData, CancellationToken token = default);

        Task DeleteVenueAsync(string venueId, CancellationToken token = default);

        Task<IEnumerable<Venue>> ListVenuesAsync(int page, int pageSize, CancellationToken token = default);

        Task<Venue> GetVenueAsync(string venueId, CancellationToken token = default);
    }
}
