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
            .Place(m => m.FirstName)                    // First column
            .Place(1, m => m.Email)                     // Second column, same row
            .Place(m => m.LastName)                     // First column, next row
            .Place(1, m => m.LinkedIn)                  // Second column, same row
            .Place(m => m.Phone)                        // First column, next row
            .Place(1, m => m.GitHub)                    // Second column, same row
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

        // Sidebar content - Profile form
        var sidebarContent = new Card(
            Layout.Vertical().Gap(6).Padding(2)
            | Text.H2("Create Your Profile")
            | Text.Block("Fill in your information to create a shareable profile")
            | new Separator()
            | formView
            | Layout.Horizontal()
                | new Button("Create Profile").HandleClick(new Action(HandleSubmit).ToEventHandler<Button>())
                    .Loading(loading).Disabled(loading)
                | validationView
        ).Height(Size.Units(150));

        // Main content - QR Code display
        var mainContent = profileSubmitted.Value && !string.IsNullOrEmpty(qrCodeBase64.Value) ?
            new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | Text.H2("Your QR Code")
                | Text.Block("Scan this QR code with your phone to automatically add this contact to your contacts:")
                | Layout.Center()
                    | Text.Html($"<img src=\"data:image/png;base64,{qrCodeBase64.Value}\" width=\"100\" height=\"100\" style=\"display: block; margin: 0 auto; border: 2px solid #ddd; border-radius: 12px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);\" />")
                | (Layout.Horizontal().Align(Align.Center)
                    | new Button("Generate New QR Code").HandleClick(new Action(() =>
                    {
                        qrCodeBase64.Value = "";
                        profileSubmitted.Value = false;
                    }).ToEventHandler<Button>()))

            ).Height(Size.Units(150))
            : new Card(
                Layout.Vertical().Gap(6).Padding(2)
                | Layout.Center()
                    | Text.H2("Welcome to Profile Creator")
                | Text.Block("Fill out the form in the sidebar to create your shareable profile QR code.")
                | Text.Block("Once you submit the form, your QR code will appear here in the main content area.")
                | Layout.Center()
                    | Text.Html("<div style='font-size: 4rem; opacity: 0.3;'>📱</div>")
            ).Height(Size.Units(150));

        return Layout.Vertical()
            | new ResizeablePanelGroup(
                new ResizeablePanel(70, sidebarContent), // Form panel - 40% width, resizable
                new ResizeablePanel(30, mainContent)     // QR Code panel - 60% width, resizable
            ).Horizontal();

    }
}
