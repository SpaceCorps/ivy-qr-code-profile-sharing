using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

public class ProfileDetailBlade : ViewBase
{
    private readonly Profile _profile;

    public ProfileDetailBlade(Profile profile)
    {
        _profile = profile;
    }

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var qrCodeBase64 = UseState(() => "");
        var loading = UseState(() => false);
        var blades = this.UseContext<IBladeController>();

        // Generate QR code when blade loads
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
                var success = ProfileStorage.Delete(_profile.Id);
                if (success)
                {
                    client.Toast("Profile deleted successfully!");
                    blades.Pop(); // Go back to the list
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
                ProfileStorage.Update(updatedProfile);
                client.Toast("Profile updated successfully!");
                
                // Regenerate QR code with updated profile
                GenerateQrCode();
            }
            catch (Exception ex)
            {
                client.Toast($"Error updating profile: {ex.Message}");
            }
        }

        return new Card(
            Layout.Vertical().Gap(6).Padding(2)
            | Text.H3($"Profile Details - {_profile.FullName}")
            | Text.Label($"First Name: {_profile.FirstName}")
            | Text.Label($"Last Name: {_profile.LastName}")
            | Text.Label($"Email: {_profile.Email}")
            | Text.Label($"Phone: {_profile.Phone ?? "Not provided"}")
            | Text.Label($"LinkedIn: {_profile.LinkedIn ?? "Not provided"}")
            | Text.Label($"GitHub: {_profile.GitHub ?? "Not provided"}")
            | Text.Label($"Created: {_profile.CreatedAt:yyyy-MM-dd HH:mm}")
            | Text.Label($"Updated: {_profile.UpdatedAt:yyyy-MM-dd HH:mm}")
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
            | Layout.Horizontal().Gap(2)
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
        ).Height(Size.Full());
    }
}
