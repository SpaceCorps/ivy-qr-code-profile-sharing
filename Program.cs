using IvyQrCodeProfileSharing.Apps;
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
#if DEBUG
server.UseHotReload();
#endif
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
