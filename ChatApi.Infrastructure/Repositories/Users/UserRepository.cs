using ChatApi.Domain.Entities;
using ChatApi.Infrastructure;
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

    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetRefreshTokensByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
            .ToListAsync();
    }


    public async Task RevokeAllUserRefreshTokensAsync(int userId)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
            .ToListAsync();

        if (refreshTokens.Any())
        {
            foreach (var token in refreshTokens)
            {
                token.Revoked = DateTime.UtcNow;
            }
        }

        _context.RefreshTokens.UpdateRange(refreshTokens);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRefreshTokensAsync(IEnumerable<RefreshToken> refreshTokens)
    {
        _context.RefreshTokens.RemoveRange(refreshTokens);
        await _context.SaveChangesAsync();
    }
}