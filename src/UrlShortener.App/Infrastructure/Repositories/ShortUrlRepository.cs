using UrlShortener.App.Models;

namespace UrlShortener.App.Infrastructure.Repositories;

public sealed class ShortUrlRepository : IShortUrlRepository
{
    private static IList<ShortUrl> ShortUrls =
    [
        new("https://example.com", "localhost:5282/")
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
        return ShortUrls.FirstOrDefault(x => x.ShortCode == shortCode);
    }

    public ShortUrl[] Get()
    {
        return ShortUrls.ToArray();
    }

    public void Delete(string shortCode)
    {
        ShortUrls.Remove(ShortUrls.First(x => x.ShortCode == shortCode));
    }
}
