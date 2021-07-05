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
    using ApiConvention = Interaction.Convention;
    using DomainConvention = Domain.Models.Convention;

    [ApiController]
    public class ConventionController : ControllerBase
    {
        private readonly ILogger<ConventionController> logger;

        private readonly IConventionService conventionService;

        public ConventionController(ILogger<ConventionController> logger, IConventionService conventionService)
        {
            this.logger = logger;
            this.conventionService = conventionService;
        }

        [HttpPost("v1/conventions")]
        [Authorize(Policies.CreateConventions)]
        public async Task<ActionResult<ApiConvention>> CreateConventionAsync(
            [FromBody] UpsertConventionRequest upsertConventionRequest,
            CancellationToken token = default)
        {
            logger.LogTrace("Creating convention {@Convention}", upsertConventionRequest);
            var conventionData = MapRequestToConventionData(upsertConventionRequest);
            try
            {
                var convention = await conventionService.CreateConventionAsync(conventionData, token);

                return Created($"v1/conventions/{convention.Id}", convention);
            }
            catch (VenueNotFoundException e)
            {
                logger.LogWarning("Cannot create convention {@Convention}, venue with id {VenueId} not found.", upsertConventionRequest, upsertConventionRequest.VenueId);

                return UnprocessableEntity(e.Message);
            }
        }

        [HttpPut("v1/conventions/{conventionId}")]
        [Authorize(Policies.UpdateConventions)]
        public async Task<ActionResult<ApiConvention>> ChangeConventionAsync(
            [FromRoute] string conventionId,
            [FromBody] UpsertConventionRequest upsertConventionRequest,
            CancellationToken token = default)
        {
            logger.LogTrace("Updating Convention with id {ConventionId}", conventionId);
            var conventionData = MapRequestToConventionData(upsertConventionRequest);
            try
            {
                var convention = await conventionService.UpdateConventionAsync(conventionId, conventionData, token);

                return Ok(MapDomainConventionToApiConvention(convention));
            }
            catch (Exception e)
            {
                logger.LogWarning("Cannot update convention with id {ConventionId}, venue with id {VenueId} not found.", conventionId, upsertConventionRequest.VenueId);

                return UnprocessableEntity(e.Message);
            }
        }

        [HttpDelete("v1/conventions/{conventionId}")]
        [Authorize(Policies.DeleteConventions)]
        public async Task<ActionResult> DeleteConventionAsync([FromRoute] string conventionId, CancellationToken token = default)
        {
            logger.LogTrace("Dropping convention with id {ConventionId}", conventionId);
            await conventionService.DeleteConventionAsync(conventionId, token);

            return NoContent();
        }

        [HttpGet("v1/conventions")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApiConvention>>> ListConventionsAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var conventions = await conventionService.ListConventionsAsync(page, pageSize, token);

            return Ok(conventions.Select(MapDomainConventionToApiConvention));
        }

        [HttpPost("v1/conventions/{conventionId}/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiConvention>> JoinConventionAsync(
            [FromRoute] string conventionId,
            [FromRoute] string userId,
            CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.SignUpConventions) == false)
            {
                return Forbid();
            }

            logger.LogTrace("User {UserId} joining convention with id {ConventionId}", userId, conventionId);

            try
            {
                await conventionService.JoinConventionAsync(conventionId, userId, token);
            }
            catch (ConventionNotFoundException e)
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

            return Created($"v1/conventions/{conventionId}/users/{userId}", null);
        }

        [HttpDelete("v1/conventions/{conventionId}/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiConvention>> LeaveConventionAsync(
            [FromRoute] string conventionId,
            [FromRoute] string userId,
            CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.EjectConventions) == false)
            {
                return Forbid();
            }

            logger.LogTrace("User {UserId} leaving convention with id {ConventionId}", userId, conventionId);
            await conventionService.LeaveConventionAsync(conventionId, userId, token);

            return NoContent();
        }

        [HttpGet("v1/conventions/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApiConvention>>> ListConventionsForUserAsync(
            [FromRoute] string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.ReadUsers) == false)
            {
                return Forbid();
            }

            var conventions = await conventionService.ListConventionsForUserAsync(userId, page, pageSize, token);

            return Ok(conventions.Select(MapDomainConventionToApiConvention));
        }

        private static ConventionData MapRequestToConventionData(UpsertConventionRequest request)
        {
            var conventionData = new ConventionData();
            request.CopyTo(conventionData);

            return conventionData;
        }

        private static ApiConvention MapDomainConventionToApiConvention(DomainConvention domainConvention)
        {
            var apiConvention = new ApiConvention();
            domainConvention.CopyTo(apiConvention);

            return apiConvention;
        }
    }
}
