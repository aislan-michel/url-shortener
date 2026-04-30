using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using UrlShortener.App.Controllers;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;
using Xunit;

namespace UrlShortener.App.Tests;

public class UrlShortenerControllerTests
{
    private UrlShortenerController CreateController(IShortUrlService service, HttpMessageHandler handler, IQrCodeService qrCodeService)
    {
        var factory = new FakeHttpClientFactory(handler);
        var controller = new UrlShortenerController(new NullLogger<UrlShortenerController>(), service, factory, qrCodeService);
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
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        service.Create(new ShortUrl("abc123", "https://example.com", "localhost:5000/"));
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ShortUrl[]>(view.Model!);
        Assert.Single(model);
    }

    [Fact]
    public void Details_ReturnsNotFound_ForMissingCode()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.Details("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Details_ReturnsView_ForExistingShortCode()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        service.Create(new ShortUrl("abc123", "https://example.com", "localhost:5000/"));
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = controller.Details("abc123");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ShortUrl>(view.Model);
        Assert.Equal("abc123", model.ShortCode);
    }

    [Fact]
    public async Task CreatePost_ReturnsView_WhenModelInvalid()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());
        controller.ModelState.AddModelError("Url", "Required");

        var result = await controller.Create(new CreateShortUrl());

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<CreateShortUrl>(view.Model);
    }

    [Fact]
    public async Task CreatePost_ReturnsView_WhenUrlValidationFails()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.BadRequest), new FakeQrCodeService());

        var result = await controller.Create(new CreateShortUrl { Url = "https://example.com" });

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsType<CreateShortUrl>(view.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("Url", controller.ModelState.Keys);
    }

    [Fact]
    public async Task CreatePost_Redirects_WhenSuccessful()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
        var service = new ShortUrlService(repository);
        var controller = CreateController(service, new FakeHttpMessageHandler(HttpStatusCode.OK), new FakeQrCodeService());

        var result = await controller.Create(new CreateShortUrl { Url = "https://example.com" });

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void QrCode_ReturnsJson_ForExistingShortCode()
    {
        var repository = new ShortUrlRepository();
        repository.Clear();
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
