using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Data.Sqlite;
using WemBam.Logging;
using WemBam.Models;
using WemBam.Services;

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

        public static void UpdateAudioAssetDuration(
            long audioAssetId,
            int? duration)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
        UPDATE AudioAssets
        SET Duration = $duration
        WHERE Id = $audioAssetId;
        """;

            command.Parameters.AddWithValue(
                "$audioAssetId",
                audioAssetId);

            command.Parameters.AddWithValue(
                "$duration",
                (object?)duration ?? DBNull.Value);

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

        public static AudioAssetDetails? GetAudioAssetDetails(
    long audioAssetId)
        {
            using SqliteConnection connection = OpenConnection();

            AudioAsset? audioAsset;

            using (SqliteCommand command = connection.CreateCommand())
            {
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
            WHERE Id = $audioAssetId;
            """;

                command.Parameters.AddWithValue(
                    "$audioAssetId",
                    audioAssetId);

                using SqliteDataReader reader =
                    command.ExecuteReader();

                if (!reader.Read())
                {
                    return null;
                }

                audioAsset =
                    new AudioAsset
                    {
                        Id = reader.GetInt64(0),

                        FileId =
                            reader.IsDBNull(1)
                                ? null
                                : reader.GetString(1),

                        FileName =
                            reader.GetString(2),

                        FileExtension =
                            reader.GetString(3),

                        Duration =
                            reader.IsDBNull(4)
                                ? null
                                : reader.GetInt32(4),

                        DefaultSourceId =
                            reader.IsDBNull(5)
                                ? null
                                : reader.GetInt64(5)
                    };
            }

            List<AudioAssetSource> sources = new();

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    """
            SELECT
                Id,
                AudioAssetId,
                SourceId,
                ContainerPath,
                AssetPath,
                ContentHash
            FROM AudioAssetSources
            WHERE AudioAssetId = $audioAssetId
            ORDER BY Id;
            """;

                command.Parameters.AddWithValue(
                    "$audioAssetId",
                    audioAssetId);

                using SqliteDataReader reader =
                    command.ExecuteReader();

                while (reader.Read())
                {
                    sources.Add(
                        new AudioAssetSource
                        {
                            Id = reader.GetInt64(0),

                            AudioAssetId =
                                reader.GetInt64(1),

                            SourceId =
                                reader.GetInt64(2),

                            ContainerPath =
                                reader.IsDBNull(3)
                                    ? null
                                    : reader.GetString(3),

                            AssetPath =
                                reader.GetString(4),

                            ContentHash =
                                reader.IsDBNull(5)
                                    ? null
                                    : reader.GetString(5)
                        });
                }
            }

            string wwisePath = string.Empty;

            if (!string.IsNullOrWhiteSpace(audioAsset.FileId))
            {
                using SqliteCommand command = connection.CreateCommand();

                command.CommandText =
                    """
            SELECT Path
            FROM WwiseStreamedFiles
            WHERE FileId = $fileId
            LIMIT 1;
            """;

                command.Parameters.AddWithValue(
                    "$fileId",
                    audioAsset.FileId);

                object? value =
                    command.ExecuteScalar();

                if (value is string path)
                {
                    wwisePath = path;
                }
            }

            List<WwiseEvent> wwiseEvents = new();

            if (!string.IsNullOrWhiteSpace(audioAsset.FileId))
            {
                using SqliteCommand command = connection.CreateCommand();

                command.CommandText =
                    """
            SELECT
                wwiseEvent.Id,
                wwiseEvent.Name,
                wwiseEvent.ObjectPath,
                wwiseEvent.DurationType,
                wwiseEvent.DurationMin,
                wwiseEvent.DurationMax
            FROM WwiseEventToStreamedFiles relationship
            INNER JOIN WwiseEvents wwiseEvent
                ON wwiseEvent.Id =
                   relationship.WwiseEventId
            WHERE relationship.FileId = $fileId
            ORDER BY wwiseEvent.Name COLLATE NOCASE;
            """;

                command.Parameters.AddWithValue(
                    "$fileId",
                    audioAsset.FileId);

                using SqliteDataReader reader =
                    command.ExecuteReader();

                while (reader.Read())
                {
                    wwiseEvents.Add(
                        new WwiseEvent
                        {
                            Id =
                                reader.GetString(0),

                            Name =
                                reader.GetString(1),

                            ObjectPath =
                                reader.GetString(2),

                            DurationType =
                                reader.GetString(3),

                            DurationMin =
                                reader.IsDBNull(4)
                                    ? null
                                    : reader.GetDouble(4),

                            DurationMax =
                                reader.IsDBNull(5)
                                    ? null
                                    : reader.GetDouble(5)
                        });
                }
            }

            List<string?> classSources =
                new()
                {
                    wwisePath,
                    audioAsset.FileName
                };

            foreach (WwiseEvent wwiseEvent in wwiseEvents)
            {
                classSources.Add(wwiseEvent.Name);
                classSources.Add(wwiseEvent.ObjectPath);
            }

            IReadOnlyList<AudioCategory> categories =
                AudioCategoryLibrary.CreateManyFromWwiseText(
                    classSources.ToArray());

            bool isLooped =
                classSources.Any(
                    source =>
                        !string.IsNullOrWhiteSpace(source) &&
                        (source.Contains(
                            "_LP",
                            StringComparison.OrdinalIgnoreCase) ||
                         source.Contains(
                            "Loop",
                            StringComparison.OrdinalIgnoreCase)));

            return new AudioAssetDetails
            {
                AudioAsset = audioAsset,

                WwisePath = wwisePath,

                Sources = sources,

                WwiseEvents = wwiseEvents,

                Categories = categories,

                IsLooped = isLooped
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
            string query,
            long? collectionId = null)
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

            audioAsset.Duration,

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

            defaultSource.AssetPath,

            audioAsset.Id AS AudioAssetId

        FROM AudioAssets audioAsset

        LEFT JOIN AudioAssetSources defaultSource
            ON defaultSource.Id =
               audioAsset.DefaultSourceId

        LEFT JOIN WwiseStreamedFiles streamedFile
            ON streamedFile.FileId =
               audioAsset.FileId

        WHERE
        (
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
        )
        AND
        (
            $collectionId IS NULL
            OR EXISTS
            (
                SELECT 1
                FROM AudioAssetCollections membership
                WHERE membership.AudioAssetId =
                      audioAsset.Id
                  AND membership.CollectionId =
                      $collectionId
            )
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

            command.Parameters.AddWithValue(
                "$collectionId",
                (object?)collectionId ?? DBNull.Value);

            List<SearchResult> results = new();

            using SqliteDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                List<string> wwiseEvents = new();

                if (!reader.IsDBNull(3))
                {
                    string eventText =
                        reader.GetString(3);

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

                        Duration =
                            reader.IsDBNull(1)
                                ? null
                                : reader.GetInt32(1),

                        WwisePath =
                            reader.IsDBNull(2)
                                ? string.Empty
                                : reader.GetString(2),

                        WwiseEvents =
                            wwiseEvents,

                        ContainerPath =
                            reader.IsDBNull(5)
                                ? null
                                : reader.GetString(5),

                        AssetPath =
                            reader.IsDBNull(6)
                                ? string.Empty
                                : reader.GetString(6),

                        AudioAssetId =
                            reader.GetInt64(7)
                    });
            }

            return results;
        }

        public static IReadOnlyList<Collection> GetCollections()
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    Id,
                    Name,
                    Colour
                FROM Collections
                ORDER BY Name COLLATE NOCASE;
                """;

            List<Collection> collections = new();

            using SqliteDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                collections.Add(
                    new Collection
                    {
                        Id = reader.GetInt64(0),
                        Name = reader.GetString(1),
                        Colour = reader.IsDBNull(2)
                            ? null
                            : reader.GetString(2)
                    });
            }

            return collections;
        }

        public static long AddCollection(
            string name,
            string? colour = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                INSERT INTO Collections
                (
                    Name,
                    Colour
                )
                VALUES
                (
                    $name,
                    $colour
                )
                RETURNING Id;
                """;

            command.Parameters.AddWithValue(
                "$name",
                name.Trim());

            command.Parameters.AddWithValue(
                "$colour",
                (object?)colour ?? DBNull.Value);

            return command.ExecuteScalar() is long id
                ? id
                : throw new InvalidOperationException(
                    "Failed to retrieve the new Collection ID.");
        }

        public static void RenameCollection(
            long collectionId,
            string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                UPDATE Collections
                SET Name = $name
                WHERE Id = $collectionId;
                """;

            command.Parameters.AddWithValue(
                "$name",
                name.Trim());

            command.Parameters.AddWithValue(
                "$collectionId",
                collectionId);

            command.ExecuteNonQuery();
        }

        public static void DeleteCollection(
            long collectionId)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteTransaction transaction =
                connection.BeginTransaction();

            try
            {
                using SqliteCommand deleteMemberships =
                    connection.CreateCommand();

                deleteMemberships.Transaction = transaction;

                deleteMemberships.CommandText =
                    """
                    DELETE FROM AudioAssetCollections
                    WHERE CollectionId = $collectionId;
                    """;

                deleteMemberships.Parameters.AddWithValue(
                    "$collectionId",
                    collectionId);

                deleteMemberships.ExecuteNonQuery();

                using SqliteCommand deleteCollection =
                    connection.CreateCommand();

                deleteCollection.Transaction = transaction;

                deleteCollection.CommandText =
                    """
                    DELETE FROM Collections
                    WHERE Id = $collectionId;
                    """;

                deleteCollection.Parameters.AddWithValue(
                    "$collectionId",
                    collectionId);

                deleteCollection.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public static IReadOnlyList<long> GetCollectionIdsForAudioAsset(
            long audioAssetId)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT CollectionId
                FROM AudioAssetCollections
                WHERE AudioAssetId = $audioAssetId
                ORDER BY CollectionId;
                """;

            command.Parameters.AddWithValue(
                "$audioAssetId",
                audioAssetId);

            List<long> collectionIds = new();

            using SqliteDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                collectionIds.Add(
                    reader.GetInt64(0));
            }

            return collectionIds;
        }

        public static void SetAudioAssetCollections(
            long audioAssetId,
            IEnumerable<long> collectionIds)
        {
            using SqliteConnection connection = OpenConnection();

            using SqliteTransaction transaction =
                connection.BeginTransaction();

            try
            {
                using SqliteCommand deleteCommand =
                    connection.CreateCommand();

                deleteCommand.Transaction = transaction;

                deleteCommand.CommandText =
                    """
                    DELETE FROM AudioAssetCollections
                    WHERE AudioAssetId = $audioAssetId;
                    """;

                deleteCommand.Parameters.AddWithValue(
                    "$audioAssetId",
                    audioAssetId);

                deleteCommand.ExecuteNonQuery();

                foreach (long collectionId in collectionIds.Distinct())
                {
                    using SqliteCommand insertCommand =
                        connection.CreateCommand();

                    insertCommand.Transaction = transaction;

                    insertCommand.CommandText =
                        """
                        INSERT INTO AudioAssetCollections
                        (
                            AudioAssetId,
                            CollectionId
                        )
                        VALUES
                        (
                            $audioAssetId,
                            $collectionId
                        );
                        """;

                    insertCommand.Parameters.AddWithValue(
                        "$audioAssetId",
                        audioAssetId);

                    insertCommand.Parameters.AddWithValue(
                        "$collectionId",
                        collectionId);

                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}