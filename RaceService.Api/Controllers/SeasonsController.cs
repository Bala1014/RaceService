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

/// <summary>Seasons: a year of a championship, its entry list and its standings.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seasons")]
public sealed class SeasonsController : ApiControllerBase
{
    private readonly ISeasonService _seasons;
    private readonly IEntryService _entries;
    private readonly IStandingsService _standings;

    /// <summary>Initialises the controller.</summary>
    public SeasonsController(ISeasonService seasons, IEntryService entries, IStandingsService standings)
    {
        _seasons = seasons;
        _entries = entries;
        _standings = standings;
    }

    /// <summary>Returns one season with its calendar.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetSeasonById))]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(SeasonDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeasonById(
        Guid id,
        [FromQuery] string? timeZone,
        CancellationToken cancellationToken)
        => FromResult(await _seasons.GetByIdAsync(id, timeZone, cancellationToken));

    /// <summary>Lists the cars registered for a season.</summary>
    [HttpGet("{id:guid}/entries")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(PagedResult<EntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntries(
        Guid id,
        [FromQuery] EntryQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _entries.GetSeasonEntriesAsync(id, query, cancellationToken));

    /// <summary>Returns the drivers' championship table.</summary>
    [HttpGet("{id:guid}/standings/drivers")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(IReadOnlyList<DriverStandingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDriverStandings(
        Guid id,
        [FromQuery] StandingsQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _standings.GetDriverStandingsAsync(id, query, cancellationToken));

    /// <summary>Returns the teams' championship table.</summary>
    [HttpGet("{id:guid}/standings/teams")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "results")]
    [ProducesResponseType(typeof(IReadOnlyList<TeamStandingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamStandings(
        Guid id,
        [FromQuery] StandingsQuery query,
        CancellationToken cancellationToken)
        => FromResult(await _standings.GetTeamStandingsAsync(id, query, cancellationToken));

    /// <summary>Creates a season.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(typeof(SeasonDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSeasonRequest request, CancellationToken cancellationToken)
        => CreatedFromResult(
            await _seasons.CreateAsync(request, cancellationToken),
            nameof(GetSeasonById),
            created => new { id = created.Id, version = "1.0" });

    /// <summary>Replaces a season.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(typeof(SeasonDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSeasonRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _seasons.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Registers a car for a season. Open to any authenticated entrant where the platform allows
    /// self-service entries; the submitting user is recorded on the entry.
    /// </summary>
    [HttpPost("{id:guid}/entries")]
    [Authorize(Policy = Policies.Entrant)]
    [ProducesResponseType(typeof(EntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEntry(
        Guid id,
        [FromBody] CreateEntryRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _entries.CreateSeasonEntryAsync(id, request, CurrentUserId, cancellationToken));

    /// <summary>Rebuilds a season's championship tables from its official results.</summary>
    [HttpPost("{id:guid}/standings/recalculate")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecalculateStandings(Guid id, CancellationToken cancellationToken)
        => FromResult(await _standings.RecalculateSeasonAsync(id, cancellationToken));
}
