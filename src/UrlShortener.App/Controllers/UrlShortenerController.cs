using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

public class UrlShortenerController : Controller
{
    private readonly ILogger<UrlShortenerController> _logger;
    private readonly IShortUrlRepository _shortUrlRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private static IList<string> _errors = [];

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
    public IActionResult Index()
    {
        var shortUrls = _shortUrlRepository.Get();

        ViewData["Errors"] = _errors;

        return View(shortUrls);
    }

    [HttpPost("UrlShortener/Create")]
    public async Task<IActionResult> Create(CreateShortUrl createShortUrl)
    {
        if (string.IsNullOrWhiteSpace(createShortUrl.Url))
        {
            _errors.Add($"url {createShortUrl.Url} is not valid");
            return RedirectToAction(nameof(Index));
        }

        var httpClient = _httpClientFactory.CreateClient("validate-url");

        httpClient.BaseAddress = new Uri(createShortUrl.Url);

        var request = new HttpRequestMessage();

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _errors.Add($"url {createShortUrl.Url} is not valid");
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception e)
        {
            _errors.Add($"url {createShortUrl.Url} is not valid");
            return RedirectToAction(nameof(Index));
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

        _logger.LogInformation("expires in " + shortUrl.Expires);

        return View(shortUrl);
    }

    [HttpPost("UrlShortener/Update/{shortCode}")]
    public IActionResult Update(string shortCode, DateOnly? expires = null)
    {
        var shortUrl = _shortUrlRepository.Get(shortCode)!;

        shortUrl.UpdateExpiresDate(expires);

        return RedirectToAction(nameof(Index));
    }
}