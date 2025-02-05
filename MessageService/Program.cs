using MessageService.Data;
using MessageService.Data.Database;
using MessageService.Data.Repositories;
using MessageService.Services;
using Npgsql;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
var logLevelString = builder.Configuration.GetValue<string>("Serilog:MinimumLevel:Default");
if (!Enum.TryParse(logLevelString, true, out LogEventLevel logLevel))
{
    logLevel = LogEventLevel.Information;
}
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(logLevel)
    .WriteTo.Console()
    .WriteTo.File("/app/logs/myapp.log", rollingInterval: RollingInterval.Day)  // Логируем в файл внутри контейнера
    .CreateLogger();
builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddTransient<IDatabase>(provider => new PostgresDatabase(connectionString, Log.Logger));
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);


builder.WebHost.UseUrls("http://0.0.0.0:5000");


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


var app = builder.Build();
Log.Information("Запускаем приложение.");

var database = app.Services.GetRequiredService<IDatabase>();
database.InitTables();

app.UseCors("AllowSpecificOrigin");

app.UseWebSockets();

// Обработчик для подключения WebSocket
app.Map("/ws", async context =>
{

    if (context.WebSockets.IsWebSocketRequest)
    {
        try
        {
            Log.Information("WebSocket запрос принят от {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            Log.Information("WebSocket подключен: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            var connectionManager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
            connectionManager.AddSocket(webSocket);
            Log.Information("WebSocket подключение добавлено в менеджер: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            await connectionManager.ReceiveMessages(webSocket);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при обработке WebSocket запроса от {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = 500;
        }
    }
    else
    {
        Log.Warning("Некорректный запрос WebSocket от {RequestMethod} {RequestPath}", context.Request.Method, context.Request.Path);
        context.Response.StatusCode = 400;
    }
});


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");
app.MapControllers();
Log.Information("Приложение успешно запущено на порту 5000.");
app.Run();
