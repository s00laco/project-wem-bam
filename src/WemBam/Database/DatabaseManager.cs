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
        public static void ClearAudioAssets()
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        DELETE FROM AudioAssets;
        """;

            command.ExecuteNonQuery();
        }

        public static void AddAudioAsset(
            string assetPath)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        INSERT INTO AudioAssets
        (
            SourceId,
            FileName,
            FileExtension,
            ContainerPath,
            AssetPath,
            Duration,
            DateIndexed
        )
        VALUES
        (
            $sourceId,
            $fileName,
            $fileExtension,
            $containerPath,
            $assetPath,
            $duration,
            $dateIndexed
        );
        """;

            command.Parameters.AddWithValue(
                "$sourceId",
                0);

            command.Parameters.AddWithValue(
                "$fileName",
                Path.GetFileName(assetPath));

            command.Parameters.AddWithValue(
                "$fileExtension",
                Path.GetExtension(assetPath));

            command.Parameters.AddWithValue(
                "$containerPath",
                DBNull.Value);

            command.Parameters.AddWithValue(
                "$assetPath",
                assetPath);

            command.Parameters.AddWithValue(
                "$duration",
                DBNull.Value);

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
        FROM AudioAssets;
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