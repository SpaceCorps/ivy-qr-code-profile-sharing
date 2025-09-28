using Microsoft.EntityFrameworkCore;
using IvyQrCodeProfileSharing.Data;
using IvyQrCodeProfileSharing.Models;

namespace IvyQrCodeProfileSharing.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly ApplicationDbContext _context;

    public ProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Profile>> GetAllAsync()
    {
        return await _context.Profiles
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Profile?> GetByIdAsync(int id)
    {
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Profile?> GetByEmailAsync(string email)
    {
        return await _context.Profiles
            .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());
    }

    public async Task<Profile> CreateAsync(Profile profile)
    {
        profile.CreatedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;
        
        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();
        
        return profile;
    }

    public async Task<Profile> UpdateAsync(Profile profile)
    {
        var existingProfile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id);
            
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

        await _context.SaveChangesAsync();
        return existingProfile;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (profile == null)
        {
            return false;
        }

        _context.Profiles.Remove(profile);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Profile>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var searchLower = searchTerm.ToLower();
        
        return await _context.Profiles
            .Where(p =>
                p.FirstName.ToLower().Contains(searchLower) ||
                p.LastName.ToLower().Contains(searchLower) ||
                p.Email.ToLower().Contains(searchLower) ||
                (p.Phone != null && p.Phone.ToLower().Contains(searchLower)) ||
                (p.LinkedIn != null && p.LinkedIn.ToLower().Contains(searchLower)) ||
                (p.GitHub != null && p.GitHub.ToLower().Contains(searchLower))
            )
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }
}
