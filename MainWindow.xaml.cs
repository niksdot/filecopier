using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using LinearGradientBrush = System.Windows.Media.LinearGradientBrush;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace FileCopier;

/// <summary>
/// Attached property that gives a templated button its coloured glow. The effect is
/// applied to a dedicated shadow layer rendered *behind* the button surface, so the
/// button label itself stays outside the effect and keeps crisp ClearType rendering.
/// </summary>
public static class Effects
{
    public static readonly DependencyProperty GlowProperty = DependencyProperty.RegisterAttached(
        "Glow", typeof(Effect), typeof(Effects), new PropertyMetadata(null));

    public static void SetGlow(DependencyObject element, Effect? value) =>
        element.SetValue(GlowProperty, value);

    public static Effect? GetGlow(DependencyObject element) =>
        (Effect?)element.GetValue(GlowProperty);
}

public partial class MainWindow : Window
{
    private string selectedFilePath = string.Empty;
    private string selectedFolderPath = string.Empty;
    private string currentLanguage = "en";
    private bool isDarkTheme;
    private bool isBusy;
    private bool uiReady;

    /// <summary>Every brush that is swapped when the theme changes.</summary>
    private static readonly string[] PaletteKeys =
    {
        "WindowBackgroundBrush",
        "Orb1Brush", "Orb2Brush", "Orb3Brush",
        "CardBackgroundBrush", "CardBorderBrush", "CardGlossBrush",
        "TitleGradientBrush",
        "TextPrimaryBrush", "TextSecondaryBrush",
        "InputBrush", "InputBorderBrush", "FocusBorderBrush", "InputIconTintBrush", "InputIconBrush",
        "AccentBrush",
        "PrimaryBrush", "PrimaryHoverBrush", "PrimaryPressedBrush",
        "TintBrush", "TintHoverBrush", "TintPressedBrush", "TintTextBrush",
        "NeutralBrush", "NeutralHoverBrush", "NeutralPressedBrush", "NeutralTextBrush",
        "SegmentTrackBrush", "SegmentHoverBrush", "SegmentActiveBrush", "SegmentTextBrush", "SegmentActiveTextBrush",
        "ProgressTrackBrush", "SuccessTextBrush", "ErrorTextBrush", "InfoTextBrush",
        "SuccessTintBrush", "SuccessBorderBrush", "ErrorTintBrush", "ErrorBorderBrush",
        "InfoTintBrush", "InfoBorderBrush",
        "WindowButtonHoverBrush", "WindowButtonPressedBrush", "CloseHoverBrush", "ClosePressedBrush",
        "WindowButtonGlyphBrush", "OnAccentBrush",
        "FocusGlowEffect", "LogoGlowEffect", "SegmentActiveGlowEffect"
    };

    /// <summary>Light palette captured from App.xaml so the two themes never drift apart.</summary>
    private readonly Dictionary<string, object> lightPalette = new();
    private readonly Dictionary<string, object> darkPalette = new();

    private readonly Dictionary<string, Dictionary<string, string>> translations = new()
    {
        {
            "en", new Dictionary<string, string>
            {
                { "Title", "File Copier" },
                { "Subtitle", "Fast and safe file copying" },
                { "SelectFile", "Select file to copy:" },
                { "SelectFolder", "Select destination folder:" },
                { "Browse", "Browse..." },
                { "Copy", "Copy" },
                { "Clear", "Clear" },
                { "Light", "Light theme" },
                { "Dark", "Dark theme" },
                { "Minimize", "Minimize" },
                { "Maximize", "Maximize" },
                { "Restore", "Restore" },
                { "Close", "Close" },
                { "SelectBoth", "Select file and destination folder." },
                { "FileNotFound", "File not found." },
                { "FolderNotFound", "Destination folder not found." },
                { "SameLocation", "Source and destination are the same file." },
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
                { "Subtitle", "Быстрое и безопасное копирование файлов" },
                { "SelectFile", "Выберите файл для копирования:" },
                { "SelectFolder", "Выберите папку назначения:" },
                { "Browse", "Обзор..." },
                { "Copy", "Копировать" },
                { "Clear", "Очистить" },
                { "Light", "Светлая тема" },
                { "Dark", "Тёмная тема" },
                { "Minimize", "Свернуть" },
                { "Maximize", "Развернуть" },
                { "Restore", "Восстановить" },
                { "Close", "Закрыть" },
                { "SelectBoth", "Выберите файл и папку назначения." },
                { "FileNotFound", "Файл не найден." },
                { "FolderNotFound", "Папка назначения не найдена." },
                { "SameLocation", "Исходный файл и файл назначения совпадают." },
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

        CaptureLightPalette();
        BuildDarkPalette();
        UpdateLanguage();
        ApplyTheme();

        uiReady = true;
    }

    #region Localization

    private string GetText(string key, params object[] args)
    {
        if (translations[currentLanguage].TryGetValue(key, out var text))
            return args.Length > 0 ? string.Format(text, args) : text;
        return key;
    }

    private void UpdateLanguage()
    {
        string title = GetText("Title");

        Title = title;
        TitleBarText.Text = title;
        TitleText.Text = title;
        SubtitleText.Text = GetText("Subtitle");
        SelectFileLabel.Text = GetText("SelectFile");
        SelectFolderLabel.Text = GetText("SelectFolder");
        BrowseFileText.Text = GetText("Browse");
        BrowseFolderText.Text = GetText("Browse");
        CopyText.Text = GetText("Copy");
        ClearText.Text = GetText("Clear");

        LangEnBtn.ToolTip = "English";
        LangRuBtn.ToolTip = "Русский";
        LightThemeBtn.ToolTip = GetText("Light");
        DarkThemeBtn.ToolTip = GetText("Dark");
        MinimizeBtn.ToolTip = GetText("Minimize");
        MaximizeBtn.ToolTip = GetText(WindowState == WindowState.Maximized ? "Restore" : "Maximize");
        CloseBtn.ToolTip = GetText("Close");

        LangEnBtn.IsChecked = currentLanguage == "en";
        LangRuBtn.IsChecked = currentLanguage == "ru";
        LightThemeBtn.IsChecked = !isDarkTheme;
        DarkThemeBtn.IsChecked = isDarkTheme;
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

    #endregion

    #region Theme

    private void SetLightTheme_Click(object sender, RoutedEventArgs e)
    {
        isDarkTheme = false;
        ApplyTheme();
        UpdateLanguage();
    }

    private void SetDarkTheme_Click(object sender, RoutedEventArgs e)
    {
        isDarkTheme = true;
        ApplyTheme();
        UpdateLanguage();
    }

    private void CaptureLightPalette()
    {
        var resources = Application.Current.Resources;
        foreach (var key in PaletteKeys)
        {
            if (resources[key] is { } value)
                lightPalette[key] = value;
        }
    }

    private void BuildDarkPalette()
    {
        darkPalette["WindowBackgroundBrush"] = Gradient3("#080B12", "#0E1526", "#141D33");

        darkPalette["Orb1Brush"] = Radial("#7A6366F1", "#006366F1");
        darkPalette["Orb2Brush"] = Radial("#66D946EF", "#00D946EF");
        darkPalette["Orb3Brush"] = Radial("#5938BDF8", "#0038BDF8");

        darkPalette["CardBackgroundBrush"] = Solid("#151A26");
        darkPalette["CardBorderBrush"] = Solid("#262E41");

        darkPalette["CardGlossBrush"] = VGradient("#14FFFFFF", "#00FFFFFF");

        darkPalette["TitleGradientBrush"] = Gradient3("#A5B4FC", "#C4B5FD", "#F0ABFC");

        darkPalette["TextPrimaryBrush"] = Solid("#E9EEF8");
        darkPalette["TextSecondaryBrush"] = Solid("#94A1B9");

        darkPalette["InputBrush"] = Solid("#1C2230");
        darkPalette["InputBorderBrush"] = Solid("#2C3548");
        darkPalette["FocusBorderBrush"] = Solid("#8B8CFF");
        darkPalette["InputIconTintBrush"] = Solid("#252E48");
        darkPalette["InputIconBrush"] = Solid("#9AA7FF");

        darkPalette["AccentBrush"] = Gradient("#7C7CF8", "#A855F7");

        darkPalette["PrimaryBrush"] = Gradient3("#7C7CF8", "#9F6FF8", "#DA5CF0");
        darkPalette["PrimaryHoverBrush"] = Gradient3("#8F8FFF", "#B285FA", "#E678F5");
        darkPalette["PrimaryPressedBrush"] = Gradient3("#6366F1", "#8B5CF6", "#C026D3");

        darkPalette["TintBrush"] = Solid("#232B42");
        darkPalette["TintHoverBrush"] = Solid("#2C3654");
        darkPalette["TintPressedBrush"] = Solid("#364265");
        darkPalette["TintTextBrush"] = Solid("#C7D2FE");

        darkPalette["NeutralBrush"] = Gradient("#242C3D", "#1E2533");
        darkPalette["NeutralHoverBrush"] = Gradient("#2D3749", "#262F41");
        darkPalette["NeutralPressedBrush"] = Gradient("#384358", "#2F3949");
        darkPalette["NeutralTextBrush"] = Solid("#DCE3F0");

        darkPalette["SegmentTrackBrush"] = Solid("#1B2231");
        darkPalette["SegmentHoverBrush"] = Solid("#263044");
        darkPalette["SegmentActiveBrush"] = Solid("#323D57");
        darkPalette["SegmentTextBrush"] = Solid("#8C99B0");
        darkPalette["SegmentActiveTextBrush"] = Solid("#C7D2FE");

        darkPalette["ProgressTrackBrush"] = Solid("#222A39");
        darkPalette["SuccessTextBrush"] = Solid("#34D399");
        darkPalette["ErrorTextBrush"] = Solid("#F87171");
        darkPalette["InfoTextBrush"] = Solid("#A5B4FC");

        darkPalette["SuccessTintBrush"] = Solid("#0E2A22");
        darkPalette["SuccessBorderBrush"] = Solid("#1D5140");
        darkPalette["ErrorTintBrush"] = Solid("#2A1719");
        darkPalette["ErrorBorderBrush"] = Solid("#5B2A2F");
        darkPalette["InfoTintBrush"] = Solid("#1B2136");
        darkPalette["InfoBorderBrush"] = Solid("#33406B");

        darkPalette["WindowButtonHoverBrush"] = Solid("#222A39");
        darkPalette["WindowButtonPressedBrush"] = Solid("#2C3648");
        darkPalette["CloseHoverBrush"] = Solid("#EF4444");
        darkPalette["ClosePressedBrush"] = Solid("#DC2626");
        darkPalette["WindowButtonGlyphBrush"] = Solid("#8E9BB2");
        darkPalette["OnAccentBrush"] = Solid("#FFFFFF");

        darkPalette["FocusGlowEffect"] = Glow("#8B8CFF", blur: 12, depth: 0, opacity: 0.42);
        darkPalette["LogoGlowEffect"] = Glow("#A855F7", blur: 14, depth: 2, opacity: 0.55);
        darkPalette["SegmentActiveGlowEffect"] = Glow("#000000", blur: 8, depth: 1, opacity: 0.35);
    }

    private void ApplyTheme()
    {
        var resources = Application.Current.Resources;

        foreach (var key in PaletteKeys)
        {
            object? value = isDarkTheme
                ? (darkPalette.TryGetValue(key, out var dark) ? dark : null)
                : (lightPalette.TryGetValue(key, out var light) ? light : null);

            if (value is not null)
                resources[key] = value;
        }
    }

    private static SolidColorBrush Solid(string hex)
    {
        var brush = new SolidColorBrush(Parse(hex));
        brush.Freeze();
        return brush;
    }

    private static LinearGradientBrush Gradient(string from, string to)
    {
        var brush = new LinearGradientBrush(Parse(from), Parse(to), new Point(0, 0), new Point(1, 1));
        brush.Freeze();
        return brush;
    }

    private static LinearGradientBrush Gradient3(string from, string middle, string to)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };
        brush.GradientStops.Add(new GradientStop(Parse(from), 0));
        brush.GradientStops.Add(new GradientStop(Parse(middle), 0.52));
        brush.GradientStops.Add(new GradientStop(Parse(to), 1));
        brush.Freeze();
        return brush;
    }

    private static LinearGradientBrush VGradient(string from, string to)
    {
        var brush = new LinearGradientBrush(Parse(from), Parse(to), new Point(0, 0), new Point(0, 1));
        brush.Freeze();
        return brush;
    }

    private static RadialGradientBrush Radial(string centerColor, string edgeColor)
    {
        var brush = new RadialGradientBrush();
        brush.GradientStops.Add(new GradientStop(Parse(centerColor), 0));
        brush.GradientStops.Add(new GradientStop(Parse(edgeColor), 1));
        brush.Freeze();
        return brush;
    }

    private static Color Parse(string hex) =>
        (Color)System.Windows.Media.ColorConverter.ConvertFromString(hex)!;

    private static DropShadowEffect Glow(string hex, double blur, double depth, double opacity)
    {
        var effect = new DropShadowEffect
        {
            Color = Parse(hex),
            BlurRadius = blur,
            ShadowDepth = depth,
            Direction = 270,
            Opacity = opacity
        };
        effect.Freeze();
        return effect;
    }

    #endregion

    #region Startup polish

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int sizeBytes);

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Rounded window corners on Windows 11 (silently ignored on older systems).
        try
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int preference = 2; // DWMWCP_ROUND
            _ = DwmSetWindowAttribute(hwnd, 33, ref preference, sizeof(int));
        }
        catch
        {
            // DWM not available — keep square corners.
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            RootSurface.BeginAnimation(UIElement.OpacityProperty, fade);

            var slide = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(650))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            CardShift.BeginAnimation(TranslateTransform.YProperty, slide);
        }
        catch
        {
            RootSurface.Opacity = 1;
        }
    }

    #endregion

    #region Window chrome

    private void Minimize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (!uiReady)
            return;

        bool maximized = WindowState == WindowState.Maximized;
        MaxIcon.Visibility = maximized ? Visibility.Collapsed : Visibility.Visible;
        RestoreIcon.Visibility = maximized ? Visibility.Visible : Visibility.Collapsed;
        MaximizeBtn.ToolTip = GetText(maximized ? "Restore" : "Maximize");
    }

    #endregion

    #region Status feedback

    private enum StatusKind
    {
        Info,
        Success,
        Error
    }

    private void ShowStatus(string text, StatusKind kind)
    {
        if (string.IsNullOrEmpty(text))
        {
            StatusPanel.Visibility = Visibility.Collapsed;
            statusText.Text = string.Empty;
            return;
        }

        string textKey;
        string tintKey;
        string borderKey;

        switch (kind)
        {
            case StatusKind.Error:
                textKey = "ErrorTextBrush";
                tintKey = "ErrorTintBrush";
                borderKey = "ErrorBorderBrush";
                break;
            case StatusKind.Success:
                textKey = "SuccessTextBrush";
                tintKey = "SuccessTintBrush";
                borderKey = "SuccessBorderBrush";
                break;
            default:
                textKey = "InfoTextBrush";
                tintKey = "InfoTintBrush";
                borderKey = "InfoBorderBrush";
                break;
        }

        statusText.Text = text;
        statusText.SetResourceReference(TextBlock.ForegroundProperty, textKey);
        StatusPanel.SetResourceReference(Border.BackgroundProperty, tintKey);
        StatusPanel.SetResourceReference(Border.BorderBrushProperty, borderKey);
        StatusPanel.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        isBusy = busy;
        CopyBtn.IsEnabled = !busy;
        ClearBtn.IsEnabled = !busy;
        BrowseFileBtn.IsEnabled = !busy;
        BrowseFolderBtn.IsEnabled = !busy;
    }

    #endregion

    #region Actions

    private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            selectedFilePath = openFileDialog.FileName;
            if (!File.Exists(selectedFilePath))
            {
                MessageBox.Show(GetText("FileNoLongerExists"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                selectedFilePath = string.Empty;
                txtFilePath.Text = string.Empty;
                return;
            }
            txtFilePath.Text = selectedFilePath;
            ShowStatus(string.Empty, StatusKind.Info);
        }
    }

    private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
    {
        using var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            selectedFolderPath = folderDialog.SelectedPath;
            txtFolderPath.Text = selectedFolderPath;
            ShowStatus(string.Empty, StatusKind.Info);
        }
    }

    private async void BtnCopy_Click(object sender, RoutedEventArgs e)
    {
        if (isBusy)
            return;

        if (string.IsNullOrEmpty(selectedFilePath) || string.IsNullOrEmpty(selectedFolderPath))
        {
            MessageBox.Show(GetText("SelectBoth"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!File.Exists(selectedFilePath))
        {
            MessageBox.Show(GetText("FileNotFound"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!Directory.Exists(selectedFolderPath))
        {
            MessageBox.Show(GetText("FolderNotFound"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string fileName = Path.GetFileName(selectedFilePath);
        string destPath = Path.Combine(selectedFolderPath, fileName);

        if (string.Equals(
                Path.GetFullPath(selectedFilePath),
                Path.GetFullPath(destPath),
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(GetText("SameLocation"), GetText("Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (File.Exists(destPath))
        {
            MessageBoxResult result = MessageBox.Show(
                GetText("FileExists", fileName),
                GetText("Confirm"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;
        }

        try
        {
            SetBusy(true);
            progressBar.Visibility = Visibility.Visible;
            ShowStatus(GetText("Copying"), StatusKind.Info);

            await Task.Run(() => File.Copy(selectedFilePath, destPath, true));

            ShowStatus(GetText("Success"), StatusKind.Success);
        }
        catch (IOException ex)
        {
            ShowStatus(GetText("Error", ex.Message), StatusKind.Error);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowStatus(GetText("AccessError", ex.Message), StatusKind.Error);
        }
        catch (Exception ex)
        {
            ShowStatus(GetText("Error", ex.Message), StatusKind.Error);
        }
        finally
        {
            progressBar.Visibility = Visibility.Collapsed;
            SetBusy(false);
        }
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        selectedFilePath = string.Empty;
        selectedFolderPath = string.Empty;
        txtFilePath.Text = string.Empty;
        txtFolderPath.Text = string.Empty;
        ShowStatus(string.Empty, StatusKind.Info);
        progressBar.Visibility = Visibility.Collapsed;
    }

    #endregion
}
