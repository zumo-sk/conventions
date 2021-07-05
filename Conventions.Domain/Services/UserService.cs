namespace Conventions.Domain.Services
{
    using Conventions.Domain.Models;
    using Conventions.Domain.DataAccess;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Exceptions;

    public class UserService : IUserService
    {
        private readonly IUserDataStore userDataStore;

        public UserService(IUserDataStore userDataStore)
        {
            this.userDataStore = userDataStore;
        }

        public async Task<User> CreateUserAsync(User user, CancellationToken token = default)
        {
            var existing = await userDataStore.GetUserAsync(user.Id, token);

            if (existing != null)
            {
                throw new IdentityAlreadyExistsException("User with this identity already exists.");
            }

            return await userDataStore.CreateUserAsync(user, token);
        }

        public Task<User> UpdateUserAsync(string userId, User user, CancellationToken token = default) => userDataStore.UpdateUserAsync(user, token);

        public Task DeleteUserAsync(string userId, CancellationToken token = default) => userDataStore.DeleteUserAsync(userId, token);

        public Task<User> GetUserAsync(string userId, CancellationToken token = default) => userDataStore.GetUserAsync(userId, token);

        public Task<IEnumerable<User>> ListUsersAsync(int page, int pageSize, CancellationToken token = default) => userDataStore.ListUsersAsync(page, pageSize, token);
    }
}
