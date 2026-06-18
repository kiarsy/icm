namespace ICMarkets.Domain.Common;

public sealed class PagedResult<T>(IReadOnlyList<T> items, int page, int pageSize, long totalCount)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public long TotalCount { get; } = totalCount;
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
