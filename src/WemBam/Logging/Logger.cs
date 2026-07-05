using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WemBam.Logging
{
    public static class Logger
    {
        private static readonly object SyncRoot = new();

        private static readonly string LogsDirectory =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Wem Bam",
                "Logs");

        private static readonly string SessionId =
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        private static readonly string LogFilePath =
            Path.Combine(
                LogsDirectory,
                $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

        public static void Initialize()
        {
            try
            {
                lock (SyncRoot)
                {
                    Directory.CreateDirectory(LogsDirectory);

                    PerformLogRetention();

                    using StreamWriter writer = CreateWriter();

                    writer.WriteLine("------------------------------------------------------------");
                    writer.WriteLine("Wem Bam Session Started");
                    writer.WriteLine($"Version: {GetApplicationVersion()}");
                    writer.WriteLine($"Machine: {Environment.MachineName}");
                    writer.WriteLine($"OS: {Environment.OSVersion}");
                    writer.WriteLine($"Session ID: {SessionId}");
                    writer.WriteLine("------------------------------------------------------------");
                    writer.WriteLine();
                }
            }
            catch
            {
                // Logging must never prevent the application from running.
            }
        }

        public static void Shutdown()
        {
            try
            {
                lock (SyncRoot)
                {
                    using StreamWriter writer = CreateWriter();

                    writer.WriteLine($"[{DateTimeOffset.UtcNow:O}] [Information] Application closed.");
                    writer.WriteLine();

                    writer.WriteLine("------------------------------------------------------------");
                    writer.WriteLine("Wem Bam Session Ended");
                    writer.WriteLine("------------------------------------------------------------");

                    writer.Flush();
                }
            }
            catch
            {
                // Logging failures must never affect application behaviour.
            }
        }

        public static void Information(string message)
        {
            Write(LogLevel.Information, null, message);
        }

        public static void Warning(string message)
        {
            Write(LogLevel.Warning, null, message);
        }

        public static void Error(string message)
        {
            Write(LogLevel.Error, null, message);
        }

        public static void Error(Exception exception, string message)
        {
            Write(LogLevel.Error, exception, message);
        }

        private static void Write(
            LogLevel level,
            Exception? exception,
            string message)
        {
            try
            {
                lock (SyncRoot)
                {
                    LogEntry entry = new()
                    {
                        Timestamp = DateTimeOffset.UtcNow,
                        Level = level,
                        Message = message,
                        Exception = exception
                    };

                    using StreamWriter writer = CreateWriter();

                    writer.WriteLine(
                        $"[{entry.Timestamp:O}] [{entry.Level}] {entry.Message}");

                    if (entry.Exception is not null)
                    {
                        writer.WriteLine();

                        writer.WriteLine(
                            $"Exception Type: {entry.Exception.GetType().FullName}");

                        writer.WriteLine();

                        writer.WriteLine(
                            $"Exception Message: {entry.Exception.Message}");

                        writer.WriteLine();

                        writer.WriteLine("Stack Trace:");

                        writer.WriteLine();

                        writer.WriteLine(entry.Exception.StackTrace);
                    }

                    writer.WriteLine();

                    writer.Flush();
                }
            }
            catch
            {
                // Logging failures must never affect application behaviour.
            }
        }

        private static StreamWriter CreateWriter()
        {
            return new StreamWriter(
                LogFilePath,
                append: true);
        }

        private static string GetApplicationVersion()
        {
            Version? version =
                Assembly.GetEntryAssembly()?.GetName().Version;

            if (version is null)
            {
                return "Unknown";
            }

            return $"{version.Major}.{version.Minor}";
        }

        private static void PerformLogRetention()
        {
            DirectoryInfo directory = new(LogsDirectory);

            FileInfo[] logFiles = directory
                .GetFiles("*.log")
                .OrderBy(file => file.CreationTimeUtc)
                .ToArray();

            int filesToDelete =
                Math.Max(0, logFiles.Length - 29);

            for (int i = 0; i < filesToDelete; i++)
            {
                try
                {
                    logFiles[i].Delete();
                }
                catch
                {
                    // Logging must never prevent the application from running.
                }
            }
        }
    }
}