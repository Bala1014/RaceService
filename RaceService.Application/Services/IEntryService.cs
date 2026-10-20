using RaceService.Application.Common;
using RaceService.Contracts.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes season entries and per-round entry lists.</summary>
public interface IEntryService
{
    Task<Result<PagedResult<EntryResponse>>> GetSeasonEntriesAsync(Guid seasonId, EntryQuery query, CancellationToken cancellationToken = default);
    Task<Result<EntryResponse>> CreateSeasonEntryAsync(Guid seasonId, CreateEntryRequest request, Guid? submittedByUserId, CancellationToken cancellationToken = default);
    Task<Result<EntryResponse>> UpdateSeasonEntryAsync(Guid entryId, UpdateEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateSeasonEntryStatusAsync(Guid entryId, UpdateEntryStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<EventEntryResponse>>> GetEventEntriesAsync(Guid raceEventId, CancellationToken cancellationToken = default);
    Task<Result<EventEntryResponse>> AddEventEntryAsync(Guid raceEventId, CreateEventEntryRequest request, CancellationToken cancellationToken = default);
    Task<Result<EventEntryResponse>> UpdateLineupAsync(Guid eventEntryId, UpdateEventEntryLineupRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveEventEntryAsync(Guid eventEntryId, CancellationToken cancellationToken = default);
}
