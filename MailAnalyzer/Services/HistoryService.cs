using MailAnalyzer.Models;
using Microsoft.Data.Sqlite;
using System.IO;

namespace MailAnalyzer.Services;

public class HistoryService
{
    private readonly string _connectionString;

    public HistoryService()
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
            CREATE TABLE IF NOT EXISTS AnalysisHistory
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL,
                Source TEXT NOT NULL,
                Total INTEGER NOT NULL,
                UniqueCount INTEGER NOT NULL
            );
            """;

        command.ExecuteNonQuery();
    }

    public void Add(AnalysisHistoryItem item)
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO AnalysisHistory
                (Date, Source, Total, UniqueCount)
            VALUES
                ($date, $source, $total, $unique);
            """;

        command.Parameters.AddWithValue(
            "$date",
            item.Date.ToString("O"));

        command.Parameters.AddWithValue(
            "$source",
            item.Source);

        command.Parameters.AddWithValue(
            "$total",
            item.Total);

        command.Parameters.AddWithValue(
            "$unique",
            item.Unique);

        command.ExecuteNonQuery();
    }

    public List<AnalysisHistoryItem> GetAll()
    {
        var result = new List<AnalysisHistoryItem>();

        using var connection = new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, Date, Source, Total, UniqueCount
            FROM AnalysisHistory
            ORDER BY Date DESC;
            """;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new AnalysisHistoryItem
            {
                Id = reader.GetInt64(0),
                Date = DateTime.Parse(reader.GetString(1)),
                Source = reader.GetString(2),
                Total = reader.GetInt32(3),
                Unique = reader.GetInt32(4)
            });
        }

        return result;
    }

    public void Clear()
    {
        using var connection = new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM AnalysisHistory;";

        command.ExecuteNonQuery();
    }
}