using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IShortUrlService _shortUrlService;

    public HomeController(ILogger<HomeController> logger, IShortUrlService shortUrlService)
    {
        _logger = logger;
        _shortUrlService = shortUrlService;
    }

    [HttpGet("{shortCode?}")]
    public IActionResult Index([FromRoute] string? shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return View();
        }

        var shortUrl = _shortUrlService.GetByCode(shortCode);

        if (shortUrl is null)
        {
            return NotFound();
        }

        if (shortUrl.Expired())
        {
            return View("Expired");
        }

        if (!shortUrl.Active)
        {
            return View("Inactive");
        }

        shortUrl.Clicked();

        var originalUrl = shortUrl.OriginalUrl;

        return Redirect(originalUrl);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
