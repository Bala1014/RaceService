using RaceService.Application.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and rebuilds championship tables.</summary>
public interface IStandingsService
{
    Task<Result<IReadOnlyList<DriverStandingResponse>>> GetDriverStandingsAsync(Guid seasonId, StandingsQuery query, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TeamStandingResponse>>> GetTeamStandingsAsync(Guid seasonId, StandingsQuery query, CancellationToken cancellationToken = default);

    /// <summary>Rebuilds both tables for a season from its official results.</summary>
    Task<Result> RecalculateSeasonAsync(Guid seasonId, CancellationToken cancellationToken = default);
}
