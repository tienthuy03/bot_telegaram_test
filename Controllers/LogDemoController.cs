using Microsoft.AspNetCore.Mvc;
using TelegramLogDemo.Services;

namespace TelegramLogDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogDemoController : ControllerBase
{
    private readonly ITelegramLogService _telegramService;
    private readonly ILogger<LogDemoController> _logger;

    public LogDemoController(ITelegramLogService telegramService, ILogger<LogDemoController> logger)
    {
        _telegramService = telegramService;
        _logger = logger;
    }

    /// <summary>
    /// Test gửi 1 tin nhắn thông báo text lên Telegram
    /// </summary>
    [HttpGet("send-message")]
    public async Task<IActionResult> SendTextMessage([FromQuery] string? message)
    {
        var msg = message ?? $"🔔 <b>[TEST]</b> Thông báo thử nghiệm từ ASP.NET Core lúc <code>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</code>!";
        var success = await _telegramService.SendMessageAsync(msg);

        if (success)
        {
            return Ok(new { success = true, message = "Đã gửi tin nhắn lên Telegram thành công!" });
        }

        return BadRequest(new { success = false, message = "Gửi thất bại. Hãy kiểm tra lại BotToken, ChatId và đảm bảo bạn đã bấm /start với bot!" });
    }

    /// <summary>
    /// Test tạo và đẩy file log song song lên Telegram
    /// </summary>
    [HttpPost("send-file")]
    public async Task<IActionResult> SendLogFile()
    {
        // 1. Giả lập tạo file log trong thư mục logs
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        var fileName = $"api_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        var filePath = Path.Combine(logDir, fileName);

        var logContent = $"""
        =====================================================
        SYSTEM LOG DUMP - TEST INTEGRATION
        Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
        Endpoint: /api/logdemo/send-file
        Status: 200 OK
        Message: Chạy thử nghiệm đẩy log song song lên Telegram
        Client IP: {HttpContext.Connection.RemoteIpAddress}
        =====================================================
        [INFO] Bắt đầu gọi API
        [INFO] Đang thực hiện xử lý dữ liệu...
        [INFO] Xử lý thành công!
        """;

        await System.IO.File.WriteAllTextAsync(filePath, logContent);

        // 2. Kích hoạt đẩy song song ngầm (Non-blocking Fire-and-Forget)
        _ = Task.Run(async () =>
        {
            var caption = $"📄 <b>File Log mới:</b> <code>{fileName}</code>\n⏰ <i>Thời gian: {DateTime.Now:HH:mm:ss}</i>";
            await _telegramService.SendLogFileAsync(filePath, caption);
        });

        // 3. API trả về ngay cho client mà không bị delay
        return Ok(new
        {
            success = true,
            message = "API đã xử lý xong và đã kích hoạt tác vụ gửi file log lên Telegram ngầm!",
            fileCreated = fileName
        });
    }
}
