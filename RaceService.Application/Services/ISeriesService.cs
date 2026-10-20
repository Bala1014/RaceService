using RaceService.Application.Common;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes championships.</summary>
public interface ISeriesService
{
    Task<PagedResult<SeriesSummaryResponse>> SearchAsync(SeriesQuery query, CancellationToken cancellationToken = default);
    Task<Result<SeriesDetailResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<SeriesDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Result<SeriesDetailResponse>> CreateAsync(CreateSeriesRequest request, CancellationToken cancellationToken = default);
    Task<Result<SeriesDetailResponse>> UpdateAsync(Guid id, UpdateSeriesRequest request, CancellationToken cancellationToken = default);
    Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
