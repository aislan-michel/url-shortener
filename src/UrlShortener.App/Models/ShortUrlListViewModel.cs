namespace UrlShortener.App.Models;

public sealed class ShortUrlListViewModel
{
    public ShortUrlListViewModel(IEnumerable<ShortUrl> shortUrls, string? shortCodeFilter, int pageIndex, int pageSize, int totalItems)
    {
        ShortUrls = shortUrls;
        ShortCodeFilter = shortCodeFilter;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalItems = totalItems;
    }

    public IEnumerable<ShortUrl> ShortUrls { get; }

    public string? ShortCodeFilter { get; }

    public int PageIndex { get; }

    public int PageSize { get; }

    public int TotalItems { get; }

    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => PageIndex > 1;

    public bool HasNextPage => PageIndex < TotalPages;

    public int StartItem => TotalItems == 0 ? 0 : (PageIndex - 1) * PageSize + 1;

    public int EndItem => Math.Min(PageIndex * PageSize, TotalItems);
}
