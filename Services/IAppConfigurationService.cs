namespace IvyQrCodeProfileSharing.Services;

public interface IAppConfigurationService
{
    string AppName { get; }
    string Version { get; }
    int MaxProfiles { get; }
    int QrCodeSize { get; }
    bool EnableEmailValidation { get; }
    bool EnableSocialProfiles { get; }
    bool EnableSearch { get; }
    string GetConnectionString(string name = "DefaultConnection");
}
