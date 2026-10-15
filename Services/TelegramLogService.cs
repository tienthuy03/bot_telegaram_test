using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TelegramLogDemo.Settings;

namespace TelegramLogDemo.Services;

public class TelegramLogService : ITelegramLogService
{
    private readonly HttpClient _httpClient;
    private readonly TelegramSettings _settings;
    private readonly ILogger<TelegramLogService> _logger;

    public TelegramLogService(
        HttpClient httpClient,
        IOptions<TelegramSettings> settings,
        ILogger<TelegramLogService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<bool> SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(_settings.BotToken) || string.IsNullOrWhiteSpace(_settings.ChatId))
        {
            _logger.LogWarning("Chưa cấu hình Telegram BotToken hoặc ChatId.");
            return false;
        }

        try
        {
            var url = $"https://api.telegram.org/bot{_settings.BotToken}/sendMessage";
            var payload = new
            {
                chat_id = _settings.ChatId,
                text = message,
                parse_mode = "HTML"
            };

            var response = await _httpClient.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Telegram SendMessage thất bại: {Error}", error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API Telegram SendMessage.");
            return false;
        }
    }

    public async Task<bool> SendLogFileAsync(string filePath, string caption = "")
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Không tìm thấy file: {Path}", filePath);
            return false;
        }

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var fileName = Path.GetFileName(filePath);
        return await SendLogFileAsync(fileBytes, fileName, caption);
    }

    public async Task<bool> SendLogFileAsync(byte[] fileBytes, string fileName, string caption = "")
    {
        if (string.IsNullOrWhiteSpace(_settings.BotToken) || string.IsNullOrWhiteSpace(_settings.ChatId))
        {
            _logger.LogWarning("Chưa cấu hình Telegram BotToken hoặc ChatId.");
            return false;
        }

        try
        {
            var url = $"https://api.telegram.org/bot{_settings.BotToken}/sendDocument";

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(_settings.ChatId), "chat_id");

            if (!string.IsNullOrEmpty(caption))
            {
                content.Add(new StringContent(caption), "caption");
            }

            var fileContent = new ByteArrayContent(fileBytes);
            content.Add(fileContent, "document", fileName);

            var response = await _httpClient.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Telegram SendDocument thất bại: {Error}", error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API Telegram SendDocument.");
            return false;
        }
    }
}
