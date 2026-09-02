using Microsoft.Data.Sqlite;

namespace WemBam.Database
{
    public static class DatabaseSchema
    {
        public static void CreateVersion1(SqliteConnection connection)
        {
            const string sql = """
        CREATE TABLE IF NOT EXISTS Sources
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            DisplayName TEXT NOT NULL,
            Path TEXT NOT NULL UNIQUE,
            SourceType INTEGER NOT NULL,
            Enabled INTEGER NOT NULL,
            DateAdded INTEGER NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_Sources_Path
            ON Sources(Path);
        """;

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public static void UpgradeToVersion2(
            SqliteConnection connection)
        {
            const string sql = """
        CREATE TABLE IF NOT EXISTS IndexedFiles
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FilePath TEXT NOT NULL UNIQUE,
            FileName TEXT NOT NULL,
            FileExtension TEXT NOT NULL,
            DateIndexed INTEGER NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_IndexedFiles_FilePath
            ON IndexedFiles(FilePath);
        """;

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public static void UpgradeToVersion3(
            SqliteConnection connection)
        {
            const string sql = """
        DROP TABLE IF EXISTS IndexedFiles;

        CREATE TABLE IF NOT EXISTS AudioAssets
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            SourceId INTEGER NOT NULL,
            FileName TEXT NOT NULL,
            FileExtension TEXT NOT NULL,
            ContainerPath TEXT NULL,
            AssetPath TEXT NOT NULL,
            Duration INTEGER NULL,
            DateIndexed INTEGER NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_AudioAssets_AssetPath
            ON AudioAssets(AssetPath);
        """;

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public static void UpgradeToVersion4(
            SqliteConnection connection)
        {
            const string sql = """
        ALTER TABLE AudioAssets
            ADD COLUMN FileId TEXT NULL;

        CREATE TABLE IF NOT EXISTS WwiseEvents
        (
            Id TEXT PRIMARY KEY,
            Name TEXT NOT NULL,
            ObjectPath TEXT NOT NULL,
            DurationType TEXT NOT NULL,
            DurationMin REAL NULL,
            DurationMax REAL NULL
        );

        CREATE TABLE IF NOT EXISTS WwiseStreamedFiles
        (
            FileId TEXT PRIMARY KEY,
            Language TEXT NOT NULL,
            ShortName TEXT NOT NULL,
            Path TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS WwiseEventToStreamedFiles
        (
            WwiseEventId TEXT NOT NULL,
            FileId TEXT NOT NULL,
            UNIQUE(WwiseEventId, FileId)
        );

        CREATE INDEX IF NOT EXISTS IX_AudioAssets_FileId
            ON AudioAssets(FileId);

        CREATE INDEX IF NOT EXISTS IX_WwiseEventToStreamedFiles_WwiseEventId
            ON WwiseEventToStreamedFiles(WwiseEventId);

        CREATE INDEX IF NOT EXISTS IX_WwiseEventToStreamedFiles_FileId
            ON WwiseEventToStreamedFiles(FileId);
        """;

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public static void UpgradeToVersion5(
            SqliteConnection connection)
        {
            const string sql = """
        DROP TABLE IF EXISTS AudioAssets;

        CREATE TABLE IF NOT EXISTS AudioAssets
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FileId TEXT NOT NULL UNIQUE,
            FileName TEXT NOT NULL,
            FileExtension TEXT NOT NULL,
            Duration INTEGER NULL,
            DateIndexed INTEGER NOT NULL,
            DefaultSourceId INTEGER NULL
        );

        CREATE TABLE IF NOT EXISTS AudioAssetSources
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AudioAssetId INTEGER NOT NULL,
            SourceId INTEGER NOT NULL,
            ContainerPath TEXT NULL,
            AssetPath TEXT NOT NULL,
            ContentHash TEXT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_AudioAssets_FileId
            ON AudioAssets(FileId);

        CREATE INDEX IF NOT EXISTS IX_AudioAssetSources_AudioAssetId
            ON AudioAssetSources(AudioAssetId);

        CREATE INDEX IF NOT EXISTS IX_AudioAssetSources_SourceId
            ON AudioAssetSources(SourceId);
        """;

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        public static void UpgradeToVersion6(
            SqliteConnection connection)
        {
            const string sql = """
        CREATE TABLE IF NOT EXISTS Collections
        (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL UNIQUE,
            Colour TEXT NULL
        );

        CREATE TABLE IF NOT EXISTS AudioAssetCollections
        (
            AudioAssetId INTEGER NOT NULL,
            CollectionId INTEGER NOT NULL,
            UNIQUE(AudioAssetId, CollectionId)
        );

        CREATE INDEX IF NOT EXISTS IX_AudioAssetCollections_AudioAssetId
            ON AudioAssetCollections(AudioAssetId);

        CREATE INDEX IF NOT EXISTS IX_AudioAssetCollections_CollectionId
            ON AudioAssetCollections(CollectionId);
        """;

            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
}