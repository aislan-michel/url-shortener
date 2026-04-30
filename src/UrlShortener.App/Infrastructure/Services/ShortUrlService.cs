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

    public ShortUrl? GetByCode(string shortCode) => _shortUrlRepository.Get(shortCode);

    public void Create(ShortUrl shortUrl)
    {
        if (Exists(shortUrl.ShortCode))
        {
            throw new InvalidOperationException("A short code with this value already exists.");
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
    }

    public void Activate(string shortCode)
    {
        var shortUrl = GetByCode(shortCode);
        if (shortUrl is null)
        {
            throw new KeyNotFoundException("Short URL not found.");
        }

        shortUrl.Activate();
    }

    public void Deactivate(string shortCode)
    {
        var shortUrl = GetByCode(shortCode);
        if (shortUrl is null)
        {
            throw new KeyNotFoundException("Short URL not found.");
        }

        shortUrl.Deactivate();
    }

    public void Delete(string shortCode)
    {
        _shortUrlRepository.Delete(shortCode);
    }

    public bool Exists(string shortCode) => GetByCode(shortCode) is not null;
}
