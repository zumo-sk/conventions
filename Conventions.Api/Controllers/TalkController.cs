namespace Conventions.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Exceptions;
    using Conventions.Domain.Helpers;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;
    using Conventions.Interaction.Authorization;
    using Conventions.Interaction.Requests;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ApiTalk = Interaction.Talk;
    using DomainTalk = Domain.Models.Talk;

    [ApiController]
    public class TalkController : ControllerBase
    {
        private readonly ILogger<TalkController> logger;

        private readonly ITalkService talkService;

        public TalkController(ILogger<TalkController> logger, ITalkService talkService)
        {
            this.logger = logger;
            this.talkService = talkService;
        }

        [HttpPost("v1/talks")]
        [Authorize(Policies.CreateTalks)]
        public async Task<ActionResult<ApiTalk>> CreateTalkAsync(
            [FromBody] UpsertTalkRequest upsertTalkRequest,
            CancellationToken token = default)
        {
            // not a mistake - if user is missing permission `...OnBehalf`, they have to be the speakerId
            // the policy check in decorator is however still necessary
            if (User.CanAccess(upsertTalkRequest.SpeakerId, Permissions.CreateTalksOnBehalf) == false)
            {
                return Forbid();
            }

            logger.LogTrace("Creating talk {@Talk}", upsertTalkRequest);
            var talkData = MapRequestToTalkData(upsertTalkRequest);
            try
            {
                var talk = await talkService.CreateTalkAsync(talkData, token);

                return Created($"v1/talks/{talk.Id}", MapDomainTalkToApiTalk(talk));
            }
            catch (ConventionNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UserNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (NotPartOfConventionException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }

        [HttpPut("v1/talks/{talkId}")]
        [Authorize]
        public async Task<ActionResult<ApiTalk>> UpdateTalkAsync(
            [FromRoute] string talkId,
            [FromBody] UpsertTalkRequest upsertTalkRequest,
            CancellationToken token = default)
        {
            if (User.CanAccess(upsertTalkRequest.SpeakerId, Permissions.UpdateTalks) == false)
            {
                return Forbid();
            }

            logger.LogTrace("Updating Talk {@Talk}", upsertTalkRequest);
            var talkData = MapRequestToTalkData(upsertTalkRequest);
            try
            {
                var talk = await talkService.UpdateTalkAsync(talkId, talkData, token);

                return Ok(MapDomainTalkToApiTalk(talk));
            }
            catch (ConventionNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UserNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (NotPartOfConventionException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (AlreadyJoinedException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }

        [HttpDelete("v1/talks/{talkId}")]
        [Authorize]
        public async Task<ActionResult> DeleteTalkAsync([FromRoute] string talkId, CancellationToken token = default)
        {
            var talk = await talkService.GetTalkAsync(talkId, token);
            if (User.CanAccess(talk.SpeakerId, Permissions.DeleteTalks) == false)
            {
                return Forbid();
            }

            logger.LogTrace("Dropping talk with id {TalkId}", talkId);
            await talkService.DeleteTalkAsync(talkId, token);

            return NoContent();
        }

        [HttpGet("v1/talks")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApiTalk>>> ListTalksAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var talks = await talkService.ListTalksAsync(page, pageSize, token);

            return Ok(talks.Select(MapDomainTalkToApiTalk));
        }

        [HttpPost("v1/talks/{talkId}/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiTalk>> JoinTalkAsync(
            [FromRoute] string talkId,
            [FromRoute] string userId,
            CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.SignUpTalks) == false)
            {
                return Forbid();
            }

            logger.LogTrace("User {UserId} joining talk with id {TalkId}", userId, talkId);

            try
            {
                await talkService.JoinTalkAsync(talkId, userId, token);
            }
            catch (TalkNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (UserNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (AlreadyJoinedException e)
            {
                return UnprocessableEntity(e.Message);
            }
            catch (NotPartOfConventionException e)
            {
                return UnprocessableEntity(e.Message);
            }

            return Ok();
        }

        [HttpDelete("v1/talks/{talkId}/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiTalk>> LeaveConventionAsync(
            [FromRoute] string talkId,
            [FromRoute] string userId,
            CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.EjectTalks) == false)
            {
                return Forbid();
            }

            logger.LogTrace("User {UserId} leaving talk with id {TalkId}", userId, talkId);
            await talkService.LeaveTalkAsync(talkId, userId, token);

            return NoContent();
        }

        [HttpGet("v1/talks/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApiTalk>>> ListTalksForUserAsync(
            [FromRoute] string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.ReadUsers) == false)
            {
                return Forbid();
            }

            var talks = await talkService.ListTalksForUserAsync(userId, page, pageSize, token);

            return Ok(talks.Select(MapDomainTalkToApiTalk));
        }

        private static TalkData MapRequestToTalkData(UpsertTalkRequest request)
        {
            var talkData = new TalkData();
            request.CopyTo(talkData);

            return talkData;
        }

        private static ApiTalk MapDomainTalkToApiTalk(DomainTalk domainTalk)
        {
            var apiTalk = new ApiTalk();
            domainTalk.CopyTo(apiTalk);

            return apiTalk;
        }
    }
}
