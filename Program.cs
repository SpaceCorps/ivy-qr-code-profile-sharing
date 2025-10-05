using IvyQrCodeProfileSharing.Apps;
using IvyQrCodeProfileSharing.Db;
using IvyQrCodeProfileSharing.Repositories;
using IvyQrCodeProfileSharing.Services;
using Microsoft.EntityFrameworkCore;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

var server = new Server();

#if DEBUG
server.UseHotReload();
#endif

server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

// Build configuration first
var configBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets("17bb6ff1-6c17-4fc0-b294-5427dfea29f3");

var configuration = configBuilder.Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");

// Register services directly on the Ivy server with the connection string
server.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(connectionString));
server.Services.AddScoped<IProfileRepository, ProfileRepository>();
server.Services.AddScoped<IQrCodeService, QrCodeService>();

server.UseBuilder(builder =>
{
    builder.Configuration.AddJsonFile("appsettings.json");
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddUserSecrets("17bb6ff1-6c17-4fc0-b294-5427dfea29f3");
});

var chromeSettings = new ChromeSettings().DefaultApp<ProfileApp>().UseTabs(preventDuplicates: true);

server.UseChrome(chromeSettings);

await server.RunAsync();
