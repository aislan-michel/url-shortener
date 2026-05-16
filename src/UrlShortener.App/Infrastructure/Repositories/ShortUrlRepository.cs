using Microsoft.EntityFrameworkCore;
using UrlShortener.App.Infrastructure.Persistence;
using UrlShortener.App.Models.Entities;

namespace UrlShortener.App.Infrastructure.Repositories;

public sealed class ShortUrlRepository : IShortUrlRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public ShortUrlRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(ShortUrl shortUrl)
    {
        _dbContext.ShortUrls.Add(shortUrl);
        _dbContext.SaveChanges();
    }

    public void Clear()
    {
        _dbContext.ShortUrls.RemoveRange(_dbContext.ShortUrls);
        _dbContext.SaveChanges();
    }

    public ShortUrl? Get(string shortCode)
    {
        return _dbContext.ShortUrls.FirstOrDefault(x =>
            EF.Functions.Collate(x.ShortCode, "NOCASE") == shortCode);
    }

    public ShortUrl[] Get()
    {
        return _dbContext.ShortUrls.OrderByDescending(x => x.CreatedAt).ToArray();
    }

    public ShortUrl[] GetProcessingUrls()
    {
        return _dbContext.ShortUrls
            .Where(x => EF.Functions.Collate(x.Status.Value, "NOCASE") == "Processing")
            .ToArray();
    }

    public void Delete(string shortCode)
    {
        var existing = Get(shortCode);
        if (existing is not null)
        {
            _dbContext.ShortUrls.Remove(existing);
            _dbContext.SaveChanges();
        }
    }
    
    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }

    public bool Exists(string? shortCode, string? originalUrl)
    {
        return _dbContext.ShortUrls.Any(x =>
            (!string.IsNullOrWhiteSpace(shortCode) && EF.Functions.Collate(x.ShortCode, "NOCASE") == shortCode) ||
            (!string.IsNullOrWhiteSpace(originalUrl) && EF.Functions.Collate(x.OriginalUrl, "NOCASE") == originalUrl));
    }
}
