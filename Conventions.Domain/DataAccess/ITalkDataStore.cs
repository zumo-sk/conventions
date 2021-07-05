namespace Conventions.Domain.DataAccess
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Models;

    public interface ITalkDataStore
    {
        Task<Talk> CreateTalkAsync(TalkData talkData, CancellationToken token = default);

        Task<Talk> UpdateTalkAsync(string talkId, TalkData talkData, CancellationToken token = default);

        Task DeleteTalkAsync(string talkId, CancellationToken token = default);

        Task<Talk> GetTalkAsync(string talkId, CancellationToken token = default);

        Task<IEnumerable<Talk>> ListTalksAsync(int page, int pageSize, CancellationToken token = default);
    }
}
