using UrlShortener.App.Models;

namespace UrlShortener.App.Infrastructure.Repositories;

public interface IShortUrlRepository
{
    void Add(ShortUrl shortUrl);
    void Clear();
    ShortUrl? Get(string shortCode);
    ShortUrl[] Get();
}