using Microsoft.EntityFrameworkCore;
using UserService.Enums;
using UserService.Models;

namespace UserService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureUserEntity(modelBuilder);
        
        SeedData(modelBuilder);
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            
            entity.HasKey(e => e.UserId);
            
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            
            entity.Property(e => e.Role).HasConversion<string>().IsRequired();
            
            entity.Property(e => e.MembershipStatus).HasConversion<string>().IsRequired();
            
            entity.Property(e => e.MemberSince).HasColumnType("datetime");
            
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime").IsRequired();
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess).Result;
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    
    private void ApplyAuditTimestamps()
    {
        var now =  DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;

                if (entry.Entity.MemberSince is null)
                {
                    entry.Entity.MemberSince = now;
                }
            }else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Property(u => u.CreatedAt).IsModified = false;
                entry.Property(u => u.MemberSince).IsModified = false;
            }
        }
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(
            2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(
            new User
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Email = "john@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "555-0101",
                Role = Role.Librarian,
                MembershipStatus = MembershipStatus.Active,
                MemberSince = createdAt,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            },
            new User
            {
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Email = "jane@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                FirstName = "Jane",
                LastName = "Doe",
                PhoneNumber = "555-0102",
                Role = Role.Patron,
                MembershipStatus = MembershipStatus.Active,
                MemberSince = createdAt,
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            });
        
        
    }
    
    
}