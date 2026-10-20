namespace RaceService.Contracts.Common;

/// <summary>
/// One page of results plus the metadata a client needs to page through the rest.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>Items on this page.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>1-based page number.</summary>
    public int Page { get; init; }

    public int PageSize { get; init; }

    /// <summary>Total matching items across every page.</summary>
    public long TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;

    /// <summary>Creates a page.</summary>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
        => new() { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
}
