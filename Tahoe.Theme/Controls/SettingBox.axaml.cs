using Avalonia;
using Avalonia.Controls;
using FluentIcons.Common;

namespace Avalonia.Themes.Tahoe.Controls;

public class SettingBox : ContentControl
{
    public static readonly StyledProperty<Symbol> IconProperty =
        AvaloniaProperty.Register<SettingBox, Symbol>(nameof(Icon), Symbol.Folder);

    public static readonly StyledProperty<string> PathProperty =
        AvaloniaProperty.Register<SettingBox, string>(nameof(Path), "???");

    public static readonly StyledProperty<string> DisplayNameProperty =
        AvaloniaProperty.Register<SettingBox, string>(nameof(DisplayName), "Property");

    public static readonly StyledProperty<bool> ShowIconProperty =
        AvaloniaProperty.Register<SettingBox, bool>(nameof(ShowIcon), true);

    public Symbol Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public string DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public bool ShowIcon
    {
        get => GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }
}
