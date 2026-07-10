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
    }
}