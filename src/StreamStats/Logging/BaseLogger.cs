
namespace StreamStats.Logging
{
    public abstract class BaseLogger : ILogger
    {
        private readonly ILogger _logger;

        public virtual void Log(LogEntry entry)
        {
            _logger?.Log(entry); //Call logger chain if its defined
        }

        protected BaseLogger(){}

        protected BaseLogger(ILogger logger)
        {
            _logger = logger;
        }
    }
}
