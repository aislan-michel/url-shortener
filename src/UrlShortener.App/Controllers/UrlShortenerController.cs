using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

[Route("UrlShortener")]
public class UrlShortenerController : Controller
{
    private readonly ILogger<UrlShortenerController> _logger;
    private readonly IShortUrlService _shortUrlService;
    private readonly IQrCodeService _qrCodeService;
    

    public UrlShortenerController(
        ILogger<UrlShortenerController> logger,
        IShortUrlService shortUrlService,
        IQrCodeService qrCodeService)
    {
        _logger = logger;
        _shortUrlService = shortUrlService;
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
        TempData["Page"] = page;

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

        return RedirectToAction(nameof(Index), new { page = TempData["Page"] });
    }

    [HttpGet("Details/{shortCode}")]
    public IActionResult Details(string shortCode)
    {
        var shortUrl = _shortUrlService.GetByCode(shortCode);
        if (shortUrl is null)
        {
            return NotFound();
        }

        var shortUrlDetailsViewModel = new ShortUrlDetailsViewModel
        {
            OriginalUrl = shortUrl.OriginalUrl,
            ShortCode = shortUrl.ShortCode,
            ShortUrlFull = shortUrl.ShortUrlFull,
            CreatedAt = shortUrl.CreatedAt,
            Expires = shortUrl.Expires,
            Status = new StatusViewModel
            {
                Value = shortUrl.Status.Value,
                Description = shortUrl.Status.Description
            },
            ClickCount = shortUrl.ClickCounter
        };

        return View(shortUrlDetailsViewModel);
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

        return RedirectToAction(nameof(Index), new { page = TempData["Page"] });
    }

    [HttpGet("Delete/{shortCode}")]
    public IActionResult Delete(string shortCode)
    {
        _shortUrlService.Delete(shortCode);

        return RedirectToAction(nameof(Index), new { page = TempData["Page"] });
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

        return RedirectToAction(nameof(Index), new { page = TempData["Page"] });
    }

    [HttpGet("Deactivate/{shortCode}")]
    public IActionResult Deactivate(string shortCode)
    {
        try
        {
            _shortUrlService.Deactivate(shortCode, "Desativado manualmente pelo usuário.");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index), new { page = TempData["Page"] });
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