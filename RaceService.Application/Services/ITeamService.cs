using RaceService.Application.Common;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes teams.</summary>
public interface ITeamService
{
    Task<PagedResult<TeamSummaryResponse>> SearchAsync(TeamQuery query, CancellationToken cancellationToken = default);
    Task<Result<TeamDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TeamDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<TeamDetailResponse>> CreateAsync(CreateTeamRequest request, CancellationToken cancellationToken = default);
    Task<Result<TeamDetailResponse>> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default);
    Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
