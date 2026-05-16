using UrlShortener.App.Models.Entities;

namespace UrlShortener.App.Infrastructure.Services;

public interface IShortUrlService
{
    ShortUrl[] GetAll();
    ShortUrl[] GetProcessingUrls();
    ShortUrl? GetByCode(string shortCode);
    void Create(ShortUrl shortUrl);
    void UpdateExpires(string shortCode, DateOnly? expires);
    void Activate(string shortCode);
    void Deactivate(string shortCode, string? description = null);
    void Invalidate(string shortCode, string? description = null);
    void Delete(string shortCode);
    bool Exists(string shortCode);
}
