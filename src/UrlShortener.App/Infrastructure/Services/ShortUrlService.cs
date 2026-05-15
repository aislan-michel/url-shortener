using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Models;

namespace UrlShortener.App.Infrastructure.Services;

public sealed class ShortUrlService : IShortUrlService
{
    private readonly IShortUrlRepository _shortUrlRepository;

    public ShortUrlService(IShortUrlRepository shortUrlRepository)
    {
        _shortUrlRepository = shortUrlRepository;
    }

    public ShortUrl[] GetAll() => _shortUrlRepository.Get();

    public ShortUrl[] GetProcessingUrls() => _shortUrlRepository.GetProcessingUrls();

    public ShortUrl? GetByCode(string shortCode) => _shortUrlRepository.Get(shortCode);

    public void Create(ShortUrl shortUrl)
    {
        var exists = _shortUrlRepository.Exists(shortUrl.ShortCode, shortUrl.OriginalUrl);
        if (exists)
        {
            throw new InvalidOperationException("A short code or original URL with this value already exists.");
        }

        _shortUrlRepository.Add(shortUrl);
    }

    public void UpdateExpires(string shortCode, DateOnly? expires)
    {
        var shortUrl = GetByCode(shortCode);
        if (shortUrl is null)
        {
            throw new KeyNotFoundException("Short URL not found.");
        }

        shortUrl.UpdateExpiresDate(expires);
        _shortUrlRepository.SaveChanges();
    }

    public void Activate(string shortCode)
    {
        var shortUrl = GetByCode(shortCode);
        if (shortUrl is null)
        {
            throw new KeyNotFoundException("Short URL not found.");
        }

        shortUrl.Activate();
        _shortUrlRepository.SaveChanges();
    }

    public void Deactivate(string shortCode, string? description = null)
    {
        var shortUrl = GetByCode(shortCode);
        if (shortUrl is null)
        {
            throw new KeyNotFoundException("Short URL not found.");
        }

        shortUrl.Deactivate(description);
        _shortUrlRepository.SaveChanges();
    }

    public void Invalidate(string shortCode, string? description = null)
    {
        var shortUrl = GetByCode(shortCode);
        if (shortUrl is null)
        {
            throw new KeyNotFoundException("Short URL not found.");
        }

        shortUrl.Invalidate(description);
        _shortUrlRepository.SaveChanges();
    }

    public void Delete(string shortCode)
    {
        _shortUrlRepository.Delete(shortCode);
    }

    public bool Exists(string shortCode) => GetByCode(shortCode) is not null;
}
