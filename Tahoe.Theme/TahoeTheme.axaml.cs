using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Avalonia.Themes.Tahoe;

public partial class TahoeTheme : Styles
{
    public TahoeTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
