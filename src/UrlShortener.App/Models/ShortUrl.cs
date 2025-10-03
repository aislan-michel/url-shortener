namespace UrlShortener.App.Models;

public sealed class ShortUrl
{
    private ShortUrl() { }

    public ShortUrl(string shortCode, string originalUrl, string baseUrl, bool active)
    {
        ShortCode = shortCode;
        OriginalUrl = originalUrl;
        ShortUrlFull = GenerateShortUrlFull(baseUrl, shortCode);
        Active = active;
    }

    public ShortUrl(string? shortCode, string originalUrl, string baseUrl, DateOnly? expires = null)
    {
        OriginalUrl = originalUrl;
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            shortCode = GenerateShortCode();
        }
        ShortCode = shortCode;
        ShortUrlFull = GenerateShortUrlFull(baseUrl, shortCode);
        Expires = expires;
    }

    public ShortUrl(string originalUrl, string baseUrl, DateOnly? expires = null)
    {
        OriginalUrl = originalUrl;

        var shortCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToLowerInvariant();
        ShortCode = shortCode;
        ShortUrlFull = GenerateShortUrlFull(baseUrl, shortCode);
        Expires = expires;
    }

    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public string OriginalUrl { get; private set; } = string.Empty;
    public string ShortCode { get; private set; } = string.Empty;
    public string ShortUrlFull { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.Now;
    public DateOnly? Expires { get; private set; } = null;
    public bool Active { get; private set; } = true;

    private string GenerateShortUrlFull(string baseUrl, string shortCode)
    {
        return $"{baseUrl.TrimEnd('/')}/{shortCode}";
    }

    private string GenerateShortCode()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8).ToLowerInvariant();
    }

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

    public void Activate()
    {
        Active = true;
    }

    public void Deactivate()
    {
        Active = false;
    }
}