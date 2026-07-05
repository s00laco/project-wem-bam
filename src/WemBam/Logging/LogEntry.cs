using System;

namespace WemBam.Logging
{
    internal class LogEntry
    {
        public DateTimeOffset Timestamp { get; init; }

        public LogLevel Level { get; init; }

        public string Message { get; init; } = string.Empty;

        public Exception? Exception { get; init; }
    }
}