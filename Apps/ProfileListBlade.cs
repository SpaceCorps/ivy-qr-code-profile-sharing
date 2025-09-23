using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

public class ProfileListBlade : ViewBase
{
    private readonly List<Profile>? _profiles;

    public ProfileListBlade(List<Profile>? profiles = null)
    {
        _profiles = profiles;
    }

    public override object? Build()
    {
        var profiles = UseState(() => _profiles ?? new List<Profile>());
        var loading = UseState(() => false);
        var client = UseService<IClientProvider>();

        // Load profiles when blade loads (only if not provided in constructor)
        UseEffect(() =>
        {
            if (_profiles == null)
            {
                LoadProfiles();
            }
        }, []);

        // Update profiles when they change from parent
        if (_profiles != null && profiles.Value != _profiles)
        {
            profiles.Value = _profiles;
        }

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

        // Profile detail will be shown via button's WithSheet method

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
                            Layout.Vertical().Gap(2).Padding(2).Align(Align.Center)
                                | Text.H4(profile.FullName)
                                | Layout.Horizontal().Align(Align.Center)
                                | new DemoBox(
                                    Text.Html($"<img src=\"data:image/png;base64,{GenerateQrCodeForProfile(profile)}\" />")
                                ).BorderStyle(BorderStyle.None).Width(Size.Units(80)).Height(Size.Units(80))
                                | new Button("View").Variant(ButtonVariant.Secondary)
                                    .WithSheet(
                                        () => new ProfileDetailSheet(profile),
                                        title: profile.FullName,
                                        description: "View profile details and QR code",
                                        width: Size.Fraction(2 / 3f)
                                    )
                        ).Width(Size.Full())
                    ).ToArray()
                    :
                    Text.Block("No profiles found. Create some profiles first.")
            );
    }
}
