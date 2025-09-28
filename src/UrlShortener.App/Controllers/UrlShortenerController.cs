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
    public async Task<IActionResult> Create(string url, DateOnly? expires = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _errors.Add($"url {url} is not valid");
            return RedirectToAction(nameof(Index));
        }

        var httpClient = _httpClientFactory.CreateClient("validate-url");

        httpClient.BaseAddress = new Uri(url);

        var request = new HttpRequestMessage();

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _errors.Add($"url {url} is not valid");
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception e)
        {
            _errors.Add($"url {url} is not valid");
            return RedirectToAction(nameof(Index));
        }

        var host = Request.Host.Value!;

        _shortUrlRepository.Add(new ShortUrl(url, host, expires));

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
    public IActionResult Update(string shortCode, object model)
    {
        return RedirectToAction(nameof(Index));
    }
}