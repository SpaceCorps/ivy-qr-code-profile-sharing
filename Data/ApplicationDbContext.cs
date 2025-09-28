using Microsoft.EntityFrameworkCore;
using IvyQrCodeProfileSharing.Models;

namespace IvyQrCodeProfileSharing.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Profile entity
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
                
            entity.Property(e => e.Phone)
                .HasMaxLength(20);
                
            entity.Property(e => e.LinkedIn)
                .HasMaxLength(255);
                
            entity.Property(e => e.GitHub)
                .HasMaxLength(255);
                
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Create index on email for faster lookups
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Profiles_Email");
        });
    }
}
