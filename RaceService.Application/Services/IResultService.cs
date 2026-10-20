using RaceService.Application.Common;
using RaceService.Contracts.Requests;
using RaceService.Contracts.Responses;

namespace RaceService.Application.Services;

/// <summary>Reads and writes session classifications and the detail recorded alongside them.</summary>
public interface IResultService
{
    Task<Result<IReadOnlyList<SessionResultResponse>>> GetResultsAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SessionResultResponse>>> ReplaceResultsAsync(Guid sessionId, ReplaceSessionResultsRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<SessionLapResponse>>> GetLapsAsync(Guid sessionId, Guid? eventEntryId, CancellationToken cancellationToken = default);

    Task<Result> ReplaceLapsAsync(Guid sessionId, ReplaceSessionLapsRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PitStopResponse>>> GetPitStopsAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<Result<PitStopResponse>> AddPitStopAsync(Guid sessionId, CreatePitStopRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PenaltyResponse>>> GetPenaltiesAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task<Result<PenaltyResponse>> AddPenaltyAsync(Guid sessionId, CreatePenaltyRequest request, CancellationToken cancellationToken = default);
}
