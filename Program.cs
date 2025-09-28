using IvyQrCodeProfileSharing.Apps;
using IvyQrCodeProfileSharing.Data;
using IvyQrCodeProfileSharing.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

// Custom configuration
var server = new Server();

// Configure services
server.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=179.61.190.43,1433;Database=IvyQrCodeProfileSharing;User Id=sa;Password=Password_2_Change_4_RealCases%26;TrustServerCertificate=true;"));

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
