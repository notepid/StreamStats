using System;
using System.Reflection;

namespace StreamStats.Logging
{
    public class LogEntry
    {
        public readonly LoggingEventType Severity;
        public readonly string Message;
        public readonly Exception Exception;
        public readonly MethodBase CallingMethod;

        public LogEntry(LoggingEventType severity, string message, Exception exception = null)
        {
            Severity = severity;
            Message = message;
            Exception = exception;
        }
    }
}
