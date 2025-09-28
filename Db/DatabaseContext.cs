using IvyQrCodeProfileSharing.Models;
using Microsoft.EntityFrameworkCore;

namespace IvyQrCodeProfileSharing.Db;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<Profile> Profiles { get; init; }
}