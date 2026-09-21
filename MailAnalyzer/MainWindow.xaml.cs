using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace MailAnalyzer;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<EmailResult> _results = new();
    private readonly ICollectionView _resultsView;
    private string _currentSource = "Ручной ввод";
    private int _totalCount;

    public MainWindow()
    {
        InitializeComponent();

        ResultsGrid.ItemsSource = _results;

        _resultsView = CollectionViewSource.GetDefaultView(_results);

        UpdateCounters();
    }

    // Вставка из буфера

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Clipboard.ContainsText())
        {
            MessageBox.Show(
                "Буфер обмена не содержит текста.",
                "Буфер обмена",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        InputTextBox.Text = Clipboard.GetText();
        InputTextBox.Focus();
        InputTextBox.CaretIndex = InputTextBox.Text.Length;
    }

    // Загрузка TXT

    private void LoadFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Выберите текстовый файл",
            Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            _currentSource = Path.GetFileName(dialog.FileName);

            InputTextBox.Text = File.ReadAllText(
                dialog.FileName,
                Encoding.UTF8);

            MessageBox.Show(
                $"Файл «{_currentSource}» успешно загружен.",
                "Загрузка файла",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось прочитать файл.\n\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // Очистка

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        InputTextBox.Clear();
        _results.Clear();

        _currentSource = "Ручной ввод";

        UpdateCounters();
    }

    // Анализ

    private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        string text = InputTextBox.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            MessageBox.Show(
                "Введите или вставьте текст для анализа.",
                "Нет данных",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            InputTextBox.Focus();
            return;
        }

        _results.Clear();

        string emailPattern =
            "[A-Za-z0-9.!#$%&'*+/=?^_`{|}~-]+@" +
            "[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?" +
            "(?:\\.[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?)+";

        MatchCollection matches = Regex.Matches(
            text,
            emailPattern,
            RegexOptions.IgnoreCase);

        _totalCount = matches.Count;

        var uniqueEmails = matches
            .Select(match => match.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(email => email, StringComparer.OrdinalIgnoreCase)
            .ToList();

        int number = 1;

        foreach (string email in uniqueEmails)
        {
            _results.Add(new EmailResult
            {
                Number = number++,
                Email = email,
                Source = _currentSource
            });
        }

        UpdateCounters();
    }


    // Копирование

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_results.Count == 0)
        {
            MessageBox.Show(
                "Нет результатов для копирования.",
                "Копирование",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (ResultsGrid.SelectedItems.Count == 0)
        {
            MessageBox.Show(
                "Выберите один или несколько e-mail адресов.",
                "Копирование",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var selectedResults = ResultsGrid.SelectedItems
            .OfType<EmailResult>()
            .ToList();

        var text = string.Join(
            Environment.NewLine,
            selectedResults.Select(result => result.Email));

        Clipboard.SetText(text);

        MessageBox.Show(
            $"Скопировано адресов: {selectedResults.Count}.",
            "Копирование",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    // Экспорт TXT

    private void ExportTxtButton_Click(object sender, RoutedEventArgs e)
    {
        if (_results.Count == 0)
        {
            MessageBox.Show(
                "Нет результатов для экспорта.",
                "MailAnalyzer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Сохранить результаты",
            Filter = "Текстовые файлы (*.txt)|*.txt",
            DefaultExt = ".txt",
            FileName = "emails.txt"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var text = string.Join(
                Environment.NewLine,
                _results.Select(result => result.Email));

            File.WriteAllText(
                dialog.FileName,
                text,
                Encoding.UTF8);

            MessageBox.Show(
                "Результаты успешно сохранены.",
                "MailAnalyzer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось сохранить файл.\n\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // Экспорт CSV

    private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
    {
        if (_results.Count == 0)
        {
            MessageBox.Show(
                "Нет результатов для экспорта.",
                "MailAnalyzer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Сохранить результаты",
            Filter = "CSV-файлы (*.csv)|*.csv",
            DefaultExt = ".csv",
            FileName = "emails.csv"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var builder = new StringBuilder();

            builder.AppendLine("№;E-mail адрес;Источник");

            foreach (var result in _results)
            {
                builder.AppendLine(
                    $"{result.Number};{result.Email};{result.Source}");
            }

            File.WriteAllText(
                dialog.FileName,
                builder.ToString(),
                Encoding.UTF8);

            MessageBox.Show(
                "CSV-файл успешно сохранён.",
                "MailAnalyzer",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось сохранить CSV-файл.\n\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // Почтовый клиент

    private void MailClientButton_Click(object sender, RoutedEventArgs e)
    {
        if (_results.Count == 0)
        {
            MessageBox.Show(
                "Нет результатов для открытия почтового клиента.",
                "Почтовый клиент",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (ResultsGrid.SelectedItems.Count == 0)
        {
            MessageBox.Show(
                "Выберите один или несколько e-mail адресов.",
                "Почтовый клиент",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        var selectedResults = ResultsGrid.SelectedItems
            .OfType<EmailResult>()
            .ToList();

        string recipients = string.Join(
            ",",
            selectedResults.Select(result => result.Email));

        string subject = SubjectTextBox.Text.Trim();
        string body = BodyTextBox.Text;

        string mailto =
            $"mailto:{recipients}" +
            $"?subject={Uri.EscapeDataString(subject)}" +
            $"&body={Uri.EscapeDataString(body)}";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = mailto,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось открыть почтовый клиент.\n\n{ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // Изменение текста

    private void InputTextBox_TextChanged(
    object sender,
    System.Windows.Controls.TextChangedEventArgs e)
    {
        bool hasText = !string.IsNullOrEmpty(InputTextBox.Text);

        InputPlaceholder.Visibility = hasText
            ? Visibility.Collapsed
            : Visibility.Visible;

        int characterCount = InputTextBox.Text.Length;

        CharacterCountText.Text = $"{characterCount:N0} {GetCharacterWordForm(characterCount)}";
    }

    // Поиск по результатам
    private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        string searchText = SearchTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            _resultsView.Filter = null;
        }
        else
        {
            _resultsView.Filter = item =>
            {
                if (item is not EmailResult result)
                    return false;

                return result.Email.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase);
            };
        }

        _resultsView.Refresh();
    }

    private static string GetCharacterWordForm(int count)
    {
        int lastTwoDigits = count % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return "символов";

        return (count % 10) switch
        {
            1 => "символ",
            2 or 3 or 4 => "символа",
            _ => "символов"
        };
    }

    // Счётчики

    private void UpdateCounters()
    {
        int totalCount = _results.Count;

        int uniqueCount = _results
            .Select(x => x.Email)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        TotalCountText.Text = _totalCount.ToString();
        UniqueCountText.Text = uniqueCount.ToString();
    }

    // Временная модель результата

    private class EmailResult
    {
        public int Number { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;
    }
}