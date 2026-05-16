using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using UrlShortener.App.Controllers;
using Microsoft.EntityFrameworkCore;
using UrlShortener.App.Infrastructure.Persistence;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;
using UrlShortener.App.Models.Entities;
using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace UrlShortener.App.Tests;

public class UrlShortenerControllerTests
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

    private UrlShortenerController CreateController(IShortUrlService service, HttpMessageHandler handler, IQrCodeService qrCodeService)
    {
        var controller = new UrlShortenerController(new NullLogger<UrlShortenerController>(), service, qrCodeService)
        {
            TempData = new Mock<ITempDataDictionary>().Object
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                Request = { Host = new HostString("localhost:5000") }
            }
        };
        return controller;
    }

    [Fact]
    public void Index_ReturnsView_WithAllShortUrls()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        service.Create(new ShortUrl("abc123", "https://example.com", "localhost:5000/"));
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ShortUrlListViewModel>(view.Model!);
        Assert.Single(model.ShortUrls);
    }

    [Fact]
    public void Details_ReturnsNotFound_ForMissingCode()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.Details("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Details_ReturnsView_ForExistingShortCode()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        service.Create(new ShortUrl("abc123", "https://example.com", "localhost:5000/"));
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.Details("abc123");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ShortUrlDetailsViewModel>(view.Model);
        Assert.Equal("abc123", model.ShortCode);
    }

    [Fact]
    public void CreatePost_ReturnsView_WhenModelInvalid()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());
        controller.ModelState.AddModelError("Url", "Required");

        var result = controller.Create(new ShortUrlCreateInputModel());

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<ShortUrlCreateInputModel>(view.Model);
    }

    [Fact]
    public void CreatePost_ReturnsView_WhenUrlValidationFails()
    {
        var repository = CreateRepository();
        var serviceMock = new Mock<IShortUrlService>();
        serviceMock.Setup(s => s.Create(It.IsAny<ShortUrl>())).Throws(new InvalidOperationException("Invalid URL"));

        var controller = CreateController(serviceMock.Object, new FakeHttpMessageHandler(HttpStatusCode.BadRequest), new FakeQrCodeService());

        var result = controller.Create(new ShortUrlCreateInputModel { Url = "https://example.com" });

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<ShortUrlCreateInputModel>(view.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("ShortCode", controller.ModelState.Keys);
    }

    [Fact]
    public void CreatePost_Redirects_WhenSuccessful()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.Create(new ShortUrlCreateInputModel { Url = "https://example.com" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void QrCode_ReturnsJson_ForExistingShortCode()
    {
        var repository = CreateRepository();
        var service = new ShortUrlService(repository);
        service.Create(new ShortUrl("abc123", "https://example.com", "localhost:5000/"));
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.QrCode("abc123");

        var json = Assert.IsType<JsonResult>(result);
        Assert.NotNull(json.Value);
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public FakeHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler, disposeHandler: false);
        }
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public FakeHttpMessageHandler(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                RequestMessage = request
            };

            return Task.FromResult(response);
        }
    }

    private sealed class FakeQrCodeService : IQrCodeService
    {
        public byte[] Generate(string url) => new byte[] { 1, 2, 3 };
    }
}
