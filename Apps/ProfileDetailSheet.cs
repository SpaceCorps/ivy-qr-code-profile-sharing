using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

public class ProfileDetailSheet : ViewBase
{
    private readonly Profile _profile;

    public ProfileDetailSheet(Profile profile)
    {
        _profile = profile;
    }

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var profileStorage = UseService<IProfileStorage>(); // Use dependency injection
        var qrCodeBase64 = UseState(() => "");
        var loading = UseState(() => false);

        // Generate QR code when sheet loads
        UseEffect(() =>
        {
            GenerateQrCode();
        }, []);

        void GenerateQrCode()
        {
            loading.Value = true;
            try
            {
                var qrCodeService = new QrCodeService();
                qrCodeBase64.Value = qrCodeService.GenerateVCardQrCodeAsBase64(
                    _profile.FirstName,
                    _profile.LastName,
                    _profile.Email,
                    _profile.Phone,
                    _profile.LinkedIn,
                    _profile.GitHub
                );
            }
            finally
            {
                loading.Value = false;
            }
        }

        void DeleteProfile()
        {
            loading.Value = true;
            try
            {
                var success = profileStorage.Delete(_profile.Id);
                if (success)
                {
                    client.Toast("Profile deleted successfully!");
                }
                else
                {
                    client.Toast("Failed to delete profile.");
                }
            }
            catch (Exception ex)
            {
                client.Toast($"Error deleting profile: {ex.Message}");
            }
            finally
            {
                loading.Value = false;
            }
        }

        void HandleProfileUpdate(Profile updatedProfile)
        {
            try
            {
                profileStorage.Update(updatedProfile);
                client.Toast("Profile updated successfully!");
                
                // Regenerate QR code with updated profile
                GenerateQrCode();
            }
            catch (Exception ex)
            {
                client.Toast($"Error updating profile: {ex.Message}");
            }
        }

        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                | new Button("Edit").Variant(ButtonVariant.Primary)
                    .WithSheet(
                        () => new EditProfileSheet(
                            _profile,
                            HandleProfileUpdate,
                            () => client.Toast("Edit cancelled")
                        ),
                        title: "Edit Profile",
                        description: "Update profile information",
                        width: Size.Fraction(1 / 2f)
                    )
                | new Button("Delete").Variant(ButtonVariant.Destructive)
                    .HandleClick(new Action(DeleteProfile).ToEventHandler<Button>())
                    .Loading(loading.Value)
                | new Button("Close").Variant(ButtonVariant.Outline)
                    .HandleClick(new Action(() => { }).ToEventHandler<Button>()),
            new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | Text.H3($"Profile Details - {_profile.FullName}")
                | new Card(
                    Layout.Vertical().Gap(2).Padding(2)
                    | Text.H4("Profile Information")
                    | Layout.Vertical().Gap(2)
                        | Layout.Horizontal().Gap(4)
                            | Layout.Vertical().Gap(1)
                                | Text.Label("First Name")
                                | Text.P(_profile.FirstName)
                            | Layout.Vertical().Gap(1)
                                | Text.Label("Last Name")
                                | Text.P(_profile.LastName)
                        | Layout.Horizontal().Gap(4)
                            | Layout.Vertical().Gap(1)
                                | Text.Label("Email")
                                | Text.P(_profile.Email)
                            | Layout.Vertical().Gap(1)
                                | Text.Label("Phone")
                                | Text.P(_profile.Phone ?? "Not provided")
                        | Layout.Horizontal().Gap(4)
                            | Layout.Vertical().Gap(1)
                                | Text.Label("LinkedIn")
                                | Text.P(_profile.LinkedIn ?? "Not provided")
                            | Layout.Vertical().Gap(1)
                                | Text.Label("GitHub")
                                | Text.P(_profile.GitHub ?? "Not provided")
                        | Layout.Horizontal().Gap(4)
                            | Layout.Vertical().Gap(1)
                                | Text.Label("Created At")
                                | Text.P(_profile.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"))
                            | Layout.Vertical().Gap(1)
                                | Text.Label("Updated At")
                                | Text.P(_profile.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"))
                )
                | (loading.Value ?
                    Text.Block("Generating QR Code...") :
                    !string.IsNullOrEmpty(qrCodeBase64.Value) ?
                        new Card(
                            Layout.Vertical().Gap(4).Padding(2).Align(Align.Center)
                            | Text.H4("QR Code")
                            | new DemoBox(
                                Text.Html($"<img src=\"data:image/png;base64,{qrCodeBase64.Value}\" />")
                            ).BorderStyle(BorderStyle.None).Width(Size.Units(120)).Height(Size.Units(120))
                        ) : null)
            ).Title("Profile Information")
        );
    }
}
