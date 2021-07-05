namespace Conventions.Api.Controllers
{
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
    using ApiUser = Interaction.User;
    using DomainUser = Domain.Models.User;

    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;

        private readonly IUserService userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            this.logger = logger;
            this.userService = userService;
        }

        [HttpPost("v1/users")]
        [Authorize]
        public async Task<ActionResult<ApiUser>> RegisterUserAsync(
            [FromBody] UpsertUserRequest upsertUserRequest,
            CancellationToken token = default)
        {
            var identity = User?.Identity?.Name;
            if (identity == null)
            {
                return UnprocessableEntity("No user identity found in the token.");
            }

            logger.LogTrace("Registering user {@User}", upsertUserRequest);

            var userData = MapRequestToUserData(upsertUserRequest, identity);
            try
            {
                var user = await userService.CreateUserAsync(userData, token);

                return Created($"v1/users/{user.Id}", MapDomainUserToApiUser(user));
            }
            catch (IdentityAlreadyExistsException e)
            {
                logger.LogInformation("User with identity {Identity} tried to register another user.", identity);

                return UnprocessableEntity(e.Message);
            }
        }

        [HttpPut("v1/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiUser>> UpdateUserAsync(
            [FromRoute] string userId,
            [FromBody] UpsertUserRequest upsertUserRequest,
            CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.UpdateUsers) == false)
            {
                return Forbid();
            }

            logger.LogTrace("Updating user {UserId}", userId);
            var userData = MapRequestToUserData(upsertUserRequest, userId);
            var user = await userService.UpdateUserAsync(userId, userData, token);

            return Ok(MapDomainUserToApiUser(user));
        }

        [HttpDelete("v1/users/{userId}")]
        [Authorize]
        public async Task<ActionResult> DeleteUserAsync([FromRoute] string userId, CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.DeleteUsers) == false)
            {
                return Forbid();
            }

            logger.LogTrace("Dropping user with id {UserId}", userId);
            await userService.DeleteUserAsync(userId, token);

            return NoContent();
        }

        [HttpGet("v1/users/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApiUser>>> GetUserAsync([FromRoute] string userId, CancellationToken token = default)
        {
            if (User.CanAccess(userId, Permissions.ReadUsers) == false)
            {
                return Forbid();
            }

            var user = await userService.GetUserAsync(userId, token);
            if (user == null)
            {
                return NoContent();
            }

            return Ok(MapDomainUserToApiUser(user));
        }

        [HttpGet("v1/users")]
        [Authorize(Policies.ReadUsers)]
        public async Task<ActionResult<IEnumerable<ApiUser>>> ListUsersAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var users = await userService.ListUsersAsync(page, pageSize, token);

            return Ok(users.Select(MapDomainUserToApiUser));
        }

        private static DomainUser MapRequestToUserData(UpsertUserRequest request, string id)
        {
            var domainUser = new DomainUser { Id = id };
            request.CopyTo(domainUser);

            return domainUser;
        }

        private static ApiUser MapDomainUserToApiUser(DomainUser domainUser)
        {
            var apiUser = new ApiUser();
            domainUser.CopyTo(apiUser);

            return apiUser;
        }
    }
}
