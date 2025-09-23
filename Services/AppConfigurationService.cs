using Microsoft.Extensions.Configuration;

namespace IvyQrCodeProfileSharing.Services;

public class AppConfigurationService : IAppConfigurationService
{
    private readonly IConfiguration _configuration;

    public AppConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string AppName => _configuration["AppSettings:AppName"] ?? "QR Code Profile Sharing";
    public string Version => _configuration["AppSettings:Version"] ?? "1.0.0";
    public int MaxProfiles => _configuration.GetValue<int>("AppSettings:MaxProfiles", 1000);
    public int QrCodeSize => _configuration.GetValue<int>("AppSettings:QrCodeSize", 8);
    public bool EnableEmailValidation => _configuration.GetValue<bool>("AppSettings:Features:EnableEmailValidation", true);
    public bool EnableSocialProfiles => _configuration.GetValue<bool>("AppSettings:Features:EnableSocialProfiles", true);
    public bool EnableSearch => _configuration.GetValue<bool>("AppSettings:Features:EnableSearch", true);
    
    public string GetConnectionString(string name = "DefaultConnection")
    {
        return _configuration.GetConnectionString(name) ?? "";
    }
}
