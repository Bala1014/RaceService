using RaceService.Application.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes sessions within a weekend.</summary>
public interface ISessionService
{
    Task<Result<IReadOnlyList<SessionSummaryResponse>>> GetForEventAsync(Guid raceEventId, string? viewerTimeZone, CancellationToken cancellationToken = default);
    Task<Result<SessionDetailResponse>> GetByIdAsync(Guid id, string? viewerTimeZone, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionSummaryResponse>> GetLiveAndNextAsync(int count, string? viewerTimeZone, CancellationToken cancellationToken = default);
    Task<Result<SessionSummaryResponse>> CreateAsync(Guid raceEventId, CreateSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result<SessionSummaryResponse>> UpdateAsync(Guid id, UpdateSessionRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateStatusAsync(Guid id, UpdateSessionStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
