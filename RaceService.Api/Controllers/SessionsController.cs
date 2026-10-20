using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using RaceService.Api.Authorization;
using RaceService.Application.Services;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Api.Controllers;

/// <summary>
/// Sessions and everything recorded about them after the flag: classification, laps, pit stops
/// and penalties.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sessions")]
public sealed class SessionsController : ApiControllerBase
{
    private readonly ISessionService _sessions;
    private readonly IResultService _results;

    /// <summary>Initialises the controller.</summary>
    public SessionsController(ISessionService sessions, IResultService results)
    {
        _sessions = sessions;
        _results = results;
    }

    /// <summary>Returns sessions running now, followed by those due next.</summary>
    [HttpGet("live")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "live")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Live(
        [FromQuery] int count = 10,
        [FromQuery] string? timeZone = null,
        CancellationToken cancellationToken = default)
        => Ok(await _sessions.GetLiveAndNextAsync(count, timeZone, cancellationToken));

    /// <summary>Returns one session with its classification.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetSessionById))]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(SessionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionById(
        Guid id,
        [FromQuery] string? timeZone,
        CancellationToken cancellationToken)
        => FromResult(await _sessions.GetByIdAsync(id, timeZone, cancellationToken));

    /// <summary>Returns a session's classification.</summary>
    [HttpGet("{id:guid}/results")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResults(Guid id, CancellationToken cancellationToken)
        => FromResult(await _results.GetResultsAsync(id, cancellationToken));

    /// <summary>Returns a session's lap-by-lap timing, optionally for one car.</summary>
    [HttpGet("{id:guid}/laps")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionLapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLaps(
        Guid id,
        [FromQuery] Guid? eventEntryId,
        CancellationToken cancellationToken)
        => FromResult(await _results.GetLapsAsync(id, eventEntryId, cancellationToken));

    /// <summary>Returns a session's pit stops.</summary>
    [HttpGet("{id:guid}/pit-stops")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(IReadOnlyList<PitStopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPitStops(Guid id, CancellationToken cancellationToken)
        => FromResult(await _results.GetPitStopsAsync(id, cancellationToken));

    /// <summary>Returns the stewards' decisions from a session.</summary>
    [HttpGet("{id:guid}/penalties")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(IReadOnlyList<PenaltyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPenalties(Guid id, CancellationToken cancellationToken)
        => FromResult(await _results.GetPenaltiesAsync(id, cancellationToken));

    /// <summary>Updates a session's schedule.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(SessionSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSessionRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _sessions.UpdateAsync(id, request, cancellationToken));

    /// <summary>Records what actually happened to a session: when it ran, the weather, red flags.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateSessionStatusRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _sessions.UpdateStatusAsync(id, request, cancellationToken));

    /// <summary>
    /// Replaces a session's entire classification.
    /// </summary>
    /// <remarks>
    /// A whole-set replacement, not a row append. A results table is only valid as a set —
    /// positions must be contiguous, one car holds the fastest lap, points depend on every other
    /// row — so it is validated and swapped atomically.
    /// </remarks>
    [HttpPut("{id:guid}/results")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(IReadOnlyList<SessionResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReplaceResults(
        Guid id,
        [FromBody] ReplaceSessionResultsRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _results.ReplaceResultsAsync(id, request, cancellationToken));

    /// <summary>Uploads a session's lap-by-lap timing sheet, replacing anything already stored.</summary>
    [HttpPut("{id:guid}/laps")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReplaceLaps(
        Guid id,
        [FromBody] ReplaceSessionLapsRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _results.ReplaceLapsAsync(id, request, cancellationToken));

    /// <summary>Records a pit stop.</summary>
    [HttpPost("{id:guid}/pit-stops")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(PitStopResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddPitStop(
        Guid id,
        [FromBody] CreatePitStopRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _results.AddPitStopAsync(id, request, cancellationToken));

    /// <summary>Records a stewards' decision.</summary>
    [HttpPost("{id:guid}/penalties")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(PenaltyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPenalty(
        Guid id,
        [FromBody] CreatePenaltyRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _results.AddPenaltyAsync(id, request, cancellationToken));

    /// <summary>Deletes a session. Refused once it has recorded results.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => FromResult(await _sessions.DeleteAsync(id, cancellationToken));
}
