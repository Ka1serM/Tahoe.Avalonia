using Avalonia;
using Avalonia.Controls;

namespace Avalonia.Themes.Tahoe.Controls;

public class GroupBox : ContentControl
{
    public static readonly StyledProperty<Thickness> EdgeThicknessProperty =
        AvaloniaProperty.Register<GroupBox, Thickness>(nameof(EdgeThickness), new Thickness(1));

    static GroupBox()
    {
        FocusableProperty.OverrideDefaultValue<GroupBox>(false);
    }

    public Thickness EdgeThickness
    {
        get => GetValue(EdgeThicknessProperty);
        set => SetValue(EdgeThicknessProperty, value);
    }
}
