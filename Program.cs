using IvyQrCodeProfileSharing.Apps;
using IvyQrCodeProfileSharing.Data;
using IvyQrCodeProfileSharing.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets("17bb6ff1-6c17-4fc0-b294-5427dfea29f3")
    .AddEnvironmentVariables()
    .Build();

// Custom configuration
var server = new Server();

// Configure services
server.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

server.Services.AddScoped<IProfileRepository, ProfileRepository>();

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

// Ensure database is created
var serviceProvider = server.Services.BuildServiceProvider();
using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

await server.RunAsync();
