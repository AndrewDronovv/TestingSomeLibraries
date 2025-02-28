using ChatApi.Application.Repositories.Users;
using ChatApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    public async Task<string> LoginAsync(UserLogin input)
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

        var tokenHandler = new JwtSecurityTokenHandler();
        var stringToken = tokenHandler.WriteToken(token);

        return stringToken;
    }
}
