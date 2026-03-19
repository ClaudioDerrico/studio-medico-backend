using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data;

public enum UserRole
{
    Admin,
    Doctor,
    Secretary
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");
            b.HasKey(x => x.Id);

            b.HasIndex(x => x.Email).IsUnique();

            b.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.PasswordHash)
                .IsRequired();

            b.Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
        });
    }
}

