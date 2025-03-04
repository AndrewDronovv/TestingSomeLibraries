using Hangfire;
using Microsoft.AspNetCore.Mvc;
using System.Text;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly FileRecord _fileRecord;

    public JobsController(FileRecord fileRecord)
    {
        _fileRecord = fileRecord;
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

    //[HttpPost("delete-tokens")]
    //public IActionResult DeleteRevokedRefreshTokens()
    //{
    //    var jobId = BackgroundJob.Schedule(
    //        ()=> )
    //}
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