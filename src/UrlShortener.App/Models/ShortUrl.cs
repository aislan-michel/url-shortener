namespace UrlShortener.App.Models;

public sealed class ShortUrl
{
    private ShortUrl() { }

    public ShortUrl(string? shortCode, string originalUrl, string baseUrl, DateOnly? expires = null)
    {
        OriginalUrl = originalUrl;

        if (string.IsNullOrWhiteSpace(shortCode))
        {
            shortCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToLowerInvariant();
        }
        ShortCode = shortCode;

        ShortUrlFull = $"{baseUrl.TrimEnd('/')}/{shortCode}";

        Expires = expires;
    }

    public ShortUrl(string originalUrl, string baseUrl, DateOnly? expires = null)
    {
        OriginalUrl = originalUrl;

        var shortCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToLowerInvariant();
        ShortCode = shortCode;

        ShortUrlFull = $"{baseUrl.TrimEnd('/')}/{shortCode}";

        Expires = expires;
    }


    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string OriginalUrl { get; private set; } = string.Empty;
    public string ShortCode { get; private set; } = string.Empty;
    public string ShortUrlFull { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateOnly? Expires { get; private set; } = null;

    public bool Expired()
    {
        if (!Expires.HasValue)
        {
            return false;
        }

        var dateTimeUtcNow = DateTime.UtcNow.Date;

        var dateNow = new DateOnly(dateTimeUtcNow.Year, dateTimeUtcNow.Month, dateTimeUtcNow.Day);

        return dateNow > Expires.Value;
    }

    public void UpdateExpiresDate(DateOnly? expires)
    {
        Expires = expires;
    }
}