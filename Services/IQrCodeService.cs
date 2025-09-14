namespace IvyQrCodeProfileSharing.Services;

public interface IQrCodeService
{
    string GenerateQrCodeAsBase64(string text, int pixelsPerModule = 8);
}
