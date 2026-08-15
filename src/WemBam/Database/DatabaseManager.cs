using System;
using System.IO;
using Microsoft.Data.Sqlite;
using WemBam.Logging;
using WemBam.Models;
using System.Collections.Generic;
using System.Threading;

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
         DELETE FROM AudioAssetSources;
         DELETE FROM AudioAssets;
         """;

            command.ExecuteNonQuery();
        }

        public static long AddAudioAsset(
            AudioAsset audioAsset)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        INSERT INTO AudioAssets
        (
            FileId,
            FileName,
            FileExtension,
            Duration,
            DateIndexed,
            DefaultSourceId
        )
        VALUES
        (
            $fileId,
            $fileName,
            $fileExtension,
            $duration,
            $dateIndexed,
            $defaultSourceId
        )
        RETURNING Id;
        """;

            command.Parameters.AddWithValue(
                "$fileId",
                (object?)audioAsset.FileId ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "$fileName",
                audioAsset.FileName);

            command.Parameters.AddWithValue(
                "$fileExtension",
                audioAsset.FileExtension);

            command.Parameters.AddWithValue(
                "$duration",
                (object?)audioAsset.Duration ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "$dateIndexed",
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            command.Parameters.AddWithValue(
                "$defaultSourceId",
                (object?)audioAsset.DefaultSourceId ?? DBNull.Value);

            return command.ExecuteScalar() is long id
                ? id
                : throw new InvalidOperationException(
                    "Failed to retrieve the new AudioAsset ID.");
        }

        public static long AddAudioAssetSource(
            AudioAssetSource audioAssetSource)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        INSERT INTO AudioAssetSources
        (
            AudioAssetId,
            SourceId,
            ContainerPath,
            AssetPath,
            ContentHash
        )
        VALUES
        (
            $audioAssetId,
            $sourceId,
            $containerPath,
            $assetPath,
            $contentHash
        )
        RETURNING Id;
        """;

            command.Parameters.AddWithValue(
                "$audioAssetId",
                audioAssetSource.AudioAssetId);

            command.Parameters.AddWithValue(
                "$sourceId",
                audioAssetSource.SourceId);

            command.Parameters.AddWithValue(
                "$containerPath",
                (object?)audioAssetSource.ContainerPath ??
                DBNull.Value);

            command.Parameters.AddWithValue(
                "$assetPath",
                audioAssetSource.AssetPath);

            command.Parameters.AddWithValue(
                "$contentHash",
                (object?)audioAssetSource.ContentHash ??
                DBNull.Value);

            return command.ExecuteScalar() is long id
                ? id
                : throw new InvalidOperationException(
                    "Failed to retrieve the new AudioAssetSource ID.");
        }

        public static AudioAsset? FindAudioAssetByFileId(
            string fileId)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        SELECT
            Id,
            FileId,
            FileName,
            FileExtension,
            Duration,
            DefaultSourceId
        FROM AudioAssets
        WHERE FileId = $fileId;
        """;

            command.Parameters.AddWithValue(
                "$fileId",
                fileId);

            using SqliteDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new AudioAsset
            {
                Id = reader.GetInt64(0),
                FileId = reader.GetString(1),
                FileName = reader.GetString(2),
                FileExtension = reader.GetString(3),
                Duration = reader.IsDBNull(4)
                    ? null
                    : reader.GetInt32(4),
                DefaultSourceId = reader.IsDBNull(5)
                    ? null
                    : reader.GetInt64(5)
            };
        }

        public static void SetDefaultAudioAssetSource(
            long audioAssetId,
            long audioAssetSourceId)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        UPDATE AudioAssets
        SET DefaultSourceId = $defaultSourceId
        WHERE Id = $audioAssetId;
        """;

            command.Parameters.AddWithValue(
                "$defaultSourceId",
                audioAssetSourceId);

            command.Parameters.AddWithValue(
                "$audioAssetId",
                audioAssetId);

            command.ExecuteNonQuery();
        }

        public static void ReplaceWwiseMetadata(
            IEnumerable<WwiseEvent> events,
            IEnumerable<WwiseStreamedFile> streamedFiles,
            IEnumerable<(string WwiseEventId, string FileId)> relationships,
            CancellationToken cancellationToken)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteTransaction transaction =
                connection.BeginTransaction();

            try
            {
                using SqliteCommand deleteRelationships =
                    connection.CreateCommand();

                deleteRelationships.Transaction = transaction;
                deleteRelationships.CommandText =
                    """
            DELETE FROM WwiseEventToStreamedFiles;
            """;

                deleteRelationships.ExecuteNonQuery();

                using SqliteCommand deleteStreamedFiles =
                    connection.CreateCommand();

                deleteStreamedFiles.Transaction = transaction;
                deleteStreamedFiles.CommandText =
                    """
            DELETE FROM WwiseStreamedFiles;
            """;

                deleteStreamedFiles.ExecuteNonQuery();

                using SqliteCommand deleteEvents =
                    connection.CreateCommand();

                deleteEvents.Transaction = transaction;
                deleteEvents.CommandText =
                    """
            DELETE FROM WwiseEvents;
            """;

                deleteEvents.ExecuteNonQuery();

                foreach (WwiseStreamedFile streamedFile in streamedFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using SqliteCommand command =
                        connection.CreateCommand();

                    command.Transaction = transaction;
                    command.CommandText =
                        """
                INSERT INTO WwiseStreamedFiles
                (
                    FileId,
                    Language,
                    ShortName,
                    Path
                )
                VALUES
                (
                    $fileId,
                    $language,
                    $shortName,
                    $path
                );
                """;

                    command.Parameters.AddWithValue(
                        "$fileId",
                        streamedFile.FileId);

                    command.Parameters.AddWithValue(
                        "$language",
                        streamedFile.Language);

                    command.Parameters.AddWithValue(
                        "$shortName",
                        streamedFile.ShortName);

                    command.Parameters.AddWithValue(
                        "$path",
                        streamedFile.Path);

                    command.ExecuteNonQuery();
                }

                foreach (WwiseEvent wwiseEvent in events)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using SqliteCommand command =
                        connection.CreateCommand();

                    command.Transaction = transaction;
                    command.CommandText =
                        """
                INSERT INTO WwiseEvents
                (
                    Id,
                    Name,
                    ObjectPath,
                    DurationType,
                    DurationMin,
                    DurationMax
                )
                VALUES
                (
                    $id,
                    $name,
                    $objectPath,
                    $durationType,
                    $durationMin,
                    $durationMax
                );
                """;

                    command.Parameters.AddWithValue(
                        "$id",
                        wwiseEvent.Id);

                    command.Parameters.AddWithValue(
                        "$name",
                        wwiseEvent.Name);

                    command.Parameters.AddWithValue(
                        "$objectPath",
                        wwiseEvent.ObjectPath);

                    command.Parameters.AddWithValue(
                        "$durationType",
                        wwiseEvent.DurationType);

                    command.Parameters.AddWithValue(
                        "$durationMin",
                        (object?)wwiseEvent.DurationMin ??
                        DBNull.Value);

                    command.Parameters.AddWithValue(
                        "$durationMax",
                        (object?)wwiseEvent.DurationMax ??
                        DBNull.Value);

                    command.ExecuteNonQuery();
                }

                foreach (
                    (string WwiseEventId, string FileId) relationship
                    in relationships)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using SqliteCommand command =
                        connection.CreateCommand();

                    command.Transaction = transaction;
                    command.CommandText =
                        """
                INSERT INTO WwiseEventToStreamedFiles
                (
                    WwiseEventId,
                    FileId
                )
                VALUES
                (
                    $wwiseEventId,
                    $fileId
                );
                """;

                    command.Parameters.AddWithValue(
                        "$wwiseEventId",
                        relationship.WwiseEventId);

                    command.Parameters.AddWithValue(
                        "$fileId",
                        relationship.FileId);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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