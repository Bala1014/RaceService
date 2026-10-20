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

/// <summary>Teams.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teams")]
public sealed class TeamsController : ApiControllerBase
{
    private readonly ITeamService _teams;

    /// <summary>Initialises the controller.</summary>
    public TeamsController(ITeamService teams) => _teams = teams;

    /// <summary>Lists teams.</summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(PagedResult<TeamSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] TeamQuery query, CancellationToken cancellationToken)
        => Ok(await _teams.SearchAsync(query, cancellationToken));

    /// <summary>Returns one team with its entries.</summary>
    [HttpGet("{id:guid}", Name = nameof(GetTeamById))]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(TeamDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamById(Guid id, CancellationToken cancellationToken)
        => FromResult(await _teams.GetByIdAsync(id, cancellationToken));

    /// <summary>Returns one team by slug.</summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "catalog")]
    [ProducesResponseType(typeof(TeamDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamBySlug(string slug, CancellationToken cancellationToken)
        => FromResult(await _teams.GetBySlugAsync(slug, cancellationToken));

    /// <summary>Creates a team.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(TeamDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
        => CreatedFromResult(
            await _teams.CreateAsync(request, cancellationToken),
            nameof(GetTeamById),
            created => new { id = created.Id, version = "1.0" });

    /// <summary>Replaces a team.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Editor)]
    [ProducesResponseType(typeof(TeamDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTeamRequest request,
        CancellationToken cancellationToken)
        => FromResult(await _teams.UpdateAsync(id, request, cancellationToken));

    /// <summary>Archives a team. Refused once it has season entries.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
        => FromResult(await _teams.ArchiveAsync(id, cancellationToken));
}
