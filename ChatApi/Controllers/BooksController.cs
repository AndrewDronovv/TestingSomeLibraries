using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApi.Controllers;

[Route("api/books")]
[Authorize]
[ApiController]
public class BooksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetBooks()
    {
        return Ok("Книги получены");
    }
}
