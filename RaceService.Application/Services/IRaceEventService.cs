using Microsoft.EntityFrameworkCore;
using RaceService.Application.Common;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities.Events;
using RaceService.Application.Mapping;
using RaceService.Contracts.Common;
using RaceService.Contracts.Enums;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes race weekends.</summary>
public interface IRaceEventService
{
    Task<PagedResult<RaceEventSummaryResponse>> SearchAsync(RaceEventQuery query, bool isAdmin, CancellationToken cancellationToken = default);

    Task<Result<RaceEventDetailResponse>> GetByIdAsync(Guid id, string? viewerTimeZone, bool isAdmin, CancellationToken cancellationToken = default);

    Task<Result<RaceEventDetailResponse>> GetBySlugAsync(string seriesSlug, int year, string eventSlug, string? viewerTimeZone, bool isAdmin, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RaceEventSummaryResponse>> GetUpcomingAsync(int count, Guid? seriesId, string? viewerTimeZone, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RaceEventSummaryResponse>> GetCalendarAsync(CalendarQuery query, CancellationToken cancellationToken = default);

    Task<Result<RaceEventDetailResponse>> CreateAsync(CreateRaceEventRequest request, CancellationToken cancellationToken = default);

    Task<Result<RaceEventDetailResponse>> UpdateAsync(Guid id, UpdateRaceEventRequest request, CancellationToken cancellationToken = default);

    Task<Result> UpdateStatusAsync(Guid id, UpdateEventStatusRequest request, CancellationToken cancellationToken = default);

    Task<Result> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
}
