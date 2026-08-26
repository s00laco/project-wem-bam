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

        public static IReadOnlyList<SearchResult> SearchAudioAssets(
    string query)
        {
            ArgumentNullException.ThrowIfNull(query);

            string trimmedQuery =
                query.Trim();

            string escapedQuery =
                trimmedQuery
                    .Replace("\\", "\\\\")
                    .Replace("%", "\\%")
                    .Replace("_", "\\_");

            string searchPattern =
                $"%{escapedQuery}%";

            string prefixPattern =
                $"{escapedQuery}%";

            using SqliteConnection connection =
                OpenConnection();

            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText =
                """
        SELECT
            audioAsset.FileName,

            streamedFile.Path,

            (
                SELECT GROUP_CONCAT(
                    wwiseEvent.Name,
                    CHAR(10))
                FROM WwiseEventToStreamedFiles relationship
                INNER JOIN WwiseEvents wwiseEvent
                    ON wwiseEvent.Id =
                       relationship.WwiseEventId
                WHERE relationship.FileId =
                      audioAsset.FileId
            ) AS WwiseEvents,

            (
                CASE
                    WHEN $query = '' THEN 0

                    WHEN audioAsset.FileId = $query
                        THEN 12

                    WHEN audioAsset.FileId LIKE
                         $prefixPattern ESCAPE '\'
                        THEN 8

                    WHEN audioAsset.FileId LIKE
                         $searchPattern ESCAPE '\'
                        THEN 4

                    ELSE 0
                END

                +

                CASE
                    WHEN $query = '' THEN 0

                    WHEN audioAsset.FileName = $query
                        THEN 9

                    WHEN audioAsset.FileName LIKE
                         $prefixPattern ESCAPE '\'
                        THEN 6

                    WHEN audioAsset.FileName LIKE
                         $searchPattern ESCAPE '\'
                        THEN 3

                    ELSE 0
                END

                +

                CASE
                    WHEN $query = '' THEN 0

                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM AudioAssetSources source
                        WHERE source.AudioAssetId =
                              audioAsset.Id
                          AND
                          (
                              source.ContainerPath =
                                  $query
                              OR source.AssetPath =
                                  $query
                          )
                    )
                        THEN 3

                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM AudioAssetSources source
                        WHERE source.AudioAssetId =
                              audioAsset.Id
                          AND
                          (
                              source.ContainerPath LIKE
                                  $prefixPattern ESCAPE '\'
                              OR source.AssetPath LIKE
                                  $prefixPattern ESCAPE '\'
                          )
                    )
                        THEN 2

                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM AudioAssetSources source
                        WHERE source.AudioAssetId =
                              audioAsset.Id
                          AND
                          (
                              source.ContainerPath LIKE
                                  $searchPattern ESCAPE '\'
                              OR source.AssetPath LIKE
                                  $searchPattern ESCAPE '\'
                          )
                    )
                        THEN 1

                    ELSE 0
                END

                +

                CASE
                    WHEN $query = '' THEN 0

                    WHEN streamedFile.ShortName =
                         $query
                        THEN 6

                    WHEN streamedFile.ShortName LIKE
                         $prefixPattern ESCAPE '\'
                        THEN 4

                    WHEN streamedFile.ShortName LIKE
                         $searchPattern ESCAPE '\'
                        THEN 2

                    ELSE 0
                END

                +

                CASE
                    WHEN $query = '' THEN 0

                    WHEN streamedFile.Path =
                         $query
                        THEN 3

                    WHEN streamedFile.Path LIKE
                         $prefixPattern ESCAPE '\'
                        THEN 2

                    WHEN streamedFile.Path LIKE
                         $searchPattern ESCAPE '\'
                        THEN 1

                    ELSE 0
                END

                +

                CASE
                    WHEN $query = '' THEN 0

                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM WwiseEventToStreamedFiles relationship
                        INNER JOIN WwiseEvents wwiseEvent
                            ON wwiseEvent.Id =
                               relationship.WwiseEventId
                        WHERE relationship.FileId =
                              audioAsset.FileId
                          AND wwiseEvent.Name =
                              $query
                    )
                        THEN 6

                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM WwiseEventToStreamedFiles relationship
                        INNER JOIN WwiseEvents wwiseEvent
                            ON wwiseEvent.Id =
                               relationship.WwiseEventId
                        WHERE relationship.FileId =
                              audioAsset.FileId
                          AND wwiseEvent.Name LIKE
                              $prefixPattern ESCAPE '\'
                    )
                        THEN 4

                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM WwiseEventToStreamedFiles relationship
                        INNER JOIN WwiseEvents wwiseEvent
                            ON wwiseEvent.Id =
                               relationship.WwiseEventId
                        WHERE relationship.FileId =
                              audioAsset.FileId
                          AND wwiseEvent.Name LIKE
                              $searchPattern ESCAPE '\'
                    )
                        THEN 2

                    ELSE 0
                END
            ) AS SearchScore,

            defaultSource.ContainerPath,

            defaultSource.AssetPath

        FROM AudioAssets audioAsset

        LEFT JOIN AudioAssetSources defaultSource
            ON defaultSource.Id =
               audioAsset.DefaultSourceId

        LEFT JOIN WwiseStreamedFiles streamedFile
            ON streamedFile.FileId =
               audioAsset.FileId

        WHERE
            $query = ''

            OR audioAsset.FileId LIKE
               $searchPattern ESCAPE '\'

            OR audioAsset.FileName LIKE
               $searchPattern ESCAPE '\'

            OR EXISTS
            (
                SELECT 1
                FROM AudioAssetSources source
                WHERE source.AudioAssetId =
                      audioAsset.Id
                  AND
                  (
                      source.ContainerPath LIKE
                          $searchPattern ESCAPE '\'
                      OR source.AssetPath LIKE
                          $searchPattern ESCAPE '\'
                  )
            )

            OR streamedFile.ShortName LIKE
               $searchPattern ESCAPE '\'

            OR streamedFile.Path LIKE
               $searchPattern ESCAPE '\'

            OR EXISTS
            (
                SELECT 1
                FROM WwiseEventToStreamedFiles relationship
                INNER JOIN WwiseEvents wwiseEvent
                    ON wwiseEvent.Id =
                       relationship.WwiseEventId
                WHERE relationship.FileId =
                      audioAsset.FileId
                  AND wwiseEvent.Name LIKE
                      $searchPattern ESCAPE '\'
            )

        ORDER BY
            SearchScore DESC,
            audioAsset.FileName COLLATE NOCASE,
            audioAsset.FileId;
        """;

            command.Parameters.AddWithValue(
                "$query",
                trimmedQuery);

            command.Parameters.AddWithValue(
                "$searchPattern",
                searchPattern);

            command.Parameters.AddWithValue(
                "$prefixPattern",
                prefixPattern);

            List<SearchResult> results = new();

            using SqliteDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                List<string> wwiseEvents = new();

                if (!reader.IsDBNull(2))
                {
                    string eventText =
                        reader.GetString(2);

                    wwiseEvents.AddRange(
                        eventText.Split(
                            '\n',
                            StringSplitOptions.RemoveEmptyEntries));
                }

                results.Add(
                    new SearchResult
                    {
                        FileName =
                            reader.GetString(0),

                        WwisePath =
                            reader.IsDBNull(1)
                                ? string.Empty
                                : reader.GetString(1),

                        WwiseEvents =
                            wwiseEvents,

                        ContainerPath =
                            reader.IsDBNull(4)
                                ? null
                                : reader.GetString(4),

                        AssetPath =
                            reader.IsDBNull(5)
                                ? string.Empty
                                : reader.GetString(5)
                    });
            }

            return results;
        }

    }
}