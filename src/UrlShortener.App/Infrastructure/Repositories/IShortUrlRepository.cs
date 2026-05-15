using System.Linq.Expressions;
using UrlShortener.App.Models;

namespace UrlShortener.App.Infrastructure.Repositories;

public interface IShortUrlRepository
{
    void Add(ShortUrl shortUrl);
    void Clear();
    ShortUrl? Get(string shortCode);
    ShortUrl[] Get();
    ShortUrl[] GetProcessingUrls();
    void Delete(string shortCode);
    void SaveChanges();
    bool Exists(string? shortCode, string? originalUrl);
}