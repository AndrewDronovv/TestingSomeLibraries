using ChatApi.Domain.Entities;

namespace ChatApi.Application.Repositories.Users;

public interface IUserRepository
{
    Task<User> GetByLoginAsync(string login);
    Task AddAsync(User user);
}
