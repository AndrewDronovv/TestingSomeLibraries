using ChatApi.Domain.Entities;
using ChatApi.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly TaskFileRecord _fileRecord;
    private readonly IUserService _userService;
    private readonly TokenFileRecord _tokenFileRecord;

    public JobsController(TaskFileRecord fileRecord, IUserService userService, TokenFileRecord tokenFileRecord)
    {
        _fileRecord = fileRecord;
        _userService = userService;
        _tokenFileRecord = tokenFileRecord;
    }

    [Authorize]
    [HttpPost("record-time")]
    public IActionResult StartRecurringTimeJob()
    {
        RecurringJob.AddOrUpdate(
            "record-time",
            () => _fileRecord.RecordCurrentTime(),
            Cron.Minutely());

        return Ok("Время было записано");
    }

    /// <summary>
    /// Запускает фоновую задачу для записи информации о refresh токенах пользователя в файл.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, чьи токены будут обработаны</param>
    /// <returns>Возвращает объект, содержащий идентификатор фоновой задачи и сообщение о статусе операции</returns>
    /// <response code="200">Фоновая задача успешно запланирована, возвращает jobId и сообщение</response>
    /// <response code="401">Пользователь не авторизован</response>
    [Authorize]
    [HttpPost("info-refresh-tokens")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> StartScheduleTimeJob([FromQuery] int userId)
    {
        var tokenInfos = await _userService
            .GetUserRefreshTokensInfoAsync(userId);

        var jobId = BackgroundJob.Schedule(
            () => _tokenFileRecord.RecordTokenInfoAsync(tokenInfos),
            TimeSpan.FromSeconds(10));

        return Ok(new
        {
            jobId,
            message = @$"Фоновая задача запланирована успешно, {tokenInfos.Count()} записей будет записано в файл",
        });
    }
}
public class TaskFileRecord
{
    public async Task RecordCurrentTime()
    {
        try
        {
            string fileName = "file.txt";
            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            using (FileStream fileStream = new FileStream(path, FileMode.Append, FileAccess.Write,
                FileShare.None, 4096, useAsync: true))
            {
                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                byte[] buffer = Encoding.UTF8.GetBytes($"Задача записана в {currentTime}\n");
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка при записи в файл: {ex.Message}");
        }
    }
}

public class TokenFileRecord
{
    public async Task RecordTokenInfoAsync(List<string> tokenInfos)
    {
        try
        {
            string fileName = "token.txt";
            string path = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            string content = string.Join(Environment.NewLine, tokenInfos) + Environment.NewLine;

            await File.WriteAllTextAsync(path, content, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при записи в файл: {ex.Message}");
        }
    }
}

