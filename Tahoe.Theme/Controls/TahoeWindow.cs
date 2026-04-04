using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;

namespace Avalonia.Themes.Tahoe.Controls;

public partial class TahoeWindow : TahoePlainWindow
{
    protected override bool WrapContentInTahoeChrome => false;

    public bool IsLinux => System.OperatingSystem.IsLinux();
    public bool IsWindows => System.OperatingSystem.IsWindows();

    protected void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (e.Source is Control sourceControl &&
            sourceControl.GetSelfAndVisualAncestors().OfType<Button>().Any())
            return;

        BeginMoveDrag(e);
    }

    protected void CloseWindow(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    protected void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    protected void ToggleMaximizeWindow(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    protected void OnResizeBorderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            return;

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        if (sender is not Control { Tag: string edgeName } ||
            !System.Enum.TryParse<WindowEdge>(edgeName, out var edge))
            return;

        BeginResizeDrag(edge, e);
        e.Handled = true;
    }
}
