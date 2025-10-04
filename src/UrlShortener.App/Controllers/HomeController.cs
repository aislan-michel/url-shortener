using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IShortUrlRepository _shortUrlRepository;

    public HomeController(ILogger<HomeController> logger, IShortUrlRepository shortUrlRepository)
    {
        _logger = logger;
        _shortUrlRepository = shortUrlRepository;
    }

    [HttpGet("{shortCode?}")]
    public IActionResult Index([FromRoute] string? shortCode)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return View();
        }

        var shortUrl = _shortUrlRepository.Get(shortCode);

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
