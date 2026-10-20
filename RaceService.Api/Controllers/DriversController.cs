using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using RaceService.Api.Authorization;
using RaceService.Application.Services;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Api.Controllers;

/// <summary>Competitors.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/drivers")]
public sealed class DriversController : ApiControllerBase
{
    private readonly IDriverService _drivers;

    /// <summary>Initialises the controller.</summary>
    public DriversController(IDriverService drivers) => _drivers = drivers;

    /// <summary>Lists competitors.</summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(PagedResult<DriverSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] DriverQuery query, CancellationToken cancellationToken)
        => Ok(await _drivers.SearchAsync(query, cancellationToken));

    /// <summary>Returns one competitor with career totals.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetDriverById))]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(DriverDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDriverById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _drivers.GetByIdAsync(id, cancellationToken));

    /// <summary>Returns one competitor by slug.</summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(DriverDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDriverBySlug(string slug, CancellationToken cancellationToken)
        => FromResult(await _drivers.GetBySlugAsync(slug, cancellationToken));

    /// <summary>Returns a competitor's most recent race results.</summary>
    [HttpGet("{id:guid}/results")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResults(
        Guid id,
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
        => FromResult(await _drivers.GetRecentResultsAsync(id, count, cancellationToken));

    /// <summary>Creates a competitor.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(DriverDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateDriverRequest request, CancellationToken cancellationToken)
        => CreatedFromResult(
            await _drivers.CreateAsync(request, cancellationToken),
            nameof(GetDriverById),
            created => new { id = created.Id, version = "1.0" });

    /// <summary>Replaces a competitor.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(DriverDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDriverRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _drivers.UpdateAsync(id, request, cancellationToken));

    /// <summary>Rebuilds a competitor's stored career totals from their results.</summary>
    [HttpPost("{id:guid}/career-stats/recalculate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecalculateCareerStats(Guid id, CancellationToken cancellationToken)
        => FromResult(await _drivers.RecalculateCareerStatsAsync(id, cancellationToken));

    /// <summary>Archives a competitor. Refused once they have recorded results.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => FromResult(await _drivers.ArchiveAsync(id, cancellationToken));
}
