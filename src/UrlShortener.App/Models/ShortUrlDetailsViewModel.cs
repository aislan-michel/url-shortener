namespace UrlShortener.App.Models;

public sealed class ShortUrlDetailsViewModel
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string ShortUrlFull { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateOnly? Expires { get; set; }
    public StatusViewModel Status { get; set; } = new();
    public int ClickCount { get; set; }
}