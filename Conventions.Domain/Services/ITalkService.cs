namespace Conventions.Domain.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;

    public interface ITalkService
    {
        Task<Talk> CreateTalkAsync(TalkData talkData, CancellationToken token = default);

        Task<Talk> UpdateTalkAsync(string talkId, TalkData talkData, CancellationToken token = default);

        Task DeleteTalkAsync(string talkId, CancellationToken token = default);

        Task<Talk> GetTalkAsync(string talkId, CancellationToken token = default);

        Task<IEnumerable<Talk>> ListTalksAsync(int page, int pageSize, CancellationToken token = default);

        Task JoinTalkAsync(string talkId, string userId, CancellationToken token = default);

        Task LeaveTalkAsync(string talkId, string userId, CancellationToken token);

        Task<IEnumerable<Talk>> ListTalksForUserAsync(string userId, int page, int pageSize, CancellationToken token = default);
    }
}
