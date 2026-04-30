using UrlShortener.App.Models;

namespace UrlShortener.App.Infrastructure.Repositories;

public sealed class ShortUrlRepository : IShortUrlRepository
{
    private static IList<ShortUrl> ShortUrls =
    [
        new("https://example.com", "localhost:5282/"),
        new("insta", "https://instagram.com", "localhost:5282/", false)
    ];

    public void Add(ShortUrl shortUrl)
    {
        ShortUrls.Add(shortUrl);
    }

    public void Clear()
    {
        ShortUrls.Clear();
    }

    public ShortUrl? Get(string shortCode)
    {
        return ShortUrls.FirstOrDefault(x =>
            string.Equals(x.ShortCode, shortCode, StringComparison.OrdinalIgnoreCase));
    }

    public ShortUrl[] Get()
    {
        return ShortUrls.ToArray();
    }

    public void Delete(string shortCode)
    {
        var existing = Get(shortCode);
        if (existing is not null)
        {
            ShortUrls.Remove(existing);
        }
    }
}
