namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Helpers;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;

    public class UserDataStore : IUserDataStore
    {
        private const string TableName = "users";

        private readonly IConnectionFactory connectionFactory;

        public UserDataStore(IConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.InsertAsync(connection, TableName, user.ToDictionary(true), token);

                return user;
            }
        }

        public async Task<User> UpdateUserAsync(User user, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.UpdateAsync(
                    connection,
                    TableName,
                    user.ToDictionary(false),
                    new KeyValuePair<string, object>(DataHelpers.Id, user.Id),
                    token);

                return user;
            }
        }

        public async Task DeleteUserAsync(string userId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                await ConnectionFactory.DeleteAsync(connection, TableName, new Dictionary<string, object> { { DataHelpers.Id, userId } }, token);
            }
        }

        public async Task<User> GetUserAsync(string userId, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                var result = await ConnectionFactory.ListAsync<User>(
                                 connection,
                                 TableName,
                                 new Dictionary<string, object> { { DataHelpers.Id, userId } },
                                 1,
                                 1,
                                 token);

                return result.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<User>> ListUsersAsync(int page, int pageSize, CancellationToken token = default)
        {
            using (var connection = await connectionFactory.CreateConnectionAsync(token))
            {
                return await ConnectionFactory.ListAsync<User>(connection, TableName, null, page, pageSize, token);
            }
        }
    }
}
