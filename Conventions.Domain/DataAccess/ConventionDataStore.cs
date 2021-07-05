namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Helpers;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;

    public class ConventionDataStore : IConventionDataStore
    {
        private const string TableName = "conventions";

        private readonly IConnectionFactory connectionFactory;

        public ConventionDataStore(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<Convention> CreateConventionAsync(ConventionData conventionData, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var id = await ConnectionFactory.InsertAsync(connection, TableName, conventionData.ToDictionary(), token);

                var convention = new Convention { Id = id };
                conventionData.CopyTo(convention);

                return convention;
            }
        }

        public async Task<Convention> UpdateConventionAsync(string conventionId, ConventionData conventionData, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.UpdateAsync(
                    connection,
                    TableName,
                    conventionData.ToDictionary(),
                    new KeyValuePair<string, object>(DataHelpers.Id, conventionId),
                    token);

                var convention = new Convention { Id = conventionId };
                conventionData.CopyTo(convention);

                return convention;
            }
        }

        public async Task DeleteConventionAsync(string conventionId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.DeleteAsync(
                    connection,
                    TableName,
                    new Dictionary<string, object> { { DataHelpers.Id, conventionId } },
                    token);
            }
        }

        public async Task<Convention> GetConventionAsync(string conventionId, CancellationToken token)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var result = await ConnectionFactory.ListAsync<Convention>(
                                 connection,
                                 TableName,
                                 new Dictionary<string, object> { { DataHelpers.Id, conventionId } },
                                 1,
                                 1,
                                 token);

                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Convention>> ListConventionsAsync(int page, int pageSize, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                return await ConnectionFactory.ListAsync<Convention>(connection, TableName, null, page, pageSize, token);
            }
        }
    }
}
