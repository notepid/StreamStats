namespace StreamStats.Logging
{
    //http://stackoverflow.com/questions/5646820/logger-wrapper-best-practice
    public interface ILogger
    {
        void Log(LogEntry entry);
    }
}
