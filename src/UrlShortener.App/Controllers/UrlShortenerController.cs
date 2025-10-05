using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

public class UrlShortenerController : Controller
{
    private readonly ILogger<UrlShortenerController> _logger;
    private readonly IShortUrlRepository _shortUrlRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IQrCodeService _qrCodeService;

    public UrlShortenerController(
        ILogger<UrlShortenerController> logger,
        IShortUrlRepository shortUrlRepository,
        IHttpClientFactory httpClientFactory,
        IQrCodeService qrCodeService)
    {
        _logger = logger;
        _shortUrlRepository = shortUrlRepository;
        _httpClientFactory = httpClientFactory;
        _qrCodeService = qrCodeService;
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

        shortUrls = shortUrls.Where(x => x.ShortCode.Contains(shortCode)).ToArray();

        if (shortUrls == null || !shortUrls.Any())
        {
            return View(new List<ShortUrl>());
        }

        return View(shortUrls);
    }

    [HttpGet("UrlShortener/Create")]
    public IActionResult Create()
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
                return View(createShortUrl);
            }
        }
        catch (Exception e)
        {
            ModelState.AddModelError("url", $"url {createShortUrl.Url} is not valid");
            return View(createShortUrl);
        }

        var host = Request.Host.Value!;

        _shortUrlRepository.Add(new ShortUrl(
            createShortUrl.ShortCode, createShortUrl.Url, host, createShortUrl.Expires));

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("UrlShortener/Details/{shortCode}")]
    public IActionResult Details(string shortCode)
    {
        return View(_shortUrlRepository.Get(shortCode)!);
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

    [HttpGet("UrlShortener/Activate/{shortCode}")]
    public IActionResult Activate(string shortCode)
    {
        var shortUrl = _shortUrlRepository.Get(shortCode)!;

        shortUrl.Activate();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("UrlShortener/Deactivate/{shortCode}")]
    public IActionResult Deactivate(string shortCode)
    {
        var shortUrl = _shortUrlRepository.Get(shortCode)!;

        shortUrl.Deactivate();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("UrlShortener/QrCode/{shortCode}")]
    public IActionResult QrCode(string shortCode)
    {
        var shortUrl = _shortUrlRepository.Get(shortCode)!;

        return Json(new { qrCodeBytes = _qrCodeService.Generate($"https://{shortUrl.ShortUrlFull}") });
    }
}