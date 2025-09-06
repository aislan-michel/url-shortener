using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

public class UrlShortenerController : Controller
{
    private readonly ILogger<UrlShortenerController> _logger;
    private readonly IShortUrlRepository _shortUrlRepository;

    public UrlShortenerController(ILogger<UrlShortenerController> logger, IShortUrlRepository shortUrlRepository)
    {
        _logger = logger;
        _shortUrlRepository = shortUrlRepository;
    }

    [HttpGet("UrlShortener")]
    public IActionResult Index()
    {
        var shortUrls = _shortUrlRepository.Get();

        return View(shortUrls);
    }

    [HttpPost]
    public IActionResult Shortener(string url)
    {
        var host = Request.Host.Value!;

        var shortCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToLowerInvariant();

        _shortUrlRepository.Add(new ShortUrl(url, shortCode, host));

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("UrlShortener/Clear")]
    public IActionResult Clear()
    {
        _shortUrlRepository.Clear();

        return RedirectToAction(nameof(Index));
    }
}