using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

public class UrlShortenerController : Controller
{
    private readonly ILogger<UrlShortenerController> _logger;
    private readonly IShortUrlRepository _shortUrlRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public UrlShortenerController(
        ILogger<UrlShortenerController> logger,
        IShortUrlRepository shortUrlRepository,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _shortUrlRepository = shortUrlRepository;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("UrlShortener")]
    public IActionResult Index(string? shortCode = null)
    {
        ViewData["shortCode"] = shortCode;

        var shortUrls = _shortUrlRepository.Get();

        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return View(shortUrls);
        }

        var shortUrl = shortUrls.FirstOrDefault(x => x.ShortCode == shortCode);

        if (shortUrl == null)
        {
            return View(new List<ShortUrl>());
        }

        return View(new List<ShortUrl>() { shortUrl });
    }

    [HttpGet("UrlShortener/Create")]
    public async Task<IActionResult> Create()
    {
        return View(new CreateShortUrl());
    }

    [HttpPost("UrlShortener/Create")]
    public async Task<IActionResult> Create(CreateShortUrl createShortUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(createShortUrl);
        }

        var httpClient = _httpClientFactory.CreateClient("validate-url");

        httpClient.BaseAddress = new Uri(createShortUrl.Url);

        var request = new HttpRequestMessage();

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("url", $"url {createShortUrl.Url} is not valid");
                return View();
            }
        }
        catch (Exception e)
        {
            ModelState.AddModelError("url", $"url {createShortUrl.Url} is not valid");
            return View();
        }

        var host = Request.Host.Value!;

        _shortUrlRepository.Add(new ShortUrl(
            createShortUrl.ShortCode, createShortUrl.Url, host, createShortUrl.Expires));

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("UrlShortener/Clear")]
    public IActionResult Clear()
    {
        _shortUrlRepository.Clear();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("UrlShortener/Update/{shortCode}")]
    public IActionResult Update(string shortCode)
    {
        var shortUrl = _shortUrlRepository.Get(shortCode);

        return View(shortUrl);
    }

    [HttpPost("UrlShortener/Update/{shortCode}")]
    public IActionResult Update(string shortCode, DateOnly? expires = null)
    {
        var shortUrl = _shortUrlRepository.Get(shortCode)!;

        shortUrl.UpdateExpiresDate(expires);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("UrlShortener/Delete/{shortCode}")]
    public IActionResult Delete(string shortCode)
    {
        _shortUrlRepository.Delete(shortCode);

        return RedirectToAction(nameof(Index));
    }
}