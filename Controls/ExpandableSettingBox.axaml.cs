using Avalonia;
using Avalonia.Controls;

namespace Avalonia.Themes.Tahoe.Controls;

public class ExpandableSettingBox : ContentControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<ExpandableSettingBox, bool>(nameof(IsExpanded));

    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<ExpandableSettingBox, object?>(nameof(HeaderContent));

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }
}
