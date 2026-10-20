using System.ComponentModel.DataAnnotations;

namespace RaceService.Contracts.Common;

/// <summary>
/// Paging and sorting parameters shared by every list endpoint.
/// </summary>
/// <remarks>
/// <see cref="PageSize"/> is capped rather than merely defaulted. An uncapped page size is an
/// open invitation for a client to ask for every race ever held in one request, which is a
/// denial-of-service vector dressed up as a query parameter.
/// </remarks>
public record PagedRequest
{
    private const int MaxPageSize = 100;

    private int _pageSize = 20;
    private int _page = 1;

    /// <summary>1-based page number. Values below 1 are treated as 1.</summary>
    [Range(1, int.MaxValue)]
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    /// <summary>Items per page, clamped to 100.</summary>
    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value switch
        {
            < 1 => 20,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    /// <summary>Field to sort by. Interpretation is endpoint-specific.</summary>
    public string? SortBy { get; init; }

    /// <summary>True to sort descending.</summary>
    public bool SortDescending { get; init; }

    /// <summary>Number of rows to skip for the current page.</summary>
    public int Skip => (Page - 1) * PageSize;
}
