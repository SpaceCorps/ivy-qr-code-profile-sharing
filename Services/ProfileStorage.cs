using IvyQrCodeProfileSharing.Models;

namespace IvyQrCodeProfileSharing.Services;

public class ProfileStorage : IProfileStorage
{
    private static readonly List<Profile> _profiles = new();
    private static int _nextId = 1;
    private static readonly object _lock = new();

    public List<Profile> GetAll()
    {
        lock (_lock)
        {
            return new List<Profile>(_profiles);
        }
    }

    public Profile? GetById(int id)
    {
        lock (_lock)
        {
            return _profiles.FirstOrDefault(p => p.Id == id);
        }
    }

    public Profile? GetByEmail(string email)
    {
        lock (_lock)
        {
            return _profiles.FirstOrDefault(p => 
                string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public Profile Create(Profile profile)
    {
        lock (_lock)
        {
            profile.Id = _nextId++;
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;
            
            _profiles.Add(profile);
            return profile;
        }
    }

    public Profile Update(Profile profile)
    {
        lock (_lock)
        {
            var existingProfile = _profiles.FirstOrDefault(p => p.Id == profile.Id);
            if (existingProfile == null)
            {
                throw new ArgumentException($"Profile with ID {profile.Id} not found");
            }

            existingProfile.FirstName = profile.FirstName;
            existingProfile.LastName = profile.LastName;
            existingProfile.Email = profile.Email;
            existingProfile.Phone = profile.Phone;
            existingProfile.LinkedIn = profile.LinkedIn;
            existingProfile.GitHub = profile.GitHub;
            existingProfile.UpdatedAt = DateTime.UtcNow;

            return existingProfile;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var profile = _profiles.FirstOrDefault(p => p.Id == id);
            if (profile == null)
            {
                return false;
            }

            _profiles.Remove(profile);
            return true;
        }
    }

    public List<Profile> Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return GetAll();
        }

        lock (_lock)
        {
            return _profiles.Where(p =>
                p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                p.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (p.Phone?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.LinkedIn?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.GitHub?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();
        }
    }
}
