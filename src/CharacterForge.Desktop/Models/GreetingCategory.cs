using Avalonia.Collections;

namespace CharacterForge.Desktop.Models;

public sealed record class GreetingCategory(bool MinOneElement, string ClearModalMessage)
{
    public AvaloniaList<GreetingItem> Items { get; } = [];
    public GreetingItem? CurrentItem { get => Items.Count != 0 && ItemIndex >= 0 && ItemIndex <= Items.Count - 1 ? Items[ItemIndex] : null; }

    public int ItemIndex = 0;
}
