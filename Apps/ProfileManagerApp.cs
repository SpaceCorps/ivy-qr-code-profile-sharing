using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

[App(icon: Icons.List, title: "Profile Manager")]
public class ProfileManagerApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new ProfileManagerRootView(), "Profile Manager");
    }
}

public class ProfileManagerRootView : ViewBase
{
    public override object? Build()
    {
        var profiles = UseState(() => new List<Profile>());
        var client = UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();

        // Load profiles on startup
        UseEffect(() =>
        {
            LoadProfiles();
        }, []);

        void LoadProfiles()
        {
            try
            {
                profiles.Value = ProfileStorage.GetAll();
            }
            catch (Exception ex)
            {
                client.Toast($"Error loading profiles: {ex.Message}");
            }
        }

        // Sidebar menu with all profile names using ListItems for blade navigation
        var sidebarMenu = Layout.Vertical().Gap(2).Padding(2)
            | Text.H4("All Profiles")
            | (profiles.Value?.Any() == true ?
                new List(profiles.Value.Select(profile =>
                    new ListItem(profile.FullName, onClick: _ =>
                    {
                        blades.Push(this, new ProfileDetailBlade(profile), profile.FullName);
                    })
                ).ToArray())
                :
                Text.Small("No profiles created yet")
            );

        return new SidebarLayout(
            mainContent: new ProfileListBlade(),
            sidebarContent: sidebarMenu,
            sidebarHeader: Layout.Vertical().Gap(2)
                | Text.Lead("Profile Manager")
        );
    }
}