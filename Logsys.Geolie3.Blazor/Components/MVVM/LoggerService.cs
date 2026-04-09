using System.Text.Json;
using ERP.DEMO.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using LogLevel = ERP.DEMO.ViewModels.LogLevel;

namespace ERP.DEMO.Components.MVVM
{
    public class LoggerService
    {
        private readonly string LogFilePath = "logs/app-logs.jsonl";
        private readonly NavigationManager _nav;
        private readonly IServiceProvider _provider;

        public LoggerService(NavigationManager nav, IServiceProvider provider)
        {
            _nav = nav;
            _provider = provider;
        }

        private UserService? _user;
        private UserService User => _user ??= _provider.GetRequiredService<UserService>();

        public async Task Log(LogLevel level, string message, Exception ex = null)
        {
            var user = User.GetUser();

            var entry = new ErrorLoggerViewModel
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message ?? ex?.Message ?? "Message d'erreur vide",
                ExceptionType = ex?.GetType().Name,
                StackTrace = ex?.StackTrace,
                IsError = (ex != null),
                UserId = user?.Id.ToString() ?? "inconnu",
                UserName = user?.Username ?? "anonyme",
                Path = _nav.Uri,
                Source = ex?.Source,
                InnerException = ex?.InnerException?.ToString()
            };


            var json = JsonSerializer.Serialize(entry) + Environment.NewLine;
            var path = "logs/app-logs.jsonl";

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.AppendAllTextAsync(path, json);
        }
        public Task LogInfo(string msg) => Log(LogLevel.Info, msg);
        public Task LogWarning(string msg) => Log(LogLevel.Warning, msg);
        public Task LogError(string msg = null, Exception ex = null) => Log(LogLevel.Error, msg, ex);
        public Task LogError(Exception ex = null) => Log(LogLevel.Error, null, ex);
        public Task LogCritical(Exception ex = null) => Log(LogLevel.Critical, null, ex);
        public Task LogCritical(string msg = null, Exception ex = null) => Log(LogLevel.Critical, msg, ex);
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Exception? Exception { get; set; }
        public int? Code { get; set; }

        public static OperationResult Ok(string? message = null) => new()
        {
            Success = true,
            Message = message
        };

        public static OperationResult Fail(Exception? ex, string? message = null, int? code =  null) => new()
        {
            Success = false,
            Message = message ?? ex.Message,
            Exception = ex,
            Code = code
        };
        public static OperationResult Fail(string? message = null, int? code = null) => new()
        {
            Success = false,
            Message = message,
            Code = code
        };
    }


}
