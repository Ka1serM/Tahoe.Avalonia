using Avalonia.Collections;
using Avalonia.Controls;
using FluentIcons.Common;

namespace Avalonia.Themes.Tahoe.Controls;

/// <summary>
/// A GroupBox-based control that provides an editable list with add/remove functionality.
/// </summary>
public class EditableList : ListBox
{
    public static readonly StyledProperty<Symbol> IconProperty =
        AvaloniaProperty.Register<EditableList, Symbol>(nameof(Icon), Symbol.Add);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<EditableList, string>(nameof(Title), "List");

    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<EditableList, string>(nameof(Subtitle), "Add or Remove items");

    public static readonly StyledProperty<object?> AddCommandProperty =
        AvaloniaProperty.Register<EditableList, object?>(nameof(AddCommand));

    public static readonly StyledProperty<object?> RemoveCommandProperty =
        AvaloniaProperty.Register<EditableList, object?>(nameof(RemoveCommand));

    public static readonly StyledProperty<object?> ExtraCommandProperty =
        AvaloniaProperty.Register<EditableList, object?>(nameof(ExtraCommand));

    public static readonly StyledProperty<string?> ExtraCommandLabelProperty =
        AvaloniaProperty.Register<EditableList, string?>(nameof(ExtraCommandLabel));

    public Symbol Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public object? AddCommand
    {
        get => GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public object? RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public object? ExtraCommand
    {
        get => GetValue(ExtraCommandProperty);
        set => SetValue(ExtraCommandProperty, value);
    }

    public string? ExtraCommandLabel
    {
        get => GetValue(ExtraCommandLabelProperty);
        set => SetValue(ExtraCommandLabelProperty, value);
    }

    static EditableList()
    {
        FocusableProperty.OverrideDefaultValue<EditableList>(false);
    }
}
