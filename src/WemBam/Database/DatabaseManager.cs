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
        public static void ClearIndexedFiles()
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        DELETE FROM IndexedFiles;
        """;

            command.ExecuteNonQuery();
        }

        public static void AddIndexedFile(
            string filePath)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        INSERT INTO IndexedFiles
        (
            FilePath,
            FileName,
            FileExtension,
            DateIndexed
        )
        VALUES
        (
            $filePath,
            $fileName,
            $fileExtension,
            $dateIndexed
        );
        """;

            command.Parameters.AddWithValue(
                "$filePath",
                filePath);

            command.Parameters.AddWithValue(
                "$fileName",
                Path.GetFileName(filePath));

            command.Parameters.AddWithValue(
                "$fileExtension",
                Path.GetExtension(filePath));

            command.Parameters.AddWithValue(
                "$dateIndexed",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            command.ExecuteNonQuery();
        }
        public static (
    DateTimeOffset? LastIndexed,
    int IndexedFileCount) LoadIndexStatus()
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        SELECT
            COUNT(*),
            MAX(DateIndexed)
        FROM IndexedFiles;
        """;

            using SqliteDataReader reader = command.ExecuteReader();

            reader.Read();

            int indexedFileCount = reader.GetInt32(0);

            if (indexedFileCount == 0 ||
                reader.IsDBNull(1))
            {
                return (null, 0);
            }

            long unixMilliseconds = reader.GetInt64(1);

            return (
                DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds),
                indexedFileCount);
        }
    }
}