using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceService.Api.Authorization;
using RaceService.Application.Services;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Api.Controllers;

/// <summary>
/// Entries addressed directly by id. Listing and creation happen under the season they belong to
/// (<c>/seasons/{id}/entries</c>) or the round they contest (<c>/races/{id}/entries</c>), since an
/// entry has no meaning outside one of those.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/entries")]
public sealed class EntriesController : ApiControllerBase
{
    private readonly IEntryService _entries;

    /// <summary>Initialises the controller.</summary>
    public EntriesController(IEntryService entries) => _entries = entries;

    /// <summary>Replaces a season entry.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(EntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateEntryRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _entries.UpdateSeasonEntryAsync(id, request, cancellationToken));

    /// <summary>Changes a season entry's registration state.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateEntryStatusRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _entries.UpdateSeasonEntryStatusAsync(id, request, cancellationToken));

    /// <summary>Replaces the crew of a car at one round.</summary>
    [HttpPut("event-entries/{eventEntryId:guid}/lineup")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(EventEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateLineup(
        Guid eventEntryId,
        [FromBody] UpdateEventEntryLineupRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _entries.UpdateLineupAsync(eventEntryId, request, cancellationToken));

    /// <summary>Removes a car from a round's entry list. Refused once it has recorded results.</summary>
    [HttpDelete("event-entries/{eventEntryId:guid}")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveEventEntry(Guid eventEntryId, CancellationToken cancellationToken)
        => FromResult(await _entries.RemoveEventEntryAsync(eventEntryId, cancellationToken));
}
