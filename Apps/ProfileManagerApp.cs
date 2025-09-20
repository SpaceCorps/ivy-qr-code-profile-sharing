using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

[App(icon: Icons.List, title: "Profile Manager")]
public class ProfileManagerApp : ViewBase
{
    public override object? Build()
    {
        var profiles = UseState(() => new List<Profile>());
        var searchTerm = UseState(() => "");
        var selectedProfile = UseState(() => (Profile?)null);
        var qrCodeBase64 = UseState(() => "");
        var loading = UseState(() => false);
        var selectedPage = UseState("qrcodes-list");
        var client = UseService<IClientProvider>();

        // Load profiles on startup
        UseEffect(() =>
        {
            LoadProfiles();
        }, []);

        void LoadProfiles()
        {
            loading.Value = true;
            try
            {
                profiles.Value = ProfileStorage.GetAll();
            }
            finally
            {
                loading.Value = false;
            }
        }

        void SearchProfiles()
        {
            loading.Value = true;
            try
            {
                profiles.Value = ProfileStorage.Search(searchTerm.Value);
            }
            finally
            {
                loading.Value = false;
            }
        }

        void GenerateQrCode(Profile profile)
        {
            loading.Value = true;
            try
            {
                var qrCodeService = new QrCodeService();
                qrCodeBase64.Value = qrCodeService.GenerateVCardQrCodeAsBase64(
                    profile.FirstName,
                    profile.LastName,
                    profile.Email,
                    profile.Phone,
                    profile.LinkedIn,
                    profile.GitHub
                );
                selectedProfile.Value = profile;
            }
            finally
            {
                loading.Value = false;
            }
        }

        void DeleteProfile(Profile profile)
        {
            loading.Value = true;
            try
            {
                var success = ProfileStorage.Delete(profile.Id);
                if (success)
                {
                    LoadProfiles(); // Refresh the list
                    if (selectedProfile.Value?.Id == profile.Id)
                    {
                        selectedProfile.Value = null;
                        qrCodeBase64.Value = "";
                    }
                }
            }
            finally
            {
                loading.Value = false;
            }
        }
        void SelectProfile(Profile profile)
        {
            selectedProfile.Value = profile;
            selectedPage.Value = "profile-details";
            GenerateQrCode(profile);
        }

        void HandleProfileUpdate(Profile updatedProfile)
        {
            try
            {
                ProfileStorage.Update(updatedProfile);
                
                // Update the selected profile and refresh the list
                selectedProfile.Value = updatedProfile;
                LoadProfiles();
                
                // Regenerate QR code
                GenerateQrCode(updatedProfile);
            }
            catch (Exception ex)
            {
                client.Toast($"Error updating profile: {ex.Message}");
            }
        }

        string GenerateQrCodeForProfile(Profile profile)
        {
            try
            {
                var qrCodeService = new QrCodeService();
                return qrCodeService.GenerateVCardQrCodeAsBase64(
                    profile.FirstName,
                    profile.LastName,
                    profile.Email,
                    profile.Phone,
                    profile.LinkedIn,
                    profile.GitHub
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating QR code for {profile.FullName}: {ex.Message}");
                return ""; // Return empty string on error
            }
        }

        object RenderContent()
        {
            return selectedPage.Value switch
            {
                "qrcodes-list" => Layout.Vertical().Gap(6).Padding(2)
                    | Text.H3("All QR Codes")
                    | (profiles.Value.Any() ?
                        Layout.Grid().Columns(3).Gap(4)
                        | profiles.Value.Select(profile =>
                            new Card(
                                 Layout.Vertical().Gap(2).Padding(2)
                                    | Text.H4(profile.FullName)
                                    | Layout.Horizontal().Align(Align.Center)
                                    | new DemoBox(
                                    Text.Html($"<img src=\"data:image/png;base64,{GenerateQrCodeForProfile(profile)}\" />")
                                ).BorderStyle(BorderStyle.None).Width(Size.Units(80)).Height(Size.Units(80))
                                    | new Button("View").Variant(ButtonVariant.Secondary)
                                        .HandleClick(new Action(() => SelectProfile(profile)).ToEventHandler<Button>())
                            ).Width(Size.Full())
                        ).ToArray()
                        :
                        Text.Block("No profiles found. Create some profiles first.")
                    ),
                "profile-details" => selectedProfile.Value != null ?
                    new Card(
                        Layout.Vertical().Gap(6).Padding(2)
                        | Text.H3($"Profile Details - {selectedProfile.Value.FullName}")
                        | Text.Label($"First Name: {selectedProfile.Value.FirstName}")
                        | Text.Label($"Last Name: {selectedProfile.Value.LastName}")
                        | Text.Label($"Email: {selectedProfile.Value.Email}")
                        | Text.Label($"Phone: {selectedProfile.Value.Phone ?? "Not provided"}")
                        | Text.Label($"LinkedIn: {selectedProfile.Value.LinkedIn ?? "Not provided"}")
                        | Text.Label($"GitHub: {selectedProfile.Value.GitHub ?? "Not provided"}")
                        | Text.Label($"Created: {selectedProfile.Value.CreatedAt:yyyy-MM-dd HH:mm}")
                        | Text.Label($"Updated: {selectedProfile.Value.UpdatedAt:yyyy-MM-dd HH:mm}")
                        | (loading.Value ?
                            Text.Block("Generating QR Code...") :
                            !string.IsNullOrEmpty(qrCodeBase64.Value) ?
                                Layout.Vertical().Gap(4)
                                | Text.H4("QR Code")
                                | Layout.Horizontal().Align(Align.Center)
                                | new DemoBox(
                                    Text.Html($"<img src=\"data:image/png;base64,{qrCodeBase64.Value}\" />")
                                ).BorderStyle(BorderStyle.None).Width(Size.Units(70)).Height(Size.Units(70))
                                : null)
                        | (Layout.Horizontal().Gap(2)
                        | new Button("Back to List").HandleClick(new Action(() => selectedPage.Value = "qrcodes-list").ToEventHandler<Button>())
                        
                             | new Button("Edit").Variant(ButtonVariant.Primary)
                                 .WithSheet(
                                     () => new EditProfileSheet(
                                         selectedProfile.Value,
                                         HandleProfileUpdate,
                                         () => client.Toast("Edit cancelled")
                                     ),
                                     title: "Edit Profile",
                                     description: "Update profile information",
                                     width: Size.Fraction(1/2f)
                                 )
                            | new Button("Delete").Variant(ButtonVariant.Destructive)
                                .HandleClick(new Action(() => DeleteProfile(selectedProfile.Value)).ToEventHandler<Button>()))
                    ).Height(Size.Full())
                    :
                    Layout.Vertical().Gap(4).Padding(2)
                    | Text.H3("No profile selected")
                    | Text.Block("Please select a profile from the sidebar."),
                "qrcodes" => selectedProfile.Value != null && !string.IsNullOrEmpty(qrCodeBase64.Value) ?
                    Layout.Vertical().Gap(4).Padding(2)
                    | Text.H3($"QR Code for {selectedProfile.Value.FullName}")
                    | new DemoBox(
                        Text.Html($"<img src=\"data:image/png;base64,{qrCodeBase64.Value}\" />")
                    ).BorderStyle(BorderStyle.None).Width(Size.Units(100)).Height(Size.Units(100))
                    | new Button("Back to List").HandleClick(new Action(() => selectedPage.Value = "profiles").ToEventHandler<Button>())
                    :
                    Layout.Vertical().Gap(4).Padding(2)
                    | Layout.Center()
                    | Text.H3("Select a profile to view QR code")
                    | Text.Block("Click 'QR Code' on any profile to generate and display its QR code."),
                _ => Layout.Vertical().Gap(4).Padding(2)
                    | Text.H3("Page not found")
                    | Text.Block("The requested page could not be found.")
            };
        }

        // Sidebar menu with all profile names
        var sidebarMenu = Layout.Vertical().Gap(2).Padding(2)
            | Text.H4("All Profiles")
            | (profiles.Value.Any() ?
                Layout.Vertical().Gap(1)
                | profiles.Value.Select(profile =>
                    new Button(profile.FullName)
                        .Variant(selectedProfile.Value?.Id == profile.Id ? ButtonVariant.Primary : ButtonVariant.Ghost)
                        .HandleClick(new Action(() => SelectProfile(profile)).ToEventHandler<Button>())
                        .Width(Size.Full())
                ).ToArray()
                :
                Text.Small("No profiles created yet")
            );

        return new SidebarLayout(
            mainContent: RenderContent(),
            sidebarContent: sidebarMenu,
            sidebarHeader: Layout.Vertical().Gap(2)
                | Text.Lead("Profile Manager")
                | new TextInput(placeholder: "Search...", variant: TextInputs.Search)
        );
    }
}
