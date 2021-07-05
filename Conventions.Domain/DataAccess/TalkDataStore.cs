namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Helpers;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;

    public class TalkDataStore : ITalkDataStore
    {
        private const string TableName = "talks";

        private readonly IConnectionFactory connectionFactory;

        public TalkDataStore(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<Talk> CreateTalkAsync(TalkData talkData, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var id = await ConnectionFactory.InsertAsync(connection, TableName, talkData.ToDictionary(), token);

                var talk = new Talk { Id = id };
                talkData.CopyTo(talk);

                return talk;
            }
        }

        public async Task<Talk> UpdateTalkAsync(string talkId, TalkData talkData, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.UpdateAsync(
                    connection,
                    TableName,
                    talkData.ToDictionary(),
                    new KeyValuePair<string, object>(DataHelpers.Id, talkId),
                    token);

                var talk = new Talk { Id = talkId };
                talkData.CopyTo(talk);

                return talk;
            }
        }

        public async Task DeleteTalkAsync(string talkId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.DeleteAsync(connection, TableName, new Dictionary<string, object> { { DataHelpers.Id, talkId } }, token);
            }
        }

        public async Task<Talk> GetTalkAsync(string talkId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var result = await ConnectionFactory.ListAsync<Talk>(
                                 connection,
                                 TableName,
                                 new Dictionary<string, object> { { DataHelpers.Id, talkId } },
                                 1,
                                 1,
                                 token);

                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Talk>> ListTalksAsync(int page, int pageSize, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                return await ConnectionFactory.ListAsync<Talk>(connection, TableName, null, page, pageSize, token);
            }
        }
    }
}
