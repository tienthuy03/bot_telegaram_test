var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Cấu hình Telegram Service
builder.Services.Configure<TelegramLogDemo.Settings.TelegramSettings>(
    builder.Configuration.GetSection("TelegramSettings"));

builder.Services.AddHttpClient<TelegramLogDemo.Services.ITelegramLogService, TelegramLogDemo.Services.TelegramLogService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
