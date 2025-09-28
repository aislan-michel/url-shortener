using System.ComponentModel.DataAnnotations;

namespace UrlShortener.App.Models;

public sealed class CreateShortUrl
{
    public string? ShortCode { get; set; } = null;

    [Required]
    public string Url { get; set; } = string.Empty;
    
    public DateOnly? Expires { get; set; } = null;
}