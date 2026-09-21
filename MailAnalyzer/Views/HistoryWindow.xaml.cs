using MailAnalyzer.Models;
using MailAnalyzer.Services;
using System.Windows;

namespace MailAnalyzer.Views;

public partial class HistoryWindow : Window
{
    private readonly HistoryService _historyService;

    public HistoryWindow()
    {
        InitializeComponent();

        _historyService = new HistoryService();

        LoadHistory();
    }

    private void LoadHistory()
    {
        HistoryGrid.ItemsSource = _historyService.GetAll();
    }

    private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Вы действительно хотите очистить историю анализа?",
            "Очистка истории",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _historyService.Clear();
            LoadHistory();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}