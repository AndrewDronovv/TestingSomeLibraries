using ChatApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Domain;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
