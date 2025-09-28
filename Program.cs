using IvyQrCodeProfileSharing.Apps;
using IvyQrCodeProfileSharing.Data;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var server = new Server();

#if DEBUG
server.UseHotReload();
#endif

server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

server.Services.AddDbContext<ApplicationDbContext>();

server.UseBuilder(builder =>
{
    builder.Configuration.AddJsonFile("appsettings.json");
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddUserSecrets<Program>();
    builder.Services.AddDbContext<ApplicationDbContext>();
});

var chromeSettings = new ChromeSettings().DefaultApp<ProfileApp>().UseTabs(preventDuplicates: true);

server.UseChrome(chromeSettings);

await server.RunAsync();
