using ChatApi.Application.Repositories.Users;
using ChatApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChatApi.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public UserService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public sealed record Response(string AccessToken, string RefreshToken);

    public async Task<bool> RegisterAsync(UserRegister input)
    {
        var existingUser = await _userRepository.GetByLoginAsync(input.Login);

        if (existingUser != null)
        {
            return false;
        }

        var user = new User
        {
            Login = input.Login,
            Password = BCrypt.Net.BCrypt.HashPassword(input.Password)
        };

        await _userRepository.AddAsync(user);

        return true;
    }
    public async Task<Response> LoginAsync(UserLogin input)
    {
        var existingUser = await _userRepository.GetByLoginAsync(input.Login);

        if (existingUser == null || !BCrypt.Net.BCrypt.Verify(input.Password, existingUser.Password))
        {
            return null;
        }

        var jwtSettings = _configuration.GetSection("Jwt");
        var key = jwtSettings["Key"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
            new Claim(ClaimTypes.Name, existingUser.Login),
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.Now.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = GenerateRefreshToken(existingUser.Id);
        await _userRepository.SaveRefreshTokenAsync(refreshToken);

        return new Response(jwtToken, refreshToken.Token);
    }

    public async Task<Response> RefreshTokenAsync(string token)
    {
        var existingRefreshToken = await _userRepository.GetRefreshTokenAsync(token);

        if (existingRefreshToken == null || !existingRefreshToken.IsActive)
        {
            return null;
        }

        await _userRepository.RevokeAllUserRefreshTokensAsync(existingRefreshToken.UserId);

        var newJwtToken = GenerateJwtToken(existingRefreshToken.UserId);
        var newRefreshToken = GenerateRefreshToken(existingRefreshToken.UserId);

        //await _userRepository.UpdateRefreshTokenAsync(newRefreshToken);
        await _userRepository.SaveRefreshTokenAsync(newRefreshToken);

        return new Response(newJwtToken, newRefreshToken.Token);
    }

    private string GenerateJwtToken(int userId)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = jwtSettings["Key"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.Now.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken(int userId)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = userId
        };
    }

    public async Task RevokeAllUserRefreshTokensAsync(int userId)
    {
        var refreshTokens = await _userRepository.GetRefreshTokensByUserIdAsync(userId);

        await _userRepository.DeleteRefreshTokensAsync(refreshTokens);
    }
}
