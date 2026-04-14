using Avalonia.Controls;

namespace Avalonia.Themes.Tahoe.Controls;

public class GroupBox : ContentControl
{
    static GroupBox()
    {
        FocusableProperty.OverrideDefaultValue<GroupBox>(false);
    }
}
