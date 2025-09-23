using IvyQrCodeProfileSharing.Models;
using IvyQrCodeProfileSharing.Services;

namespace IvyQrCodeProfileSharing.Apps;

public class EditProfileSheet : ViewBase
{
    private readonly Profile _profile;
    private readonly Action<Profile> _onSave;
    private readonly Action _onCancel;

    public EditProfileSheet(Profile profile, Action<Profile> onSave, Action onCancel)
    {
        _profile = profile;
        _onSave = onSave;
        _onCancel = onCancel;
    }

    public override object? Build()
    {
        var client = UseService<IClientProvider>();
        var profileStorage = UseService<IProfileStorage>(); // Use dependency injection
        
        // Create a copy of the profile for editing
        var editProfile = UseState(() => new Profile
        {
            Id = _profile.Id,
            FirstName = _profile.FirstName,
            LastName = _profile.LastName,
            Email = _profile.Email,
            Phone = _profile.Phone,
            LinkedIn = _profile.LinkedIn,
            GitHub = _profile.GitHub,
            CreatedAt = _profile.CreatedAt,
            UpdatedAt = _profile.UpdatedAt
        });

        var formBuilder = editProfile.ToForm()
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

        async void HandleSave()
        {
            if (await onSubmit())
            {
                try
                {
                    // Check if email is being changed and if the new email already exists
                    if (_profile.Email != editProfile.Value.Email)
                    {
                        var existingProfile = profileStorage.GetByEmail(editProfile.Value.Email);
                        if (existingProfile != null)
                        {
                            client.Toast("A profile with this email already exists!");
                            return;
                        }
                    }

                    // Create updated profile
                    var updatedProfile = new Profile
                    {
                        Id = _profile.Id,
                        FirstName = editProfile.Value.FirstName,
                        LastName = editProfile.Value.LastName,
                        Email = editProfile.Value.Email,
                        Phone = editProfile.Value.Phone,
                        LinkedIn = editProfile.Value.LinkedIn,
                        GitHub = editProfile.Value.GitHub,
                        CreatedAt = _profile.CreatedAt,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _onSave(updatedProfile);
                    client.Toast("Profile updated successfully!");
                }
                catch (Exception ex)
                {
                    client.Toast($"Error updating profile: {ex.Message}");
                }
            }
        }

        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                | new Button("Save").Variant(ButtonVariant.Primary)
                    .HandleClick(new Action(HandleSave).ToEventHandler<Button>())
                    .Loading(loading).Disabled(loading)
                | new Button("Cancel").Variant(ButtonVariant.Outline)
                    .HandleClick(new Action(_onCancel).ToEventHandler<Button>()),
            new Card(
                Layout.Vertical().Gap(4).Padding(2)
                | Text.H3($"Edit Profile - {_profile.FullName}")
                | new Card(
                    Layout.Vertical().Gap(2).Padding(2)
                    | Text.H4("Profile Information (Read-Only)")
                    | Layout.Horizontal().Gap(4)
                        | Layout.Vertical().Gap(1)
                            | Text.Label("Full Name")
                            | Text.P(_profile.FullName)
                        | Layout.Vertical().Gap(1)
                            | Text.Label("Display Name")
                            | Text.P(_profile.DisplayName)
                    | Layout.Horizontal().Gap(4)
                        | Layout.Vertical().Gap(1)
                            | Text.Label("Created At")
                            | Text.P(_profile.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"))
                        | Layout.Vertical().Gap(1)
                            | Text.Label("Updated At")
                            | Text.P(_profile.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"))
                )
                | formView
                | validationView
            ).Title("Profile Information")
        );
    }
}
