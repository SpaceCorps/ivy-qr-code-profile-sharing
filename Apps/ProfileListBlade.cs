using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

public class ProfileListBlade : ViewBase
{
    public override object? Build()
    {
        var profiles = UseState(() => new List<Profile>());
        var loading = UseState(() => false);
        var blades = this.UseContext<IBladeController>();

        // Load profiles when blade loads
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

        void SelectProfile(Profile profile)
        {
            blades.Push(this, new ProfileDetailBlade(profile), profile.FullName);
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

        return Layout.Vertical().Gap(6).Padding(2)
            | Text.H3("All QR Codes")
            | (loading.Value ?
                Text.Block("Loading profiles...") :
                (profiles.Value?.Any() == true) ?
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
            );
    }
}
