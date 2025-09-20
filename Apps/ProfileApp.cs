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
            .Place(m => m.FirstName)
            .Place(m => m.LastName)
            .Place(m => m.Email)
            .Place(m => m.Phone)
            .Place(m => m.LinkedIn)
            .Place(m => m.GitHub)
            .Label(m => m.FirstName, "First Name")
            .Label(m => m.LastName, "Last Name")
            .Label(m => m.Email, "Email Address")
            .Label(m => m.Phone, "Phone Number")
            .Label(m => m.LinkedIn, "LinkedIn Profile")
            .Label(m => m.GitHub, "GitHub Profile")
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
        }

        // Sidebar content - Profile form
        var formContent = Layout.Vertical().Gap(6).Padding(2)
            | Text.Block("Fill in your information to create a shareable profile")
            | new Separator()
            | formView
            | Layout.Horizontal()
                | new Button("Create Profile").HandleClick(new Action(HandleSubmit).ToEventHandler<Button>())
                    .Loading(loading).Disabled(loading)
                | validationView;

        // Main content - QR Code display
        var qrCodeContent = profileSubmitted.Value && !string.IsNullOrEmpty(qrCodeBase64.Value) ?
            new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | Text.H2("Your QR Code")
                | Text.Block("Scan this QR code with your phone to automatically add this contact to your contacts:")
                | (Layout.Horizontal().Align(Align.Center)
                | new DemoBox(
                    Text.Html($"<img src=\"data:image/png;base64,{qrCodeBase64.Value}\" />")
            ).BorderStyle(BorderStyle.None).Width(Size.Units(70)).Height(Size.Units(70)))
            ).Height(Size.Full())
            : new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | (Layout.Center()
                    | Text.H2("Welcome to Profile Creator"))
                | Text.Block("Fill out the form in the sidebar to create your shareable profile QR code.")
                | Text.Block("Once you submit the form, your QR code will appear here in the main content area.")
            ).Height(Size.Full());

        return new SidebarLayout(
            mainContent: qrCodeContent,
            sidebarContent: formContent,
            sidebarHeader: Layout.Vertical().Gap(1)
                | Text.Lead("Profile Creator")
        );

    }
}
