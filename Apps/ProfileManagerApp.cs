using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;
using IvyQrCodeProfileSharing.Repositories;

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
        var filteredProfiles = UseState(() => new List<Profile>());
        var searchTerm = UseState(() => "");
        var client = UseService<IClientProvider>();
        var profileRepository = UseService<IProfileRepository>();
        var blades = this.UseContext<IBladeController>();

        // Load profiles on startup and whenever component mounts
        UseEffect(() =>
        {
            LoadProfiles();
        }, []);

        // Note: Profiles will be reloaded when component re-renders

        // Filter profiles when search term changes
        UseEffect(() =>
        {
            FilterProfiles();
        }, [searchTerm, profiles]);

        async void LoadProfiles()
        {
            try
            {
                profiles.Value = await profileRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                client.Toast($"Error loading profiles: {ex.Message}");
            }
        }

        void FilterProfiles()
        {
            if (string.IsNullOrWhiteSpace(searchTerm.Value))
            {
                filteredProfiles.Value = profiles.Value;
            }
            else
            {
                var searchLower = searchTerm.Value.ToLowerInvariant();
                filteredProfiles.Value = profiles.Value.Where(p =>
                    p.FirstName.ToLowerInvariant().Contains(searchLower) ||
                    p.LastName.ToLowerInvariant().Contains(searchLower) ||
                    p.FullName.ToLowerInvariant().Contains(searchLower) ||
                    p.Email.ToLowerInvariant().Contains(searchLower) ||
                    (p.Phone?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.LinkedIn?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (p.GitHub?.ToLowerInvariant().Contains(searchLower) ?? false)
                ).ToList();
            }
        }


        // Sidebar menu with search and filtered profile names
        var sidebarMenu = Layout.Vertical().Gap(2).Padding(2)
            | Text.H4("All Profiles")
            | searchTerm.ToTextInput()
                .Placeholder("Search profiles...")
                .Variant(TextInputs.Search)
            | (filteredProfiles.Value?.Any() == true ?
                Layout.Vertical().Gap(1)
                | filteredProfiles.Value.Select(profile =>
                    new Button(profile.FullName).Variant(ButtonVariant.Outline)
                        .WithSheet(
                            () => new ProfileDetailSheet(profile),
                            title: profile.FullName,
                            description: "View profile details and QR code",
                            width: Size.Fraction(2 / 3f)
                        )
                ).ToArray()
                :
                searchTerm.Value?.Length > 0 ?
                    Text.Small($"No profiles found matching '{searchTerm.Value}'") :
                    Text.Small("No profiles created yet")
            );

        return new SidebarLayout(
            mainContent: new ProfileListBlade(filteredProfiles.Value),
            sidebarContent: sidebarMenu,
            sidebarHeader: Layout.Vertical().Gap(2)
        );
    }
}