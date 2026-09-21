namespace MailAnalyzer.Models;

public class AnalysisHistoryItem
{
    public long Id { get; set; }

    public DateTime Date { get; set; }

    public string Source { get; set; } = string.Empty;

    public int Total { get; set; }

    public int Unique { get; set; }
}