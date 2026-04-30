using UrlShortener.App.Models;

namespace UrlShortener.App.Infrastructure.Services;

public interface IShortUrlService
{
    ShortUrl[] GetAll();
    ShortUrl? GetByCode(string shortCode);
    void Create(ShortUrl shortUrl);
    void UpdateExpires(string shortCode, DateOnly? expires);
    void Activate(string shortCode);
    void Deactivate(string shortCode);
    void Delete(string shortCode);
    bool Exists(string shortCode);
}
