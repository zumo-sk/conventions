namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;

    public class TalkRegistrationDataStore : ITalkRegistrationDataStore
    {
        private const string TableName = "talkRegistrations";

        private readonly IConnectionFactory connectionFactory;

        public TalkRegistrationDataStore(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task JoinTalkAsync(string talkId, string userId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.InsertAsync(
                    connection,
                    TableName,
                    new Dictionary<string, object> { { nameof(talkId), talkId }, { nameof(userId), userId }, },
                    token);
            }
        }

        public async Task LeaveTalkAsync(string talkId, string userId, CancellationToken token)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.DeleteAsync(
                    connection,
                    TableName,
                    new Dictionary<string, object> { { nameof(talkId), talkId }, { nameof(userId), userId }, },
                    token);
            }
        }

        public async Task<bool> HasAlreadyJoinedAsync(string talkId, string userId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var count = await ConnectionFactory.GetRecordCountAsync(
                                connection,
                                TableName,
                                new Dictionary<string, object> { { nameof(talkId), talkId }, { nameof(userId), userId }, },
                                token);

                return count > 0;
            }
        }

        public async Task<IEnumerable<Talk>> ListTalksForUserAsync(string userId, int page, int pageSize, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                return await ConnectionFactory.ListAsync<Talk>(connection, $"view{TableName}", new Dictionary<string, object> { { nameof(userId), userId }, }, page, pageSize, token);
            }
        }
    }
}
