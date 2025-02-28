using ChatApi.Domain.Entities;

namespace ChatApi.Infrastructure.Services;

public interface IUserService
{
    Task<bool> RegisterAsync(UserRegister input);
    Task<string> LoginAsync(UserLogin input);
}
