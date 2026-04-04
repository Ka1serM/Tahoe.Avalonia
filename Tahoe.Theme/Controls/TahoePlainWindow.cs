using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace Avalonia.Themes.Tahoe.Controls;

public partial class TahoePlainWindow : Window
{
    private const string ChromeHostName = "TahoeChromeHost";
    private static readonly CornerRadius DefaultCornerRadius = new(22);
    private bool _isWrappingContent;

    protected virtual bool WrapContentInTahoeChrome => true;

    public TahoePlainWindow()
    {
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        SystemDecorations = SystemDecorations.None;
        ExtendClientAreaToDecorationsHint = false;
        ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
        Background = Brushes.Transparent;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        EnsureChromeWrapper();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ContentProperty)
            EnsureChromeWrapper();
    }

    private void EnsureChromeWrapper()
    {
        if (!WrapContentInTahoeChrome || _isWrappingContent || Content is null)
            return;

        if (Content is Border { Name: ChromeHostName })
            return;

        var content = Content;

        _isWrappingContent = true;
        try
        {
            // Detach first so an existing visual can be safely re-parented into the chrome border.
            Content = null;

            var wrappedContent = content as Control ?? new ContentControl { Content = content };
            var chrome = new Border
            {
                Name = ChromeHostName,
                CornerRadius = DefaultCornerRadius,
                ClipToBounds = true,
                Background = ResolveChromeBackground(),
                Child = wrappedContent
            };

            Content = chrome;
        }
        finally
        {
            _isWrappingContent = false;
        }
    }

    private IBrush ResolveChromeBackground()
    {
        if (Application.Current?.TryGetResource("Background", ActualThemeVariant, out var resource) == true &&
            resource is IBrush brush)
        {
            return brush;
        }

        return Brushes.Transparent;
    }
}
