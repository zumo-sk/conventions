namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;

    public class ConventionRegistrationDataStore : IConventionRegistrationDataStore
    {
        private const string TableName = "conventionRegistrations";

        private readonly IConnectionFactory connectionFactory;

        public ConventionRegistrationDataStore(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task JoinConventionAsync(string conventionId, string userId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.InsertAsync(
                    connection,
                    TableName,
                    new Dictionary<string, object> { { nameof(conventionId), conventionId }, { nameof(userId), userId }, },
                    token);
            }
        }

        public async Task LeaveConventionAsync(string conventionId, string userId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.DeleteAsync(
                    connection,
                    TableName,
                    new Dictionary<string, object> { { nameof(conventionId), conventionId }, { nameof(userId), userId }, },
                    token);
            }
        }

        public async Task<bool> HasAlreadyJoinedAsync(string conventionId, string userId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var count = await ConnectionFactory.GetRecordCountAsync(
                                connection,
                                TableName,
                                new Dictionary<string, object> { { nameof(conventionId), conventionId }, { nameof(userId), userId }, },
                                token);

                return count > 0;
            }
        }

        public async Task<IEnumerable<Convention>> ListConventionsForUserAsync(string userId, int page, int pageSize, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                return await ConnectionFactory.ListAsync<Convention>(connection, $"view{TableName}", new Dictionary<string, object> { { nameof(userId), userId }, }, page, pageSize, token);
            }
        }
    }
}
