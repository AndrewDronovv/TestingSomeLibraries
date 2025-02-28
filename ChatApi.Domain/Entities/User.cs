using ChatApi.Domain.Common;

namespace ChatApi.Domain.Entities;

public class User : Entity
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}
