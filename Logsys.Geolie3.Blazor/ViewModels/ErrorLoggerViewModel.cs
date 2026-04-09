namespace ERP.DEMO.ViewModels
{
    public class ErrorLoggerViewModel
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string ExceptionType { get; set; }
        public string StackTrace { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Path { get; set; }
        public bool IsError { get; set; }
        public string? Source { get; set; }
        public string? InnerException { get; set; }

    }
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }
}
