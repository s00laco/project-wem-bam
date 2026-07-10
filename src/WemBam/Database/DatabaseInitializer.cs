using Microsoft.Data.Sqlite;

namespace WemBam.Database
{
    public static class DatabaseInitializer
    {
        public static void Initialize(SqliteConnection connection)
        {
            int currentVersion = GetSchemaVersion(connection);

            if (currentVersion == 0)
            {
                CreateVersion1(connection);

                currentVersion = GetSchemaVersion(connection);
            }

            ApplyFutureMigrations(connection, currentVersion);
        }

        private static int GetSchemaVersion(SqliteConnection connection)
        {
            using SqliteCommand command = connection.CreateCommand();

            command.CommandText = "PRAGMA user_version;";

            object? result = command.ExecuteScalar();

            if (result is long version)
            {
                return (int)version;
            }

            return 0;
        }

        private static void CreateVersion1(SqliteConnection connection)
        {
            DatabaseSchema.CreateVersion1(connection);

            SetSchemaVersion(
                connection,
                DatabaseConstants.CurrentSchemaVersion);
        }

        private static void SetSchemaVersion(
            SqliteConnection connection,
            int version)
        {
            using SqliteCommand command = connection.CreateCommand();

            command.CommandText = $"PRAGMA user_version = {version};";

            command.ExecuteNonQuery();
        }
        private static void ApplyFutureMigrations(
            SqliteConnection connection,
            int currentVersion)
        {
            if (currentVersion < 2)
            {
                DatabaseSchema.UpgradeToVersion2(connection);

                SetSchemaVersion(connection, 2);

                currentVersion = 2;
            }
        }
    }
}