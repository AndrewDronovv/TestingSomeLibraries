using ChatApi.Domain;
using ChatApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApi.Application.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> GetByLoginAsync(string login)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Login == login);
    }
    public async Task AddAsync(User user)
    {
        await _context.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
