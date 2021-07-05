namespace Conventions.Domain.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.DataAccess;
    using Conventions.Domain.Exceptions;
    using Conventions.Domain.Models;

    public class TalkService : ITalkService
    {
        private readonly ITalkDataStore talkDataStore;
        private readonly ITalkRegistrationDataStore talkRegistrationDataStore;
        private readonly IConventionService conventionService;
        private readonly IUserService userService;
        private readonly IConventionRegistrationDataStore conventionRegistrationDataStore;

        public TalkService(
            ITalkDataStore talkDataStore,
            IConventionService conventionService,
            ITalkRegistrationDataStore talkRegistrationDataStore,
            IUserService userService,
            IConventionRegistrationDataStore conventionRegistrationDataStore)
        {
            this.talkDataStore = talkDataStore;
            this.conventionService = conventionService;
            this.talkRegistrationDataStore = talkRegistrationDataStore;
            this.userService = userService;
            this.conventionRegistrationDataStore = conventionRegistrationDataStore;
        }

        public async Task<Talk> CreateTalkAsync(TalkData talkData, CancellationToken token = default)
        {
            var conventionTask = conventionService.GetConventionAsync(talkData.ConventionId, token);
            var userTask = userService.GetUserAsync(talkData.SpeakerId, token);
            var partOfConventionTask = conventionRegistrationDataStore.HasAlreadyJoinedAsync(talkData.ConventionId, talkData.SpeakerId, token);
            if (await conventionTask == null)
            {
                throw new ConventionNotFoundException($"Convention `{talkData.ConventionId}` not found.");
            }

            if (await userTask == null)
            {
                throw new UserNotFoundException($"User `{talkData.SpeakerId}` not found.");
            }

            if (await partOfConventionTask == false)
            {
                throw new NotPartOfConventionException($"User `{talkData.SpeakerId}` has to join convention `{talkData.ConventionId}` to be able to speak there.");
            }

            return await talkDataStore.CreateTalkAsync(talkData, token);
        }

        public async Task<Talk> UpdateTalkAsync(string talkId, TalkData talkData, CancellationToken token = default)
        {
            var conventionTask = conventionService.GetConventionAsync(talkData.ConventionId, token);
            var userTask = userService.GetUserAsync(talkData.SpeakerId, token);
            var partOfConventionTask = conventionRegistrationDataStore.HasAlreadyJoinedAsync(talkData.ConventionId, talkData.SpeakerId, token);
            var speakerIsGuestOnThisTalkTask = talkRegistrationDataStore.HasAlreadyJoinedAsync(talkId, talkData.SpeakerId, token);
            if (await conventionTask == null)
            {
                throw new ConventionNotFoundException($"Convention `{talkData.ConventionId}` not found.");
            }

            if (await userTask == null)
            {
                throw new UserNotFoundException($"User `{talkData.SpeakerId}` not found.");
            }

            if (await partOfConventionTask == false)
            {
                throw new NotPartOfConventionException($"User `{talkData.SpeakerId}` has to join convention `{talkData.ConventionId}` to be able to speak there.");
            }

            if (await speakerIsGuestOnThisTalkTask)
            {
                throw new AlreadyJoinedException($"User `{talkData.SpeakerId}` cannot be a guest on talk `{talkId}`, they are speaking there.");
            }

            return await talkDataStore.UpdateTalkAsync(talkId, talkData, token);
        }

        public Task DeleteTalkAsync(string talkId, CancellationToken token = default) =>
            talkDataStore.DeleteTalkAsync(talkId, token);

        public Task<Talk> GetTalkAsync(string talkId, CancellationToken token = default) =>
            talkDataStore.GetTalkAsync(talkId, token);

        public Task<IEnumerable<Talk>> ListTalksAsync(int page, int pageSize, CancellationToken token = default) =>
            talkDataStore.ListTalksAsync(page, pageSize, token);

        public async Task JoinTalkAsync(string talkId, string userId, CancellationToken token = default)
        {
            var talkTask = talkDataStore.GetTalkAsync(talkId, token);
            var userTask = userService.GetUserAsync(userId, token);
            var alreadyJoinedTask = talkRegistrationDataStore.HasAlreadyJoinedAsync(talkId, userId, token);

            var talk = await talkTask;
            if (talk == null)
            {
                throw new TalkNotFoundException($"Talk `{talkId}` not found.");
            }

            var partOfConventionTask = conventionRegistrationDataStore.HasAlreadyJoinedAsync(talk.ConventionId, userId, token);

            if (talk.SpeakerId == userId)
            {
                throw new AlreadyJoinedException($"User `{userId}` is a speaker on talk `{talkId}`.");
            }

            if (await userTask == null)
            {
                throw new UserNotFoundException($"User `{userId}` not found.");
            }

            if (await alreadyJoinedTask)
            {
                throw new AlreadyJoinedException($"User `{userId}` already joined talk `{talkId}`.");
            }

            if (await partOfConventionTask == false)
            {
                throw new NotPartOfConventionException($"User `{userId}` has to join convention `{talk.ConventionId}` to be able to join talks there.");
            }

            await talkRegistrationDataStore.JoinTalkAsync(talkId, userId, token);
        }

        public async Task LeaveTalkAsync(string talkId, string userId, CancellationToken token) =>
            await talkRegistrationDataStore.LeaveTalkAsync(talkId, userId, token);

        public async Task<IEnumerable<Talk>> ListTalksForUserAsync(string userId, int page, int pageSize, CancellationToken token = default) =>
            await talkRegistrationDataStore.ListTalksForUserAsync(userId, page, pageSize, token);
    }
}
