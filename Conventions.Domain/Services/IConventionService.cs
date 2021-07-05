namespace Conventions.Domain.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;

    public interface IConventionService
    {
        Task<Convention> CreateConventionAsync(ConventionData conventionData, CancellationToken token = default);

        Task<Convention> UpdateConventionAsync(string conventionId, ConventionData conventionData, CancellationToken token = default);

        Task DeleteConventionAsync(string conventionId, CancellationToken token = default);

        Task<IEnumerable<Convention>> ListConventionsAsync(int page, int pageSize, CancellationToken token = default);

        Task<Convention> GetConventionAsync(string conventionId, CancellationToken token = default);

        Task JoinConventionAsync(string conventionId, string userId, CancellationToken token = default);

        Task LeaveConventionAsync(string conventionId, string userId, CancellationToken token = default);

        Task<IEnumerable<Convention>> ListConventionsForUserAsync(string userId, int page, int pageSize, CancellationToken token = default);
    }
}
