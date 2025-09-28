using IvyQrCodeProfileSharing.Models;

namespace IvyQrCodeProfileSharing.Repositories;

public interface IProfileRepository
{
    Task<List<Profile>> GetAllAsync();
    Task<Profile?> GetByIdAsync(int id);
    Task<Profile?> GetByEmailAsync(string email);
    Task<Profile> CreateAsync(Profile profile);
    Task<Profile> UpdateAsync(Profile profile);
    Task<bool> DeleteAsync(int id);
    Task<List<Profile>> SearchAsync(string searchTerm);
}
