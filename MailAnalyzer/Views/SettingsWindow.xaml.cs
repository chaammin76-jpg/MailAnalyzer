using MailAnalyzer.Models;
using MailAnalyzer.Services;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace MailAnalyzer.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsService _settingsService;
    private AppSettings _settings;

    public SettingsWindow()
    {
        InitializeComponent();

        _settingsService = new SettingsService();
        _settings = _settingsService.Load();

        LoadSettings();
    }

    private void LoadSettings()
    {
        CaseSensitiveCheckBox.IsChecked = _settings.CaseSensitive;
        SortResultsCheckBox.IsChecked = _settings.SortResults;
        DefaultSavePathTextBox.Text = _settings.DefaultSavePath;

        foreach (ComboBoxItem item in DefaultFormatComboBox.Items)
        {
            if (item.Content?.ToString() == _settings.DefaultFileFormat)
            {
                DefaultFormatComboBox.SelectedItem = item;
                break;
            }
        }

        if (DefaultFormatComboBox.SelectedItem == null)
        {
            DefaultFormatComboBox.SelectedIndex = 0;
        }
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Выберите папку для сохранения результатов",
            InitialDirectory = DefaultSavePathTextBox.Text
        };

        if (dialog.ShowDialog() == true)
        {
            DefaultSavePathTextBox.Text = dialog.FolderName;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        string savePath = DefaultSavePathTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(savePath))
        {
            MessageBox.Show(
                "Укажите путь для сохранения результатов.",
                "Настройки",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (!System.IO.Directory.Exists(savePath))
        {
            MessageBox.Show(
                "Указанная папка не существует.",
                "Настройки",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        string format =
            (DefaultFormatComboBox.SelectedItem as ComboBoxItem)?
            .Content?
            .ToString() ?? ".txt";

        _settings = new AppSettings
        {
            CaseSensitive = CaseSensitiveCheckBox.IsChecked == true,
            SortResults = SortResultsCheckBox.IsChecked == true,
            DefaultSavePath = savePath,
            DefaultFileFormat = format
        };

        _settingsService.Save(_settings);

        MessageBox.Show(
            "Настройки сохранены.",
            "MailAnalyzer",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        DialogResult = true;
        Close();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Восстановить настройки по умолчанию?",
            "Сброс настроек",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        _settingsService.Reset();
        _settings = _settingsService.Load();

        LoadSettings();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}