using IvyQrCodeProfileSharing.Apps;
using IvyQrCodeProfileSharing.Db;
using Microsoft.EntityFrameworkCore;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var server = new Server();

#if DEBUG
server.UseHotReload();
#endif

server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

server.Services.AddDbContext<DatabaseContext>();

server.UseBuilder(builder =>
{
    builder.Configuration.AddJsonFile("appsettings.json");
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddUserSecrets<Program>();
    builder.Services.AddDbContext<DatabaseContext>(
        options => {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")
            );
        }
    );
});

var chromeSettings = new ChromeSettings().DefaultApp<ProfileApp>().UseTabs(preventDuplicates: true);

server.UseChrome(chromeSettings);

await server.RunAsync();
