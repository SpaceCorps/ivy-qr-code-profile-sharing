namespace IvyQrCodeProfileSharing.Models;

public class Profile
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? LinkedIn { get; set; }
    public string? GitHub { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string FullName => $"{FirstName} {LastName}";
    public string DisplayName => $"{FirstName} {LastName} ({Email})";
}
