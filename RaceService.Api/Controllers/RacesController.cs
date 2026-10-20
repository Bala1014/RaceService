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

/// <summary>
/// Race weekends: the calendar, individual event pages, and their schedules.
/// </summary>
/// <remarks>
/// A "race" in the URL is a race weekend — the thing a fan navigates to. The individual pieces of
/// running inside it are sessions, addressed under <c>/sessions</c>.
/// </remarks>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/races")]
public sealed class RacesController : ApiControllerBase
{
    private readonly IRaceEventService _events;
    private readonly ISessionService _sessions;
    private readonly IEntryService _entries;

    /// <summary>Initialises the controller.</summary>
    public RacesController(IRaceEventService events, ISessionService sessions, IEntryService entries)
    {
        _events = events;
        _sessions = sessions;
        _entries = entries;
    }

    /// <summary>Lists race weekends with filtering, paging and optional local times.</summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "races")]
    [ProducesResponseType(typeof(PagedResult<RaceEventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] RaceEventQuery query, CancellationToken cancellationToken)
        => Ok(await _events.SearchAsync(query, IsAdministrator, cancellationToken));

    /// <summary>Returns the next races due to run, for a home page strip.</summary>
    [HttpGet("upcoming")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "races")]
    [ProducesResponseType(typeof(IReadOnlyList<RaceEventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upcoming(
        [FromQuery] int count = 5,
        [FromQuery] Guid? seriesId = null,
        [FromQuery] string? timeZone = null,
        CancellationToken cancellationToken = default)
        => Ok(await _events.GetUpcomingAsync(count, seriesId, timeZone, cancellationToken));

    /// <summary>Returns every race in a date window, for a calendar grid.</summary>
    [HttpGet("calendar")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "races")]
    [ProducesResponseType(typeof(IReadOnlyList<RaceEventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Calendar([FromQuery] CalendarQuery query, CancellationToken cancellationToken)
        => Ok(await _events.GetCalendarAsync(query, cancellationToken));

    /// <summary>Returns one race weekend by id.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetRaceById))]
    [AllowAnonymous]
    [OutputCache(PolicyName = "races")]
    [ProducesResponseType(typeof(RaceEventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRaceById(
        Guid id,
        [FromQuery] string? timeZone,
        CancellationToken cancellationToken)
        => FromResult(await _events.GetByIdAsync(id, timeZone, IsAdministrator, cancellationToken));

    /// <summary>
    /// Returns one race weekend by its human-readable path, e.g.
    /// <c>/races/formula-1/2026/monaco-grand-prix</c>.
    /// </summary>
    [HttpGet("{seriesSlug}/{year:int}/{eventSlug}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "races")]
    [ProducesResponseType(typeof(RaceEventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRaceBySlug(
        string seriesSlug,
        int year,
        string eventSlug,
        [FromQuery] string? timeZone,
        CancellationToken cancellationToken)
        => FromResult(await _events.GetBySlugAsync(seriesSlug, year, eventSlug, timeZone, IsAdministrator, cancellationToken));

    /// <summary>Lists a weekend's sessions in running order.</summary>
    [HttpGet("{id:guid}/sessions")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "races")]
    [ProducesResponseType(typeof(IReadOnlyList<SessionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessions(
        Guid id,
        [FromQuery] string? timeZone,
        CancellationToken cancellationToken)
        => FromResult(await _sessions.GetForEventAsync(id, timeZone, cancellationToken));

    /// <summary>Lists the cars entered for one round, with their crews.</summary>
    [HttpGet("{id:guid}/entries")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "races")]
    [ProducesResponseType(typeof(IReadOnlyList<EventEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntries(Guid id, CancellationToken cancellationToken)
        => FromResult(await _entries.GetEventEntriesAsync(id, cancellationToken));

    /// <summary>Creates a race weekend, optionally with its whole schedule.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(RaceEventDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateRaceEventRequest request,
        CancellationToken cancellationToken)
        => CreatedFromResult(
            await _events.CreateAsync(request, cancellationToken),
            nameof(GetRaceById),
            created => new { id = created.Id, version = "1.0" });

    /// <summary>Replaces a race weekend's metadata.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(RaceEventDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateRaceEventRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _events.UpdateAsync(id, request, cancellationToken));

    /// <summary>Moves a race weekend to a new lifecycle state.</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateEventStatusRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _events.UpdateStatusAsync(id, request, cancellationToken));

    /// <summary>Adds a session to a weekend.</summary>
    [HttpPost("{id:guid}/sessions")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(SessionSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddSession(
        Guid id,
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _sessions.CreateAsync(id, request, cancellationToken));

    /// <summary>Adds a car to a round's entry list.</summary>
    [HttpPost("{id:guid}/entries")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(EventEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddEntry(
        Guid id,
        [FromBody] CreateEventEntryRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _entries.AddEventEntryAsync(id, request, cancellationToken));

    /// <summary>
    /// Archives a race weekend. This is a soft delete: the event and its results are retained and
    /// simply stop appearing in queries.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => FromResult(await _events.ArchiveAsync(id, cancellationToken));
}
