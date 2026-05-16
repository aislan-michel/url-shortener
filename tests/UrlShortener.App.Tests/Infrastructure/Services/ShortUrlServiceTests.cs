using Microsoft.EntityFrameworkCore;
using UrlShortener.App.Infrastructure.Persistence;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models.Entities;
using Xunit;

namespace UrlShortener.App.Tests.Infrastructure.Services;

public class ShortUrlServiceTests
{
    private static ShortUrlRepository CreateRepository()
    {
        var options = new DbContextOptionsBuilder<UrlShortenerDbContext>()
            .UseSqlite($"Data Source={Path.GetTempFileName()}")
            .Options;
        var dbContext = new UrlShortenerDbContext(options);
        dbContext.Database.EnsureCreated();
        var repository = new ShortUrlRepository(dbContext);
        repository.Clear();
        return repository;
    }

    [Fact]
    public void Create_AddsShortUrl_WhenShortCodeIsUnique()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("abc123", "https://example.com", "localhost:5000/");

        service.Create(shortUrl);

        var result = service.GetByCode("abc123");

        Assert.NotNull(result);
        Assert.Equal("https://example.com", result!.OriginalUrl);
        Assert.Equal("abc123", result.ShortCode);
    }

    [Fact]
    public void Create_Throws_WhenDuplicateShortCodeOrUrlExists()
    {
        var repository = CreateRepository();
        Assert.Empty(repository.Get());
        var service = new ShortUrlService(repository);
        var first = new ShortUrl("abc123", "https://example.com", "localhost:5000/");
        service.Create(first);

        Assert.Single(service.GetAll());
        Assert.NotNull(service.GetByCode("abc123"));

        var duplicate = new ShortUrl("abc123", "https://example.org", "localhost:5000/");

        var exception = Assert.Throws<InvalidOperationException>(() => service.Create(duplicate));

        Assert.Contains("A short code or original URL with this value already exists.", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetByCode_IsCaseInsensitive()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("abc123", "https://example.com", "localhost:5000/");
        service.Create(shortUrl);

        var result = service.GetByCode("ABC123");

        Assert.NotNull(result);
        Assert.Equal("abc123", result!.ShortCode);
    }

    [Fact]
    public void UpdateExpires_UpdatesExpirationDate()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("exp123", "https://example.com", "localhost:5000/");
        service.Create(shortUrl);

        var newExpires = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        service.UpdateExpires("exp123", newExpires);

        var result = service.GetByCode("exp123");

        Assert.NotNull(result);
        Assert.Equal(newExpires, result!.Expires);
    }

    [Fact]
    public void Activate_Deactivate_ChangeActiveState()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("act123", "https://example.com", "localhost:5000/");
        service.Create(shortUrl);

        service.Deactivate("act123");
        var deactivated = service.GetByCode("act123");
        Assert.NotNull(deactivated);
        Assert.Equal("Inactive", deactivated!.Status.Value);

        service.Activate("act123");
        var activated = service.GetByCode("act123");
        Assert.NotNull(activated);
        Assert.Equal("Active", activated!.Status.Value);
    }

    [Fact]
    public void Delete_RemovesShortUrl()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("del123", "https://example.com", "localhost:5000/");
        service.Create(shortUrl);

        service.Delete("del123");

        Assert.Null(service.GetByCode("del123"));
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void Exists_ReturnsFalse_WhenShortUrlMissing()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);

        Assert.False(service.Exists("missing"));
    }
}
