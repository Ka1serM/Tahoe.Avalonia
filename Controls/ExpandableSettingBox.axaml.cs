using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Avalonia.Themes.Tahoe.Controls;

public class ExpandableSettingBox : ContentControl
{
    private Border? _headerRoot;

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

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_headerRoot is not null)
        {
            _headerRoot.PointerPressed -= OnHeaderPointerPressed;
            _headerRoot.KeyDown -= OnHeaderKeyDown;
        }

        _headerRoot = e.NameScope.Find<Border>("HeaderRoot");
        if (_headerRoot is null)
            return;

        _headerRoot.PointerPressed += OnHeaderPointerPressed;
        _headerRoot.KeyDown += OnHeaderKeyDown;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_headerRoot is not null)
        {
            _headerRoot.PointerPressed -= OnHeaderPointerPressed;
            _headerRoot.KeyDown -= OnHeaderKeyDown;
            _headerRoot = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    private void OnHeaderPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_headerRoot is null || !e.GetCurrentPoint(_headerRoot).Properties.IsLeftButtonPressed)
            return;

        IsExpanded = !IsExpanded;
        e.Handled = true;
    }

    private void OnHeaderKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Space))
            return;

        IsExpanded = !IsExpanded;
        e.Handled = true;
    }
}
