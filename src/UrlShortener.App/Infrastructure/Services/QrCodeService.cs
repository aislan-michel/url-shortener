using QRCoder;

namespace UrlShortener.App.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] Generate(string url)
    {
        byte[] qrCodeImage;

        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            qrCodeImage = qrCode.GetGraphic(11);
        }

        return qrCodeImage;
    }
}