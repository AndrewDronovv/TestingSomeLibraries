using ChatApi.Domain.Entities;
using ChatApi.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ChatApi.Application.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context ??
            throw new ArgumentNullException(nameof(context));
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

    public async Task<IEnumerable<RefreshToken>> GetActiveRefreshTokensByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task RevokeAllActiveRefreshTokensAsync(int userId)
    {
        var refreshTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Revoked == null && rt.Expires > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.Revoked = DateTime.UtcNow;
        }

        _context.RefreshTokens.UpdateRange(refreshTokens);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRefreshTokensAsync(IEnumerable<RefreshToken> refreshTokens)
    {
        _context.RefreshTokens.RemoveRange(refreshTokens);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetUserRefreshTokenStringsAsync(int userId)
    {
        return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .Select(rt =>
                    $"Token: {rt.Token}, " +
                    $"Created: {rt.Created.ToString("F")}, " +
                    $"Expires: {rt.Expires.ToString("F")}, " +
                    $"Revoked: {(rt.Revoked.HasValue ? rt.Revoked.Value.ToString("F")
                    : "null")}")
                .ToListAsync();
    }
}