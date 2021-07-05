namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;

    public interface IUserDataStore
    {
        Task<User> CreateUserAsync(User user, CancellationToken token = default);

        Task<User> UpdateUserAsync(User user, CancellationToken token = default);

        Task DeleteUserAsync(string userId, CancellationToken token = default);

        Task<IEnumerable<User>> ListUsersAsync(int page, int pageSize, CancellationToken token = default);

        Task<User> GetUserAsync(string userId, CancellationToken token = default);
    }
}
