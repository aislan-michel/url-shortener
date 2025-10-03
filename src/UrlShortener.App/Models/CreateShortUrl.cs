using System.ComponentModel.DataAnnotations;

namespace UrlShortener.App.Models;

public sealed class CreateShortUrl
{
    [MaxLength(8, ErrorMessage = "Short code must have a maximum of 8 characters.")]
    public string? ShortCode { get; set; } = null;

    [Required(AllowEmptyStrings = false, ErrorMessage = "The Long URL field is required.")]
    public string Url { get; set; } = string.Empty;
    
    public DateOnly? Expires { get; set; } = null;
}