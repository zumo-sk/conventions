namespace Conventions.Domain.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;

    public interface IUserService
    {
        Task<User> CreateUserAsync(User user, CancellationToken token = default);

        Task<User> UpdateUserAsync(string userId, User user, CancellationToken token = default);

        Task DeleteUserAsync(string userId, CancellationToken token = default);

        Task<User> GetUserAsync(string userId, CancellationToken token = default);

        Task<IEnumerable<User>> ListUsersAsync(int page, int pageSize, CancellationToken token = default);
    }
}
