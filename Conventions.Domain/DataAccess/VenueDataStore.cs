namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Helpers;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;

    public class VenueDataStore : IVenueDataStore
    {
        private const string TableName = "venues";

        private readonly IConnectionFactory connectionFactory;

        public VenueDataStore(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<Venue> CreateVenueAsync(VenueData venueData, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var id = await ConnectionFactory.InsertAsync(connection, TableName, venueData.ToDictionary(), token);

                var venue = new Venue { Id = id };
                venueData.CopyTo(venue);

                return venue;
            }
        }

        public async Task<Venue> UpdateVenueAsync(string venueId, VenueData venueData, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.UpdateAsync(
                    connection,
                    TableName,
                    venueData.ToDictionary(),
                    new KeyValuePair<string, object>(DataHelpers.Id, venueId),
                    token);

                var venue = new Venue { Id = venueId };
                venueData.CopyTo(venue);

                return venue;
            }
        }

        public async Task DeleteVenueAsync(string venueId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.DeleteAsync(connection, TableName, new Dictionary<string, object> { { DataHelpers.Id, venueId } }, token);
            }
        }

        public async Task<Venue> GetVenueAsync(string venueId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var result = await ConnectionFactory.ListAsync<Venue>(
                                 connection,
                                 TableName,
                                 new Dictionary<string, object> { { DataHelpers.Id, venueId } },
                                 1,
                                 1,
                                 token);

                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Venue>> ListVenuesAsync(int page, int pageSize, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                return await ConnectionFactory.ListAsync<Venue>(connection, TableName, null, page, pageSize, token);
            }
        }
    }
}
