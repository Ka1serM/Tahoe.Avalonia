using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Avalonia.Themes.Tahoe.Controls;

public sealed class EditableComboBoxTextSubmittedEventArgs(string text) : EventArgs
{
    public string Text { get; } = text;
    public bool Handled { get; set; }
}

public class EditableComboBox : ComboBox
{
    private TextBox? _editableTextBox;
    private bool _isApplyingAutoComplete;
    private bool _isHandlingUserTextChange;

    public event EventHandler<EditableComboBoxTextSubmittedEventArgs>? TextSubmitted;

    public EditableComboBox()
    {
        SelectionChanged += OnSelectionChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_editableTextBox is not null)
        {
            _editableTextBox.KeyDown -= OnEditableTextBoxKeyDown;
            _editableTextBox.TextChanged -= OnEditableTextBoxTextChanged;
        }

        _editableTextBox = e.NameScope.Find<TextBox>("PART_EditableTextBox");
        if (_editableTextBox is null)
            return;

        _editableTextBox.KeyDown += OnEditableTextBoxKeyDown;
        _editableTextBox.TextChanged += OnEditableTextBoxTextChanged;

        SyncTextFromSelectedItem(forceWhileTyping: true);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_editableTextBox is not null)
        {
            _editableTextBox.KeyDown -= OnEditableTextBoxKeyDown;
            _editableTextBox.TextChanged -= OnEditableTextBoxTextChanged;
            _editableTextBox = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_isApplyingAutoComplete)
            return;

        if (change.Property == SelectedItemProperty || change.Property == ItemsSourceProperty)
            SyncTextFromSelectedItem(forceWhileTyping: false);

        if (change.Property == TextProperty)
            SyncEditorTextFromControlText(forceWhileTyping: false);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isApplyingAutoComplete)
            return;

        SyncTextFromSelectedItem(forceWhileTyping: false);
    }

    private void OnEditableTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        SubmitText(_editableTextBox?.Text);
        e.Handled = true;
    }

    private void OnEditableTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        _isHandlingUserTextChange = true;
        try
        {
            if (!IsEditable || !IsTextSearchEnabled || _isApplyingAutoComplete || _editableTextBox is null)
                return;

            if (!_editableTextBox.IsFocused)
                return;

            var text = _editableTextBox.Text;
            if (string.IsNullOrEmpty(text))
                return;

            var selectionStart = _editableTextBox.SelectionStart;
            if (selectionStart <= 0 || selectionStart > text.Length)
                selectionStart = text.Length;

            var userPrefix = text[..selectionStart];
            if (string.IsNullOrEmpty(userPrefix))
                return;

            if (!TryFindItemByPrefix(userPrefix, out _, out var completion))
                return;

            if (string.Equals(completion, text, StringComparison.Ordinal))
                return;

            _isApplyingAutoComplete = true;
            try
            {
                _editableTextBox.Text = completion;
                _editableTextBox.SelectionStart = userPrefix.Length;
                _editableTextBox.SelectionEnd = completion.Length;
            }
            finally
            {
                _isApplyingAutoComplete = false;
            }
        }
        finally
        {
            _isHandlingUserTextChange = false;
        }
    }

    private void SubmitText(string? rawText)
    {
        if (!IsEditable)
            return;

        var text = rawText?.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        if (TryFindItemByExactText(text, out var matchedItem, out var normalizedText))
        {
            SelectedItem = matchedItem;
            if (!string.Equals(Text, normalizedText, StringComparison.Ordinal))
                Text = normalizedText;

            return;
        }

        var args = new EditableComboBoxTextSubmittedEventArgs(text);
        TextSubmitted?.Invoke(this, args);

        if (!args.Handled)
            SelectedItem = null;
    }

    private bool TryFindItemByExactText(string text, out object? matchedItem, out string matchedText)
    {
        foreach (var item in EnumerateItems())
        {
            var itemText = ItemToString(item);
            if (!string.Equals(itemText, text, StringComparison.OrdinalIgnoreCase))
                continue;

            matchedItem = item;
            matchedText = itemText;
            return true;
        }

        matchedItem = null;
        matchedText = text;
        return false;
    }

    private bool TryFindItemByPrefix(string textPrefix, out object? matchedItem, out string matchedText)
    {
        matchedItem = null;
        matchedText = string.Empty;

        foreach (var item in EnumerateItems())
        {
            var itemText = ItemToString(item);
            if (!itemText.StartsWith(textPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            if (matchedItem is null
                || itemText.Length < matchedText.Length
                || itemText.Length == matchedText.Length && string.Compare(itemText, matchedText, StringComparison.OrdinalIgnoreCase) < 0)
            {
                matchedItem = item;
                matchedText = itemText;
            }
        }

        return matchedItem is not null;
    }

    private IEnumerable<object?> EnumerateItems()
    {
        if (ItemsSource is IEnumerable source)
        {
            foreach (var item in source)
                yield return item;

            yield break;
        }

        if (Items is IEnumerable localItems)
        {
            foreach (var item in localItems)
                yield return item;
        }
    }

    private static string ItemToString(object? item) => item?.ToString() ?? string.Empty;

    private void SyncTextFromSelectedItem(bool forceWhileTyping)
    {
        if (SelectedItem is null)
        {
            SyncEditorTextFromControlText(forceWhileTyping);
            return;
        }

        if (!forceWhileTyping && _isHandlingUserTextChange)
            return;

        var selectedText = ItemToString(SelectedItem);
        if (string.Equals(Text, selectedText, StringComparison.Ordinal))
            return;

        _isApplyingAutoComplete = true;
        try
        {
            Text = selectedText;
            if (_editableTextBox is not null)
            {
                _editableTextBox.SelectionStart = selectedText.Length;
                _editableTextBox.SelectionEnd = selectedText.Length;
            }
        }
        finally
        {
            _isApplyingAutoComplete = false;
        }
    }

    private void SyncEditorTextFromControlText(bool forceWhileTyping)
    {
        if (_editableTextBox is null)
            return;

        if (!forceWhileTyping && _isHandlingUserTextChange)
            return;

        var controlText = Text ?? string.Empty;
        if (string.Equals(_editableTextBox.Text, controlText, StringComparison.Ordinal))
            return;

        _isApplyingAutoComplete = true;
        try
        {
            _editableTextBox.Text = controlText;
            _editableTextBox.SelectionStart = controlText.Length;
            _editableTextBox.SelectionEnd = controlText.Length;
        }
        finally
        {
            _isApplyingAutoComplete = false;
        }
    }
}
