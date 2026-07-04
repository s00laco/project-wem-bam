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
    }
}