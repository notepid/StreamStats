using System;
using System.IO;
using System.Reflection;

namespace StreamStats.Logging
{
    public class FileLogger : BaseLogger
    {
        private readonly string _filename;
        static readonly object Locker = new object();

        public override void Log(LogEntry entry)
        {
            base.Log(entry);

            lock (Locker)
            {
                switch (entry.Severity)
                {
                    case LoggingEventType.Debug:
                        WriteLine(entry.Message, "Debug");
                        break;
                    case LoggingEventType.Error:
                        WriteLine(entry.Exception != null
                            ? $"{entry.Message} : {entry.Exception} : {entry.Exception.StackTrace}"
                            : entry.Message, "Error");
                        break;
                    case LoggingEventType.Fatal:
                        WriteLine(entry.Exception != null
                            ? $"{entry.Message} : {entry.Exception} : {entry.Exception.StackTrace}"
                            : entry.Message, "Fatal");
                        break;
                    case LoggingEventType.Information:
                        WriteLine(entry.Message, "Info");
                        break;
                    case LoggingEventType.Warning:
                        WriteLine(entry.Message, "Warning");
                        break;
                }
            }
        }

        private void WriteLine(string message, string level)
        {
            File.AppendAllText(_filename, $"[{DateTime.Now}] ({level}): {message}\n");
        }

        public FileLogger(string filename, ILogger logger) : base (logger) //Logger chaining
        {
            _filename = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), filename);
        }

        public FileLogger(string filename) : this (filename, null) {}
    }
}
