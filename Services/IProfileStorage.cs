using IvyQrCodeProfileSharing.Models;

namespace IvyQrCodeProfileSharing.Services;

public interface IProfileStorage
{
    List<Profile> GetAll();
    Profile? GetById(int id);
    Profile? GetByEmail(string email);
    Profile Create(Profile profile);
    Profile Update(Profile profile);
    bool Delete(int id);
    List<Profile> Search(string searchTerm);
}
