namespace TelegramLogDemo.Services;

public interface ITelegramLogService
{
    Task<bool> SendMessageAsync(string message);
    Task<bool> SendLogFileAsync(string filePath, string caption = "");
    Task<bool> SendLogFileAsync(byte[] fileBytes, string fileName, string caption = "");
}
