namespace UrlShortener.App.Infrastructure.Services;

public interface IQrCodeService
{
    byte[] Generate(string url);
}