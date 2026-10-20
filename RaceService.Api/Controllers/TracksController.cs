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

/// <summary>Venues and their layouts.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tracks")]
public sealed class TracksController : ApiControllerBase
{
    private readonly ITrackService _tracks;

    /// <summary>Initialises the controller.</summary>
    public TracksController(ITrackService tracks) => _tracks = tracks;

    /// <summary>Lists venues.</summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(PagedResult<TrackSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] TrackQuery query, CancellationToken cancellationToken)
        => Ok(await _tracks.SearchAsync(query, cancellationToken));

    /// <summary>Returns one venue with its layouts and lap records.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetTrackById))]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _tracks.GetByIdAsync(id, cancellationToken));

    /// <summary>Returns one venue by slug.</summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrackBySlug(string slug, CancellationToken cancellationToken)
        => FromResult(await _tracks.GetBySlugAsync(slug, cancellationToken));

    /// <summary>Creates a venue, optionally with its first layout.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTrackRequest request, CancellationToken cancellationToken)
        => CreatedFromResult(
            await _tracks.CreateAsync(request, cancellationToken),
            nameof(GetTrackById),
            created => new { id = created.Id, version = "1.0" });

    /// <summary>Replaces a venue.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTrackRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _tracks.UpdateAsync(id, request, cancellationToken));

    /// <summary>Adds a layout to a venue, demoting the previous current one if needed.</summary>
    [HttpPost("{id:guid}/layouts")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(TrackLayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddLayout(
        Guid id,
        [FromBody] CreateTrackLayoutRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _tracks.AddLayoutAsync(id, request, cancellationToken));

    /// <summary>Archives a venue. Refused once it has hosted events.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => FromResult(await _tracks.ArchiveAsync(id, cancellationToken));
}
