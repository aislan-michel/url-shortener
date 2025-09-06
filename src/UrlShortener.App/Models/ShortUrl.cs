namespace UrlShortener.App.Models;

public sealed class ShortUrl
{
    private ShortUrl() { }

    public ShortUrl(string originalUrl, string shortCode, string baseUrl)
    {
        Id = Guid.NewGuid().ToString();
        OriginalUrl = originalUrl;
        ShortCode = shortCode;
        ShortUrlFull = $"{baseUrl.TrimEnd('/')}/{shortCode}";
        CreatedAt = DateTime.Now;
    }

    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string OriginalUrl { get; private set; } = string.Empty;
    public string ShortCode { get; private set; } = string.Empty;
    public string ShortUrlFull { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
}