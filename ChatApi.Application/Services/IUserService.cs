using ChatApi.Domain.Entities;
using static ChatApi.Infrastructure.Services.UserService;

namespace ChatApi.Infrastructure.Services;

public interface IUserService
{
    Task<bool> RegisterAsync(UserRegister input);
    Task<Response> LoginAsync(UserLogin input);
    Task<Response> RefreshTokenAsync(string token);
    Task RevokeAllUserRefreshTokensAsync(int userId);
}
