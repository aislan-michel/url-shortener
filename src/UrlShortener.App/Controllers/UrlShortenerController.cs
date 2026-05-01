using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

[Route("UrlShortener")]
public class UrlShortenerController : Controller
{
    private readonly ILogger<UrlShortenerController> _logger;
    private readonly IShortUrlService _shortUrlService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IQrCodeService _qrCodeService;

    public UrlShortenerController(
        ILogger<UrlShortenerController> logger,
        IShortUrlService shortUrlService,
        IHttpClientFactory httpClientFactory,
        IQrCodeService qrCodeService)
    {
        _logger = logger;
        _shortUrlService = shortUrlService;
        _httpClientFactory = httpClientFactory;
        _qrCodeService = qrCodeService;
    }

    [HttpGet("")]
    public IActionResult Index(string? shortCode = null, int page = 1, int pageSize = 10)
    {
        var shortUrls = _shortUrlService.GetAll();

        if (!string.IsNullOrWhiteSpace(shortCode))
        {
            shortUrls = shortUrls.Where(x => x.ShortCode.Contains(shortCode)).ToArray();
        }

        var totalItems = shortUrls.Length;
        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        var totalPages = totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page < 1)
        {
            page = 1;
        }
        else if (page > totalPages)
        {
            page = totalPages;
        }

        var pagedUrls = shortUrls.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return View(new ShortUrlListViewModel(pagedUrls, shortCode, page, pageSize, totalItems));
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateShortUrl());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
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
                ModelState.AddModelError(nameof(CreateShortUrl.Url), $"The URL '{createShortUrl.Url}' is not valid.");
                return View(createShortUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "URL validation failed for {Url}", createShortUrl.Url);
            ModelState.AddModelError(nameof(CreateShortUrl.Url), $"The URL '{createShortUrl.Url}' is not valid.");
            return View(createShortUrl);
        }

        var host = Request.Host.Value!;
        var shortUrl = new ShortUrl(createShortUrl.ShortCode, createShortUrl.Url, host, createShortUrl.Expires);

        try
        {
            _shortUrlService.Create(shortUrl);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("ShortCode", ex.Message);
            return View(createShortUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Details/{shortCode}")]
    public IActionResult Details(string shortCode)
    {
        var shortUrl = _shortUrlService.GetByCode(shortCode);
        if (shortUrl is null)
        {
            return NotFound();
        }

        return View(shortUrl);
    }

    [HttpGet("Update/{shortCode}")]
    public IActionResult Update(string shortCode)
    {
        var shortUrl = _shortUrlService.GetByCode(shortCode);
        if (shortUrl is null)
        {
            return NotFound();
        }

        return View(shortUrl);
    }

    [HttpPost("Update/{shortCode}")]
    [ValidateAntiForgeryToken]
    public IActionResult Update(string shortCode, DateOnly? expires = null)
    {
        try
        {
            _shortUrlService.UpdateExpires(shortCode, expires);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Delete/{shortCode}")]
    public IActionResult Delete(string shortCode)
    {
        _shortUrlService.Delete(shortCode);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Activate/{shortCode}")]
    public IActionResult Activate(string shortCode)
    {
        try
        {
            _shortUrlService.Activate(shortCode);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Deactivate/{shortCode}")]
    public IActionResult Deactivate(string shortCode)
    {
        try
        {
            _shortUrlService.Deactivate(shortCode);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("QrCode/{shortCode}")]
    public IActionResult QrCode(string shortCode)
    {
        var shortUrl = _shortUrlService.GetByCode(shortCode);
        if (shortUrl is null)
        {
            return NotFound();
        }

        return Json(new { qrCodeBytes = _qrCodeService.Generate($"https://{shortUrl.ShortUrlFull}") });
    }
}