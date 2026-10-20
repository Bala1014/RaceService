using RaceService.Application.Common;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes competitors.</summary>
public interface IDriverService
{
    Task<PagedResult<DriverSummaryResponse>> SearchAsync(DriverQuery query, CancellationToken cancellationToken = default);
    Task<Result<DriverDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DriverDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SessionResultResponse>>> GetRecentResultsAsync(Guid driverId, int count, CancellationToken cancellationToken = default);
    Task<Result<DriverDetailResponse>> CreateAsync(CreateDriverRequest request, CancellationToken cancellationToken = default);
    Task<Result<DriverDetailResponse>> UpdateAsync(Guid id, UpdateDriverRequest request, CancellationToken cancellationToken = default);
    Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Rebuilds a driver's stored career totals from their results.</summary>
    Task<Result> RecalculateCareerStatsAsync(Guid driverId, CancellationToken cancellationToken = default);
}
