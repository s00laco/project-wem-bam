using System;
using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using WemBam.Database;
using WemBam.Models;

namespace WemBam.Services
{
    public class SourceManager
    {
        private static readonly SourceManager _instance = new();

        public static SourceManager Instance => _instance;

        public ObservableCollection<Source> Sources { get; } = new();

        private SourceManager()
        {
            LoadSources();
        }

        private void LoadSources()
        {
            Sources.Clear();

            using SqliteConnection connection = DatabaseManager.OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                SELECT
                    Id,
                    DisplayName,
                    Path,
                    SourceType,
                    Enabled,
                    DateAdded
                FROM Sources
                ORDER BY DateAdded ASC;
                """;

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Sources.Add(new Source
                {
                    Id = reader.GetInt64(0),
                    DisplayName = reader.GetString(1),
                    Path = reader.GetString(2),
                    Type = (SourceType)reader.GetInt32(3),
                    Enabled = reader.GetInt64(4) != 0,
                    DateAdded = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(5))
                });
            }
        }

        public bool AddSource(Source source)
        {
            ArgumentNullException.ThrowIfNull(source);

            using SqliteConnection connection = DatabaseManager.OpenConnection();

            using SqliteCommand duplicateCommand = connection.CreateCommand();

            duplicateCommand.CommandText =
                """
                SELECT COUNT(*)
                FROM Sources
                WHERE LOWER(Path) = LOWER($Path);
                """;

            duplicateCommand.Parameters.AddWithValue("$Path", source.Path);

            long duplicateCount = (long)(duplicateCommand.ExecuteScalar() ?? 0);

            if (duplicateCount > 0)
            {
                return false;
            }

            source.Enabled = true;
            source.DateAdded = DateTimeOffset.UtcNow;

            using SqliteCommand insertCommand = connection.CreateCommand();

            insertCommand.CommandText =
                """
                INSERT INTO Sources
                (
                    DisplayName,
                    Path,
                    SourceType,
                    Enabled,
                    DateAdded
                )
                VALUES
                (
                    $DisplayName,
                    $Path,
                    $SourceType,
                    $Enabled,
                    $DateAdded
                );

                SELECT last_insert_rowid();
                """;

            insertCommand.Parameters.AddWithValue("$DisplayName", source.DisplayName);
            insertCommand.Parameters.AddWithValue("$Path", source.Path);
            insertCommand.Parameters.AddWithValue("$SourceType", (int)source.Type);
            insertCommand.Parameters.AddWithValue("$Enabled", 1);
            insertCommand.Parameters.AddWithValue(
                "$DateAdded",
                source.DateAdded.ToUnixTimeMilliseconds());

            source.Id = (long)(insertCommand.ExecuteScalar() ?? 0);

            Sources.Add(source);

            return true;
        }
        public bool RemoveSource(Source source)
        {
            ArgumentNullException.ThrowIfNull(source);

            using SqliteConnection connection = DatabaseManager.OpenConnection();

            using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                """
                DELETE
                FROM Sources
                WHERE Id = $Id;
                """;

            command.Parameters.AddWithValue("$Id", source.Id);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                return false;
            }

            return Sources.Remove(source);
        }
    }
}