using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace FileCopier;

public partial class MainWindow : Window
{
    private string selectedFilePath = string.Empty;
    private string selectedFolderPath = string.Empty;
    private string currentLanguage = "en";
    private bool isDarkTheme = false;

    private readonly Dictionary<string, Dictionary<string, string>> translations = new()
    {
        {
            "en", new Dictionary<string, string>
            {
                { "Title", "File Copier" },
                { "SelectFile", "Select file to copy:" },
                { "SelectFolder", "Select destination folder:" },
                { "Browse", "Browse..." },
                { "Copy", "Copy" },
                { "Clear", "Clear" },
                { "Language", "Language" },
                { "Theme", "Theme" },
                { "Light", "Light" },
                { "Dark", "Dark" },
                { "SelectBoth", "Select file and destination folder." },
                { "FileNotFound", "File not found." },
                { "FileExists", "File '{0}' already exists in this folder.\nOverwrite?" },
                { "Confirm", "Confirmation" },
                { "Copying", "Copying..." },
                { "Success", "✓ File copied successfully!" },
                { "Error", "✗ Error: {0}" },
                { "AccessError", "✗ Access error: {0}" },
                { "FileNoLongerExists", "File no longer exists." }
            }
        },
        {
            "ru", new Dictionary<string, string>
            {
                { "Title", "Копировщик файлов" },
                { "SelectFile", "Выберите файл для копирования:" },
                { "SelectFolder", "Выберите папку назначения:" },
                { "Browse", "Обзор..." },
                { "Copy", "Копировать" },
                { "Clear", "Очистить" },
                { "Language", "Язык" },
                { "Theme", "Тема" },
                { "Light", "Светлая" },
                { "Dark", "Темная" },
                { "SelectBoth", "Выберите файл и папку назначения." },
                { "FileNotFound", "Файл не найден." },
                { "FileExists", "Файл '{0}' уже существует в этой папке.\nПерезаписать?" },
                { "Confirm", "Подтверждение" },
                { "Copying", "Копирование..." },
                { "Success", "✓ Файл успешно скопирован!" },
                { "Error", "✗ Ошибка: {0}" },
                { "AccessError", "✗ Ошибка доступа: {0}" },
                { "FileNoLongerExists", "Файл больше не существует." }
            }
        }
    };

    public MainWindow()
    {
        InitializeComponent();
        UpdateLanguage();
    }

    private string GetText(string key, params object[] args)
    {
        if (translations[currentLanguage].TryGetValue(key, out var text))
            return args.Length > 0 ? string.Format(text, args) : text;
        return key;
    }

    private void UpdateLanguage()
    {
        LanguageMenu.Header = GetText("Language");
        ThemeMenu.Header = GetText("Theme");
        TitleText.Text = GetText("Title");
        SelectFileLabel.Text = GetText("SelectFile");
        SelectFolderLabel.Text = GetText("SelectFolder");
        BrowseFileBtn.Content = GetText("Browse");
        BrowseFolderBtn.Content = GetText("Browse");
        CopyBtn.Content = GetText("Copy");
        ClearBtn.Content = GetText("Clear");
        Title = GetText("Title");
    }

    private void SetEnglish_Click(object sender, RoutedEventArgs e)
    {
        currentLanguage = "en";
        UpdateLanguage();
    }

    private void SetRussian_Click(object sender, RoutedEventArgs e)
    {
        currentLanguage = "ru";
        UpdateLanguage();
    }

    private void SetLightTheme_Click(object sender, RoutedEventArgs e)
    {
        isDarkTheme = false;
        ApplyTheme();
    }

    private void SetDarkTheme_Click(object sender, RoutedEventArgs e)
    {
        isDarkTheme = true;
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        if (isDarkTheme)
        {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 48));
            TitleText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 230));
            SelectFileLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
            SelectFolderLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));

            txtFilePath.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));
            txtFilePath.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 230));
            txtFilePath.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100));

            txtFolderPath.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));
            txtFolderPath.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 230, 230));
            txtFolderPath.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 100));

            progressBar.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 70, 70));
        }
        else
        {
            Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
            TitleText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
            SelectFileLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));
            SelectFolderLabel.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 73, 94));

            txtFilePath.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            txtFilePath.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
            txtFilePath.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 195, 199));

            txtFolderPath.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
            txtFolderPath.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(44, 62, 80));
            txtFolderPath.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(189, 195, 199));

            progressBar.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(236, 240, 241));
        }
    }

    private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
    {
        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            selectedFilePath = openFileDialog.FileName;
            if (!File.Exists(selectedFilePath))
            {
                System.Windows.MessageBox.Show(GetText("FileNoLongerExists"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                selectedFilePath = string.Empty;
                txtFilePath.Text = string.Empty;
                return;
            }
            txtFilePath.Text = selectedFilePath;
            statusText.Text = string.Empty;
        }
    }

    private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
    {
        var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            selectedFolderPath = folderDialog.SelectedPath;
            txtFolderPath.Text = selectedFolderPath;
            statusText.Text = string.Empty;
        }
    }

    private void BtnCopy_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(selectedFilePath) || string.IsNullOrEmpty(selectedFolderPath))
        {
            System.Windows.MessageBox.Show(GetText("SelectBoth"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!File.Exists(selectedFilePath))
        {
            System.Windows.MessageBox.Show(GetText("FileNotFound"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string fileName = Path.GetFileName(selectedFilePath);
        string destPath = Path.Combine(selectedFolderPath, fileName);

        if (File.Exists(destPath))
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(
                GetText("FileExists", fileName),
                GetText("Confirm"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
        }

        try
        {
            progressBar.Visibility = Visibility.Visible;
            statusText.Text = GetText("Copying");

            File.Copy(selectedFilePath, destPath, true);

            progressBar.Visibility = Visibility.Collapsed;
            statusText.Text = GetText("Success");
            statusText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(39, 174, 96));
        }
        catch (IOException ex)
        {
            progressBar.Visibility = Visibility.Collapsed;
            statusText.Text = GetText("Error", ex.Message);
            statusText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
        }
        catch (UnauthorizedAccessException ex)
        {
            progressBar.Visibility = Visibility.Collapsed;
            statusText.Text = GetText("AccessError", ex.Message);
            statusText.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
        }
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        selectedFilePath = string.Empty;
        selectedFolderPath = string.Empty;
        txtFilePath.Text = string.Empty;
        txtFolderPath.Text = string.Empty;
        statusText.Text = string.Empty;
        progressBar.Visibility = Visibility.Collapsed;
    }
}