using RaceService.Application.Common;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes venues and their layouts.</summary>
public interface ITrackService
{
    Task<PagedResult<TrackSummaryResponse>> SearchAsync(TrackQuery query, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default);
    Task<Result<TrackDetailResponse>> UpdateAsync(Guid id, UpdateTrackRequest request, CancellationToken cancellationToken = default);
    Task<Result<TrackLayoutResponse>> AddLayoutAsync(Guid trackId, CreateTrackLayoutRequest request, CancellationToken cancellationToken = default);
    Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
