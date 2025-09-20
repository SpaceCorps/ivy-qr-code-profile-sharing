using IvyQrCodeProfileSharing.Models;
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
        var qrCodeBase64 = UseState(() => "");
        var profileSubmitted = UseState(() => false);
        var createdProfile = UseState(() => (Profile?)null);

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
                try
                {
                    // Check if email already exists
                    var existingProfile = ProfileStorage.GetByEmail(profile.Value.Email);
                    if (existingProfile != null)
                    {
                        // Show error or handle duplicate email
                        return;
                    }

                    // Create profile in storage
                    var newProfile = new Profile
                    {
                        FirstName = profile.Value.FirstName,
                        LastName = profile.Value.LastName,
                        Email = profile.Value.Email,
                        Phone = profile.Value.Phone,
                        LinkedIn = profile.Value.LinkedIn,
                        GitHub = profile.Value.GitHub
                    };
                    
                    createdProfile.Value = ProfileStorage.Create(newProfile);
                    
                    // Generate QR code for the created profile
                    qrCodeBase64.Value = qrCodeService.GenerateVCardQrCodeAsBase64(
                        createdProfile.Value.FirstName,
                        createdProfile.Value.LastName,
                        createdProfile.Value.Email,
                        createdProfile.Value.Phone,
                        createdProfile.Value.LinkedIn,
                        createdProfile.Value.GitHub
                    );
                    profileSubmitted.Value = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating profile: {ex.Message}");
                }
            }
        }

        // Sidebar content - Profile form
        var formContent = Layout.Vertical().Gap(6).Padding(2)
            | formView
            | (Layout.Horizontal()
                | new Button("Create Profile").HandleClick(new Action(HandleSubmit).ToEventHandler<Button>())
                    .Loading(loading).Disabled(loading)
                | validationView
                );

        // Main content - Single card that changes content
        var qrCodeContent = new Card(
            Layout.Vertical().Gap(6).Padding(2)
            | (profileSubmitted.Value && !string.IsNullOrEmpty(qrCodeBase64.Value) && createdProfile.Value != null ?
                Layout.Vertical().Gap(6)
                | (Layout.Center()
                    | Text.H2($"QR Code for {createdProfile.Value.FullName}"))
                | (Layout.Vertical().Align(Align.Center)
                | new DemoBox(
                    Text.Html($"<img src=\"data:image/png;base64,{qrCodeBase64.Value}\" />")
                ).BorderStyle(BorderStyle.None).Width(Size.Units(70)).Height(Size.Units(70))
                | Text.Block($"Profile ID: {createdProfile.Value.Id} | Created: {createdProfile.Value.CreatedAt:yyyy-MM-dd HH:mm}"))
                | (Layout.Horizontal().Align(Align.Center)
                    | new Button("Create Another Profile").HandleClick(new Action(() =>
                    {
                        profile.Value = new ProfileModel("", "", "", null, null, null);
                        qrCodeBase64.Value = "";
                        profileSubmitted.Value = false;
                        createdProfile.Value = null;
                    }).ToEventHandler<Button>()))
                :
                Layout.Vertical().Gap(6)
                | (Layout.Center()
                    | Text.H2("Welcome to Profile Creator"))
                | Text.Block("Fill out the form in the sidebar to create your shareable profile QR code.")
                | Text.Block("Once you submit the form, your QR code will appear here in the main content area.")
            )
        ).Height(Size.Full());

        return new SidebarLayout(
            mainContent: qrCodeContent,
            sidebarContent: formContent,
            sidebarHeader: Layout.Vertical().Gap(1)
                | Text.Lead("Profile Creator")
        );

    }
}
