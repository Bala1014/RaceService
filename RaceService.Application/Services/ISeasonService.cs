using RaceService.Application.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes seasons.</summary>
public interface ISeasonService
{
    Task<Result<SeasonDetailResponse>> GetByIdAsync(Guid id, string? viewerTimeZone, CancellationToken cancellationToken = default);
    Task<Result<SeasonDetailResponse>> GetBySlugAsync(string seriesSlug, int year, string? viewerTimeZone, CancellationToken cancellationToken = default);
    Task<Result<SeasonDetailResponse>> CreateAsync(CreateSeasonRequest request, CancellationToken cancellationToken = default);
    Task<Result<SeasonDetailResponse>> UpdateAsync(Guid id, UpdateSeasonRequest request, CancellationToken cancellationToken = default);
}
