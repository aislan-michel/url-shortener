using Microsoft.AspNetCore.Mvc;
using UrlShortener.App.Infrastructure.Repositories;
using UrlShortener.App.Infrastructure.Services;
using UrlShortener.App.Models;

namespace UrlShortener.App.Controllers;

[Route("UrlShortener")]
public class UrlShortenerController(
    ILogger<UrlShortenerController> logger,
    IShortUrlRepository shortUrlRepository,
    IHttpClientFactory httpClientFactory,
    IQrCodeService qrCodeService)
    : Controller
{
    [HttpGet("")]
    public IActionResult Index(string? shortCode = null)
    {
        ViewData["shortCode"] = shortCode;

        var shortUrls = shortUrlRepository.Get();

        if (string.IsNullOrWhiteSpace(shortCode))
        {
            return View(shortUrls);
        }

        shortUrls = shortUrls.Where(x => x.ShortCode.Contains(shortCode)).ToArray();

        if (shortUrls.Length == 0)
        {
            return View(new List<ShortUrl>());
        }

        return View(shortUrls);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CreateShortUrl());
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(CreateShortUrl createShortUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(createShortUrl);
        }

        var httpClient = httpClientFactory.CreateClient("validate-url");

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

        shortUrlRepository.Add(new ShortUrl(
            createShortUrl.ShortCode, createShortUrl.Url, host, createShortUrl.Expires));

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Details/{shortCode}")]
    public IActionResult Details(string shortCode)
    {
        return View(shortUrlRepository.Get(shortCode)!);
    }

    [HttpGet("Update/{shortCode}")]
    public IActionResult Update(string shortCode)
    {
        return View(shortUrlRepository.Get(shortCode));
    }

    [HttpPost("Update/{shortCode}")]
    public IActionResult Update(string shortCode, DateOnly? expires = null)
    {
        shortUrlRepository.Get(shortCode)!.UpdateExpiresDate(expires);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Delete/{shortCode}")]
    public IActionResult Delete(string shortCode)
    {
        shortUrlRepository.Delete(shortCode);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Activate/{shortCode}")]
    public IActionResult Activate(string shortCode)
    {
        shortUrlRepository.Get(shortCode)!.Activate();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Deactivate/{shortCode}")]
    public IActionResult Deactivate(string shortCode)
    {
        shortUrlRepository.Get(shortCode)!.Deactivate();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("QrCode/{shortCode}")]
    public IActionResult QrCode(string shortCode)
    {
        var shortUrl = shortUrlRepository.Get(shortCode)!;

        return Json(new { qrCodeBytes = qrCodeService.Generate($"https://{shortUrl.ShortUrlFull}") });
    }
}