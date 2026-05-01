using UrlShortener.App.Models;

namespace UrlShortener.App.Infrastructure.Repositories;

public sealed class ShortUrlRepository : IShortUrlRepository
{
    private static IList<ShortUrl> ShortUrls =
    [
        new("google", "https://www.google.com", "http://localhost:5282/"),
        new("youtube", "https://www.youtube.com", "http://localhost:5282/"),
        new("wikipedia", "https://www.wikipedia.org", "http://localhost:5282/", false),
        new("amazon", "https://www.amazon.com", "http://localhost:5282/", DateOnly.FromDateTime(DateTime.Now.AddDays(-2))),
        new("facebook", "https://www.facebook.com", "http://localhost:5282/"),
        new("instagram", "https://www.instagram.com", "http://localhost:5282/"),
        new("twitter", "https://www.twitter.com", "http://localhost:5282/"),
        new("linkedin", "https://www.linkedin.com", "http://localhost:5282/"),
        new("netflix", "https://www.netflix.com", "http://localhost:5282/"),
        new("spotify", "https://www.spotify.com", "http://localhost:5282/"),
        new("github", "https://www.github.com", "http://localhost:5282/"),
        new("reddit", "https://www.reddit.com", "http://localhost:5282/"),
        new("office", "https://www.office.com", "http://localhost:5282/"),
        new("whatsapp", "https://www.whatsapp.com", "http://localhost:5282/"),
        new("stackoverflow", "https://stackoverflow.com", "http://localhost:5282/")
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
