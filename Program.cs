using IvyQrCodeProfileSharing.Apps;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
// Custom configuration
var server = new Server();
#if DEBUG
server.UseHotReload();
#endif
server.AddAppsFromAssembly();

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
