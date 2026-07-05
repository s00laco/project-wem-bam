using System;
using System.IO;
using Microsoft.Data.Sqlite;
using WemBam.Logging;

namespace WemBam.Database
{
    public static class DatabaseManager
    {
        private static readonly string DatabaseDirectory =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                DatabaseConstants.ApplicationFolderName);

        private static readonly string DatabasePath =
            Path.Combine(DatabaseDirectory, DatabaseConstants.DatabaseFileName);

        private static readonly string ConnectionString =
            $"Data Source={DatabasePath}";

        public static string GetDatabasePath()
        {
            return DatabasePath;
        }

        public static void Initialize()
        {
            try
            {
                Directory.CreateDirectory(DatabaseDirectory);

                using SqliteConnection connection = OpenConnection();

                DatabaseInitializer.Initialize(connection);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize the database.");

                throw new InvalidOperationException(
                    "Failed to initialize the Wem Bam database.",
                    ex);
            }
        }

        public static SqliteConnection OpenConnection()
        {
            SqliteConnection connection = new(ConnectionString);

            connection.Open();

            return connection;
        }
    }
}