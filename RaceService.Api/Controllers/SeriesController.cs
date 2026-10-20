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

/// <summary>Championships: Formula 1, WEC, MotoGP and the rest.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/series")]
public sealed class SeriesController : ApiControllerBase
{
    private readonly ISeriesService _series;
    private readonly ISeasonService _seasons;

    /// <summary>Initialises the controller.</summary>
    public SeriesController(ISeriesService series, ISeasonService seasons)
    {
        _series = series;
        _seasons = seasons;
    }

    /// <summary>Lists championships.</summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(PagedResult<SeriesSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SeriesQuery query, CancellationToken cancellationToken)
        => Ok(await _series.SearchAsync(query, cancellationToken));

    /// <summary>Returns one championship with its seasons and classes.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetSeriesById))]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(SeriesDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeriesById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _series.GetByIdAsync(id, cancellationToken));

    /// <summary>Returns one championship by slug.</summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(SeriesDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeriesBySlug(string slug, CancellationToken cancellationToken)
        => FromResult(await _series.GetBySlugAsync(slug, cancellationToken));

    /// <summary>Returns one season of a championship, with its calendar.</summary>
    [HttpGet("{slug}/seasons/{year:int}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(SeasonDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeason(
        string slug,
        int year,
        [FromQuery] string? timeZone,
        CancellationToken cancellationToken)
        => FromResult(await _seasons.GetBySlugAsync(slug, year, timeZone, cancellationToken));

    /// <summary>Creates a championship.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(typeof(SeriesDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSeriesRequest request, CancellationToken cancellationToken)
        => CreatedFromResult(
            await _series.CreateAsync(request, cancellationToken),
            nameof(GetSeriesById),
            created => new { id = created.Id, version = "1.0" });

    /// <summary>Replaces a championship.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(typeof(SeriesDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSeriesRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _series.UpdateAsync(id, request, cancellationToken));

    /// <summary>Archives a championship. Refused once it has seasons.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => FromResult(await _series.ArchiveAsync(id, cancellationToken));
}
