namespace Conventions.Domain.DataAccess
{
    using Conventions.Domain.Models;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITalkRegistrationDataStore
    {
        Task JoinTalkAsync(string talkId, string userId, CancellationToken token = default);

        Task LeaveTalkAsync(string talkId, string userId, CancellationToken token);

        Task<bool> HasAlreadyJoinedAsync(string talkId, string userId, CancellationToken token = default);

        Task<IEnumerable<Talk>> ListTalksForUserAsync(string userId, int page, int pageSize, CancellationToken token = default);
    }
}
