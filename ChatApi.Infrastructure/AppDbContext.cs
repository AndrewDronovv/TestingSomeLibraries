using ChatApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RefreshToken>().HasKey(r => r.Id);
        modelBuilder.Entity<RefreshToken>().Property(r => r.Token).HasMaxLength(150);
        modelBuilder.Entity<RefreshToken>().HasIndex(r => r.Token).IsUnique();
        modelBuilder.Entity<RefreshToken>().HasOne(r => r.User).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UserId);

        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<User>().Property(u => u.FirstName).HasMaxLength(200);
        modelBuilder.Entity<User>().Property(u => u.LastName).HasMaxLength(200);
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
