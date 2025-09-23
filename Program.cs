using IvyQrCodeProfileSharing.Apps;
using IvyQrCodeProfileSharing.Services;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
// Custom configuration
var server = new Server(new ServerArgs
{
    Port = 8080,
    Verbose = false,
    Browse = true,
    Silent = true
});

server.SetMetaTitle("Ivy QR Code Profile Sharing");
server.SetMetaDescription("A powerful web application for creating and sharing QR code profiles built with Ivy framework");

#if DEBUG
server.UseHotReload();
// server.UseHttpRedirection();
server.UseContentBuilder(new CustomContentBuilder());
#endif

// Create and configure IConfiguration manually
var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

var configuration = configurationBuilder.Build();
// Configure the underlying ASP.NET Core builder
server.UseBuilder(builder =>
{
    // Use the same configuration
    builder.Configuration.AddConfiguration(configuration);
});

// Register services in the correct order
// 1. First register IConfiguration (no dependencies)
server.Services.AddSingleton<IConfiguration>(configuration);

// 2. Register other services
server.Services.AddSingleton<IQrCodeService, QrCodeService>();
server.Services.AddSingleton<IProfileStorage, ProfileStorage>();

// 3. Register AppConfigurationService (depends on IConfiguration)
server.Services.AddSingleton<IAppConfigurationService, AppConfigurationService>();

// server.AddAppsFromAssembly();

// Profile Creator App
server.AddApp(new AppDescriptor
{
    Id = "profile-creator",
    Title = "Profile Creator",
    ViewFunc = (context) => new ProfileApp(),
    Path = ["Apps", "ProfileCreator"],
    IsVisible = true,
    // Additional properties for better organization
    Description = "Create and share QR code profiles",
    Order = 1 // Display order
});

// Profile Manager App  
server.AddApp(new AppDescriptor
{
    Id = "profile-manager",
    Title = "Profile Manager", 
    ViewFunc = (context) => new ProfileManagerApp(),
    Path = ["Apps", "ProfileManager"],
    IsVisible = true,
    // Additional properties for better organization
    Description = "Manage and view all created profiles",
    Order = 2 // Display order
});

// Add connections from assembly
server.AddConnectionsFromAssembly();
// Custom chrome settings
var chromeSettings = new ChromeSettings()
    .Header(
        Layout.Vertical().Padding(2)
        | new IvyLogo()
        | Text.Muted("Version 1.0")
    )
    .DefaultApp<ProfileManagerApp>()
    .UseTabs(preventDuplicates: true);
server.UseChrome(chromeSettings);
await server.RunAsync();
