using System;

namespace StreamStats.Logging
{
    public class ConsoleLogger : BaseLogger
    {
        public LoggingEventType DisplayLevel = LoggingEventType.Information;

        public override void Log(LogEntry entry)
        {
            base.Log(entry);

            if (entry.Severity < DisplayLevel) return;

            //var callerMethod = GetCallerMethod();

            switch (entry.Severity)
            {
                case LoggingEventType.Debug:
                    WriteLine(entry.Message, "Debug", ConsoleColor.DarkGray);
                    break;
                case LoggingEventType.Error:
                    WriteLine(entry.Exception != null
                        ? $"{entry.Message} : {entry.Exception} : {entry.Exception.StackTrace}"
                        : entry.Message, "Error", ConsoleColor.Red);
                    break;
                case LoggingEventType.Fatal:
                    WriteLine(entry.Exception != null
                        ? $"{entry.Message} : {entry.Exception} : {entry.Exception.StackTrace}"
                        : entry.Message, "Fatal", ConsoleColor.Red);
                    break;
                case LoggingEventType.Information:
                    WriteLine(entry.Message, "Info", ConsoleColor.Green);
                    break;
                case LoggingEventType.Warning:
                    WriteLine(entry.Message, "Warning", ConsoleColor.Yellow);
                    break;
            }
        }

        private void WriteLine(string message, string level, ConsoleColor highlightColor)
        {
            Console.Write($"[{DateTime.Now}] ");
            Console.ForegroundColor = highlightColor;
            Console.Write($"({level}): ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public ConsoleLogger(ILogger logger) : base(logger){}
        public ConsoleLogger() : this(null){}
    }
}
