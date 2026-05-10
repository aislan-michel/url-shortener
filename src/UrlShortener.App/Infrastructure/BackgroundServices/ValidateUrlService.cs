using UrlShortener.App.Infrastructure.Services;

namespace UrlShortener.App.Infrastructure.BackgroundServices;

public sealed class ValidateUrlService(
    ILogger<ValidateUrlService> logger, 
    IServiceScopeFactory serviceScopeFactory,
    IHttpClientFactory httpClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
           using IServiceScope scope = serviceScopeFactory.CreateScope();

            var shortUrlService = scope.ServiceProvider.GetRequiredService<IShortUrlService>();

            var processingUrls = shortUrlService.GetProcessingUrls();

            foreach (var url in processingUrls)
            {
                try
                {
                    var httpClient = httpClientFactory.CreateClient("validate-url");
                    httpClient.BaseAddress = new Uri(url.OriginalUrl);
                    var response = await httpClient.GetAsync("", stoppingToken);

                    if (response.IsSuccessStatusCode)
                    {
                        shortUrlService.Activate(url.ShortCode);
                        logger.LogInformation("URL '{OriginalUrl}' is valid. Short code '{ShortCode}' activated.", url.OriginalUrl, url.ShortCode);
                    }
                    else
                    {
                        var description = $"URL retornou status code {response.StatusCode}.";
                        shortUrlService.Deactivate(url.ShortCode, description);
                        logger.LogWarning("URL '{OriginalUrl}' is invalid (status code {StatusCode}). Short code '{ShortCode}' marked as invalid.", url.OriginalUrl, response.StatusCode, url.ShortCode);
                    }
                }
                catch (Exception ex)
                {
                    var description = $"Error ao validar URL: {ex.Message}";
                    shortUrlService.Invalidate(url.ShortCode, description);
                    logger.LogError(ex, "Error validating URL '{OriginalUrl}'. Short code '{ShortCode}' marked as invalid.", url.OriginalUrl, url.ShortCode);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}