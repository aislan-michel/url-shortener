using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using UrlShortener.App.Controllers;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;
using Xunit;

namespace UrlShortener.App.Tests;

public class ShortUrlServiceTests
{
    private ShortUrlRepository CreateRepository()
    {
        var repository = new ShortUrlRepository();
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
    public void Create_Throws_WhenDuplicateShortCodeExists()
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

        Assert.Contains("short code", exception.Message, StringComparison.OrdinalIgnoreCase);
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
        Assert.False(deactivated!.Active);

        service.Activate("act123");
        var activated = service.GetByCode("act123");
        Assert.NotNull(activated);
        Assert.True(activated!.Active);
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
