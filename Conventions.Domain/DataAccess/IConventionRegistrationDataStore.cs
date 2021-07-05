namespace Conventions.Domain.DataAccess
{
    using Conventions.Domain.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IConventionRegistrationDataStore
    {
        Task JoinConventionAsync(string conventionId, string userId, CancellationToken token = default);

        Task LeaveConventionAsync(string conventionId, string userId, CancellationToken token = default);

        Task<bool> HasAlreadyJoinedAsync(string conventionId, string userId, CancellationToken token = default);

        Task<IEnumerable<Convention>> ListConventionsForUserAsync(string userId, int page, int pageSize, CancellationToken token = default);
    }
}
