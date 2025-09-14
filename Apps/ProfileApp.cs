using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

[App(icon: Icons.User, title: "Profile Creator")]
public class ProfileApp : ViewBase
{
    // Profile model with basic fields for sharing
    public record ProfileModel(
        string FirstName,
        string LastName,
        string Email,
        string? Phone,
        string? LinkedIn,
        string? GitHub
    );

    public override object? Build()
    {
        var profile = UseState(() => new ProfileModel("", "", "", null, null, null));
        var qrCodeService = new QrCodeService();
        var qrCodeBase64 = UseState<string>("");
        var profileSubmitted = UseState<bool>(false);
        
        var formBuilder = profile.ToForm()
            .Required(m => m.FirstName, m => m.LastName, m => m.Email)
            .Label(m => m.FirstName, "First Name")
            .Label(m => m.LastName, "Last Name")
            .Label(m => m.Email, "Email Address")
            .Label(m => m.Phone, "Phone Number")
            .Label(m => m.LinkedIn, "LinkedIn Profile")
            .Label(m => m.GitHub, "GitHub Profile")
            .Description(m => m.Email, "We'll use this to contact you")
            .Description(m => m.Phone, "Optional - for direct contact")
            .Description(m => m.LinkedIn, "Optional - your professional profile")
            .Description(m => m.GitHub, "Optional - your code repositories")
            .Validate<string>(m => m.Email, email =>
                (email.Contains("@") && email.Contains("."), "Please enter a valid email address"))
            .Validate<string>(m => m.LinkedIn, linkedin =>
                (string.IsNullOrEmpty(linkedin) || linkedin.Contains("linkedin.com"), "Please enter a valid LinkedIn URL"))
            .Validate<string>(m => m.GitHub, github =>
                (string.IsNullOrEmpty(github) || github.Contains("github.com"), "Please enter a valid GitHub URL"));

        var (onSubmit, formView, validationView, loading) = formBuilder.UseForm(this.Context);

        async void HandleSubmit()
        {
            if (await onSubmit())
            {
                try
                {
                    // Generate vCard QR code for contact sharing
                    qrCodeBase64.Value = qrCodeService.GenerateVCardQrCodeAsBase64(
                        profile.Value.FirstName,
                        profile.Value.LastName,
                        profile.Value.Email,
                        profile.Value.Phone,
                        profile.Value.LinkedIn,
                        profile.Value.GitHub,
                        8
                    );
                    profileSubmitted.Value = true;
                }
                catch (Exception ex)
                {
                    // Handle QR code generation error
                    Console.WriteLine($"Error generating QR code: {ex.Message}");
                }
            }
        }

        return Layout.Center()
            | new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | new IvyLogo()
                | Text.H2("Create Your Profile")
                | Text.Block("Fill in your information to create a shareable profile")
                | new Separator()
                | formView
                | Layout.Horizontal()
                    | new Button("Create Profile").HandleClick(new Action(HandleSubmit).ToEventHandler<Button>())
                        .Loading(loading).Disabled(loading)
                    | validationView
                | (profile.Value.FirstName != "" && profile.Value.LastName != "" && profile.Value.Email != "" ?
                    new Card(
                        Layout.Vertical()
                        | Text.H3("Profile Preview")
                        | Text.Block($"Name: {profile.Value.FirstName} {profile.Value.LastName}")
                        | Text.Block($"Email: {profile.Value.Email}")
                        | (profile.Value.Phone != null ? Text.Block($"Phone: {profile.Value.Phone}") : null)
                        | (profile.Value.LinkedIn != null ? Text.Block($"LinkedIn: {profile.Value.LinkedIn}") : null)
                        | (profile.Value.GitHub != null ? Text.Block($"GitHub: {profile.Value.GitHub}") : null)
                    ).Title("Preview")
                    : null)
                | (profileSubmitted.Value && !string.IsNullOrEmpty(qrCodeBase64.Value) ?
                    new Card(
                        Layout.Vertical().Gap(6)
                        | Text.H3("Your QR Code")
                        | Text.Block("Scan this QR code with your phone to automatically add this contact:")
                        | Text.Html($"<img src=\"data:image/png;base64,{qrCodeBase64.Value}\" width=\"250\" height=\"250\" style=\"display: block; margin: 0 auto; border: 1px solid #ddd; border-radius: 8px;\" />")
                        | new Button("Generate New QR Code").HandleClick(new Action(() => {
                            qrCodeBase64.Value = "";
                            profileSubmitted.Value = false;
                        }).ToEventHandler<Button>())
                    ).Title("QR Code")
                    : null)
            )
            .Width(Size.Units(120).Max(600))
            .Title("Profile Creator");
    }
}
