using ChatApi.Domain.Entities;
using ChatApi.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegister input)
    {
        var result = await _userService.RegisterAsync(input);

        if (!result)
        {
            return BadRequest("Пользователь с таким логином уже существует");
        }

        return Ok("Регистрация успешна");
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLogin input)
    {
        var token = await _userService.LoginAsync(input);

        if (token == null)
        {
            return Unauthorized("Неверный логин или пароль");
        }

        return Ok(new { token });
    }
}