using MailAnalyzer.Models;
using Microsoft.Data.Sqlite;
using System.IO;

namespace MailAnalyzer.Services;

public class SettingsService
{
    private readonly string _connectionString;

    public SettingsService()
    {
        string databasePath = Path.Combine(
            AppContext.BaseDirectory,
            "mailanalyzer.db");

        _connectionString = $"Data Source={databasePath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Settings
            (
                Id INTEGER PRIMARY KEY,
                CaseSensitive INTEGER NOT NULL,
                SortResults INTEGER NOT NULL,
                DefaultSavePath TEXT NOT NULL,
                DefaultFileFormat TEXT NOT NULL
            );
            """;

        command.ExecuteNonQuery();
    }

    public AppSettings Load()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT CaseSensitive,
                   SortResults,
                   DefaultSavePath,
                   DefaultFileFormat
            FROM Settings
            WHERE Id = 1;
            """;

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new AppSettings
            {
                CaseSensitive = reader.GetInt32(0) != 0,
                SortResults = reader.GetInt32(1) != 0,
                DefaultSavePath = reader.GetString(2),
                DefaultFileFormat = reader.GetString(3)
            };
        }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Settings
                (Id, CaseSensitive, SortResults, DefaultSavePath, DefaultFileFormat)
            VALUES
                (1, $caseSensitive, $sortResults, $defaultSavePath, $defaultFileFormat)
            ON CONFLICT(Id) DO UPDATE SET
                CaseSensitive = excluded.CaseSensitive,
                SortResults = excluded.SortResults,
                DefaultSavePath = excluded.DefaultSavePath,
                DefaultFileFormat = excluded.DefaultFileFormat;
            """;

        command.Parameters.AddWithValue(
            "$caseSensitive",
            settings.CaseSensitive ? 1 : 0);

        command.Parameters.AddWithValue(
            "$sortResults",
            settings.SortResults ? 1 : 0);

        command.Parameters.AddWithValue(
            "$defaultSavePath",
            settings.DefaultSavePath);

        command.Parameters.AddWithValue(
            "$defaultFileFormat",
            settings.DefaultFileFormat);

        command.ExecuteNonQuery();
    }

    public void Reset()
    {
        Save(new AppSettings());
    }
}