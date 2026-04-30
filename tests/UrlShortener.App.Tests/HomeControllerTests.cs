using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using UrlShortener.App.Controllers;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;
using Xunit;

namespace UrlShortener.App.Tests;

public class HomeControllerTests
{
    private HomeController CreateController(IShortUrlService shortUrlService)
    {
        return new HomeController(new NullLogger<HomeController>(), shortUrlService);
    }

    [Fact]
    public void Index_ReturnsRedirectResult_ForActiveShortUrl()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("abc123", "https://example.com", "localhost:5000/");
        service.Create(shortUrl);
        var controller = CreateController(service);

        var result = controller.Index("abc123");

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("https://example.com", redirect.Url);
        Assert.Equal(1, shortUrl.ClickCounter);
    }

    [Fact]
    public void Index_ReturnsNotFound_ForMissingShortCode()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service);

        var result = controller.Index("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Index_ReturnsExpiredView_ForExpiredShortUrl()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("expired", "https://example.com", "localhost:5000/");
        shortUrl.UpdateExpiresDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        service.Create(shortUrl);
        Assert.NotNull(service.GetByCode("expired"));
        var controller = CreateController(service);

        var result = controller.Index("expired");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Expired", view.ViewName);
    }

    [Fact]
    public void Index_ReturnsInactiveView_ForInactiveShortUrl()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var shortUrl = new ShortUrl("inactive", "https://example.com", "localhost:5000/");
        service.Create(shortUrl);
        service.Deactivate("inactive");
        var controller = CreateController(service);

        var result = controller.Index("inactive");

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Inactive", view.ViewName);
    }
}
