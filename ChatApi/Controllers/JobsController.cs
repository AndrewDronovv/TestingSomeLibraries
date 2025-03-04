using ChatApi.Domain.Entities;
using ChatApi.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using System.Text;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly FileRecord _fileRecord;
    private readonly IUserService _userService;
    private readonly TokenFileRecord _tokenFileRecord;

    public JobsController(FileRecord fileRecord, IUserService userService, TokenFileRecord tokenFileRecord)
    {
        _fileRecord = fileRecord;
        _userService = userService;
        _tokenFileRecord = tokenFileRecord;
    }

    [HttpPost("record-time")]
    public IActionResult StartRecurringTimeJob()
    {
        RecurringJob.AddOrUpdate(
            "record-time",
            () => _fileRecord.RecordCurrentTime(),
            Cron.Minutely());

        return Ok("Время было записано");
    }

    [HttpPost("delete-tokens")]
    public async Task<IActionResult> DeleteRevokedRefreshTokens(int userId)
    {
        var tokenInfos = await _userService
            .GetUserRefreshTokensInfoAsync(userId);

        var jobId = BackgroundJob.Schedule(
            () => _tokenFileRecord.RecordTokenAsync(tokenInfos),
            TimeSpan.FromSeconds(10));

        return Ok(jobId);
    }
}
public class FileRecord
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
    public async Task RecordTokenAsync(List<string> tokenInfos)
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

