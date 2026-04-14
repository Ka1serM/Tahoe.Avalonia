using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;

namespace Avalonia.Themes.Tahoe.Controls;

/// <summary>
/// A window chrome control that provides a title bar, window buttons, rounded corners, and resize borders.
/// Place as the sole content of a Window with SystemDecorations=None.
/// </summary>
public class TahoeWindow : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<TahoeWindow, string?>(nameof(Title));

    public static readonly StyledProperty<bool> IsWindowsProperty =
        AvaloniaProperty.Register<TahoeWindow, bool>(nameof(IsWindows), true);

    public static readonly StyledProperty<bool> IsLinuxProperty =
        AvaloniaProperty.Register<TahoeWindow, bool>(nameof(IsLinux));

    public static readonly StyledProperty<bool> ButtonsVisibleProperty =
        AvaloniaProperty.Register<TahoeWindow, bool>(nameof(ButtonsVisible), true);

    public static readonly StyledProperty<bool> TitleBarVisibleProperty =
        AvaloniaProperty.Register<TahoeWindow, bool>(nameof(TitleBarVisible), true);

    private Border? _titleBar;
    private TextBlock? _titleText;
    private StackPanel? _winButtons;
    private StackPanel? _linButtons;

    private Button? _winClose;
    private Button? _winMinimize;
    private Button? _winMaximize;
    private Button? _linClose;
    private Button? _linMinimize;
    private Button? _linMaximize;

    private readonly Border?[] _resizeBorders = new Border?[8];

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsWindows
    {
        get => GetValue(IsWindowsProperty);
        set => SetValue(IsWindowsProperty, value);
    }

    public bool IsLinux
    {
        get => GetValue(IsLinuxProperty);
        set => SetValue(IsLinuxProperty, value);
    }

    public bool ButtonsVisible
    {
        get => GetValue(ButtonsVisibleProperty);
        set => SetValue(ButtonsVisibleProperty, value);
    }

    /// <summary>
    /// Shows or hides the entire title bar (drag area + buttons + title text).
    /// </summary>
    public bool TitleBarVisible
    {
        get => GetValue(TitleBarVisibleProperty);
        set => SetValue(TitleBarVisibleProperty, value);
    }

    protected Window? ParentWindow => this.FindAncestorOfType<Window>();

    static TahoeWindow()
    {
        FocusableProperty.OverrideDefaultValue<TahoeWindow>(false);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TitleBarVisibleProperty ||
            change.Property == ButtonsVisibleProperty ||
            change.Property == IsWindowsProperty ||
            change.Property == IsLinuxProperty ||
            change.Property == TitleProperty)
        {
            UpdateTitleBarVisibility();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UnhookTemplateEvents();

        _titleBar = e.NameScope.Find<Border>("PART_TitleBar");
        _titleText = e.NameScope.Find<TextBlock>("PART_TitleText");
        _winButtons = e.NameScope.Find<StackPanel>("PART_WinButtons");
        _linButtons = e.NameScope.Find<StackPanel>("PART_LinButtons");

        _winClose = e.NameScope.Find<Button>("PART_WinClose");
        _winMinimize = e.NameScope.Find<Button>("PART_WinMinimize");
        _winMaximize = e.NameScope.Find<Button>("PART_WinMaximize");

        _linClose = e.NameScope.Find<Button>("PART_LinClose");
        _linMinimize = e.NameScope.Find<Button>("PART_LinMinimize");
        _linMaximize = e.NameScope.Find<Button>("PART_LinMaximize");

        _resizeBorders[0] = e.NameScope.Find<Border>("PART_ResizeNW");
        _resizeBorders[1] = e.NameScope.Find<Border>("PART_ResizeN");
        _resizeBorders[2] = e.NameScope.Find<Border>("PART_ResizeNE");
        _resizeBorders[3] = e.NameScope.Find<Border>("PART_ResizeE");
        _resizeBorders[4] = e.NameScope.Find<Border>("PART_ResizeSE");
        _resizeBorders[5] = e.NameScope.Find<Border>("PART_ResizeS");
        _resizeBorders[6] = e.NameScope.Find<Border>("PART_ResizeSW");
        _resizeBorders[7] = e.NameScope.Find<Border>("PART_ResizeW");

        HookTemplateEvents();
        UpdateTitleBarVisibility();
    }

    private void HookTemplateEvents()
    {
        if (_titleBar is not null)
            _titleBar.PointerPressed += OnTitleBarPointerPressed;

        if (_winClose is not null)
            _winClose.Click += CloseWindow;
        if (_linClose is not null)
            _linClose.Click += CloseWindow;

        if (_winMinimize is not null)
            _winMinimize.Click += MinimizeWindow;
        if (_linMinimize is not null)
            _linMinimize.Click += MinimizeWindow;

        if (_winMaximize is not null)
            _winMaximize.Click += ToggleMaximizeWindow;
        if (_linMaximize is not null)
            _linMaximize.Click += ToggleMaximizeWindow;

        foreach (var border in _resizeBorders)
        {
            if (border is not null)
                border.PointerPressed += OnResizeBorderPressed;
        }
    }

    private void UnhookTemplateEvents()
    {
        if (_titleBar is not null)
            _titleBar.PointerPressed -= OnTitleBarPointerPressed;

        if (_winClose is not null)
            _winClose.Click -= CloseWindow;
        if (_linClose is not null)
            _linClose.Click -= CloseWindow;

        if (_winMinimize is not null)
            _winMinimize.Click -= MinimizeWindow;
        if (_linMinimize is not null)
            _linMinimize.Click -= MinimizeWindow;

        if (_winMaximize is not null)
            _winMaximize.Click -= ToggleMaximizeWindow;
        if (_linMaximize is not null)
            _linMaximize.Click -= ToggleMaximizeWindow;

        foreach (var border in _resizeBorders)
        {
            if (border is not null)
                border.PointerPressed -= OnResizeBorderPressed;
        }
    }

    private void UpdateTitleBarVisibility()
    {
        if (_titleBar is not null)
            _titleBar.IsVisible = TitleBarVisible;

        if (_titleText is not null)
            _titleText.IsVisible = TitleBarVisible && !string.IsNullOrWhiteSpace(Title);

        if (_winButtons is not null)
            _winButtons.IsVisible = TitleBarVisible && IsWindows && ButtonsVisible;

        if (_linButtons is not null)
            _linButtons.IsVisible = TitleBarVisible && IsLinux && ButtonsVisible;
    }

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var window = ParentWindow;
        if (window is null || _titleBar is null || !TitleBarVisible)
            return;

        if (!e.GetCurrentPoint(_titleBar).Properties.IsLeftButtonPressed)
            return;

        if (e.Source is Visual source &&
            source.GetSelfAndVisualAncestors().OfType<Button>().Any())
            return;

        window.BeginMoveDrag(e);
        e.Handled = true;
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        ParentWindow?.Close();
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        if (ParentWindow is { } window)
            window.WindowState = WindowState.Minimized;
    }

    private void ToggleMaximizeWindow(object? sender, RoutedEventArgs e)
    {
        if (ParentWindow is not { } window)
            return;

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnResizeBorderPressed(object? sender, PointerPressedEventArgs e)
    {
        var window = ParentWindow;
        if (window is null || window.WindowState == WindowState.Maximized)
            return;

        if (!e.GetCurrentPoint(window).Properties.IsLeftButtonPressed)
            return;

        if (sender is not Control { Tag: string edgeName } ||
            !System.Enum.TryParse(edgeName, out WindowEdge edge))
            return;

        window.BeginResizeDrag(edge, e);
        e.Handled = true;
    }
}