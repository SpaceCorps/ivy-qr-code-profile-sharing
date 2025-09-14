using QRCoder;

namespace IvyQrCodeProfileSharing.Services;

public class QrCodeService : IQrCodeService
{
    public string GenerateQrCodeAsBase64(string text, int pixelsPerModule = 8)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
        return Convert.ToBase64String(qrCodeBytes);
    }
}
