namespace MailAnalyzer.Models;

public class AppSettings
{
    public bool CaseSensitive { get; set; } = false;

    public bool SortResults { get; set; } = true;

    public string DefaultSavePath { get; set; } = AppContext.BaseDirectory;

    public string DefaultFileFormat { get; set; } = ".txt";
}