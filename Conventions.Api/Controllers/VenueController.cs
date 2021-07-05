namespace Conventions.Api.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Conventions.Domain.Helpers;
    using Conventions.Domain.Models;
    using Conventions.Domain.Services;
    using Conventions.Interaction.Authorization;
    using Conventions.Interaction.Requests;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ApiVenue = Interaction.Venue;
    using DomainVenue = Domain.Models.Venue;

    [ApiController]
    public class VenueController : ControllerBase
    {
        private readonly ILogger<VenueController> logger;

        private readonly IVenueService venueService;

        public VenueController(ILogger<VenueController> logger, IVenueService venueService)
        {
            this.logger = logger;
            this.venueService = venueService;
        }

        [HttpPost("v1/venues")]
        [Authorize(Policies.CreateVenues)]
        public async Task<ActionResult<ApiVenue>> CreateVenueAsync(
            [FromBody] UpsertVenueRequest upsertVenueRequest,
            CancellationToken token = default)
        {
            logger.LogTrace("Creating venue {@Venue}", upsertVenueRequest);
            var venueData = MapRequestToVenueData(upsertVenueRequest);
            var venue = await venueService.CreateVenueAsync(venueData, token);

            return Created($"v1/venues/{venue.Id}", MapDomainVenueToApiVenue(venue));
        }

        [HttpPut("v1/venues/{venueId}")]
        [Authorize(Policies.UpdateVenues)]
        public async Task<ActionResult<ApiVenue>> UpdateVenueAsync(
            [FromRoute] string venueId,
            [FromBody] UpsertVenueRequest upsertVenueRequest,
            CancellationToken token = default)
        {
            logger.LogTrace("Updating venue {VenueId}", venueId);
            var venueData = MapRequestToVenueData(upsertVenueRequest);
            var venue = await venueService.UpdateVenueAsync(venueId, venueData, token);

            return Ok(MapDomainVenueToApiVenue(venue));
        }

        [HttpDelete("v1/venues/{venueId}")]
        [Authorize(Policies.DeleteVenues)]
        public async Task<ActionResult> DeleteVenueAsync([FromRoute] string venueId, CancellationToken token = default)
        {
            logger.LogTrace("Dropping venue with id {VenueId}", venueId);
            await venueService.DeleteVenueAsync(venueId, token);

            return NoContent();
        }

        [HttpGet("v1/venues/{venueId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApiVenue>>> GetVenueAsync([FromRoute] string venueId, CancellationToken token = default)
        {
            var venue = await venueService.GetVenueAsync(venueId, token);
            if (venue == null)
            {
                return NoContent();
            }

            return Ok(MapDomainVenueToApiVenue(venue));
        }

        [HttpGet("v1/venues")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ApiVenue>>> ListVenuesAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken token = default)
        {
            var venues = await venueService.ListVenuesAsync(page, pageSize, token);

            return Ok(venues.Select(MapDomainVenueToApiVenue));
        }

        private static VenueData MapRequestToVenueData(UpsertVenueRequest request)
        {
            var venueData = new VenueData();
            request.CopyTo(venueData);

            return venueData;
        }

        private static ApiVenue MapDomainVenueToApiVenue(DomainVenue domainVenue)
        {
            var apiVenue = new ApiVenue();
            domainVenue.CopyTo(apiVenue);

            return apiVenue;
        }
    }
}
