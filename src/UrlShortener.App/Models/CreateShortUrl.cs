using System.ComponentModel.DataAnnotations;

namespace UrlShortener.App.Models;

public sealed class CreateShortUrl
{
    [MaxLength(10, ErrorMessage = "Short code must have a maximum of 10 characters.")]
    public string? ShortCode { get; set; } = null;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Long URL field is required.")]
    public string Url { get; set; } = string.Empty;
    
    public DateOnly? Expires { get; set; } = null;
}