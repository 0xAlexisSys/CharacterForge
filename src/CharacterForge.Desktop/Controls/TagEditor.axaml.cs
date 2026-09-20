using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Material.Icons;
using Material.Icons.Avalonia;
using ItemChip = (Avalonia.Controls.Border Body, Avalonia.Controls.TextBox TextBox, Avalonia.Controls.Button RemoveButton);

namespace CharacterForge.Desktop.Controls;

public sealed class TagEditor : TemplatedControl
{
    private const string ClassItemChip = "itemChip";
    private const string ClassRestricted = "restricted";

    public static readonly DirectProperty<TagEditor, AvaloniaList<string>?> ItemsProperty = AvaloniaProperty.RegisterDirect<TagEditor, AvaloniaList<string>?>(nameof(Items),
                                                                                                                                                              getter: static owner => owner.Items,
                                                                                                                                                              setter: static (owner, value) => owner.Items = value,
                                                                                                                                                              defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<int> MaxLinesProperty = AvaloniaProperty.Register<TagEditor, int>(nameof(MaxLines), defaultValue: 2);
    public static readonly StyledProperty<bool> EnsureMaxHeightProperty = AvaloniaProperty.Register<TagEditor, bool>(nameof(EnsureMaxHeight), defaultValue: false);

    public AvaloniaList<string>? Items { get; set => SetAndRaise(ItemsProperty, ref field, value); }
    public int MaxLines { get => GetValue(MaxLinesProperty); set => SetValue(MaxLinesProperty, value); }
    public bool EnsureMaxHeight { get => GetValue(EnsureMaxHeightProperty); set => SetValue(EnsureMaxHeightProperty, value); }

    private ScrollViewer? _itemsScrollViewer;
    private WrapPanel? _itemsPanel;
    private Button? _addItemButton;
    private readonly Dictionary<string, ItemChip> _itemChips = [];

    protected override void OnLoaded(RoutedEventArgs args)
    {
        LayoutUpdated += OnLayoutUpdated;
        Items?.CollectionChanged += OnItemsCollectionChanged;
    }

    protected override void OnUnloaded(RoutedEventArgs args)
    {
        LayoutUpdated -= OnLayoutUpdated;
        Items?.CollectionChanged -= OnItemsCollectionChanged;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs args)
    {
        _itemsScrollViewer = args.NameScope.Find<ScrollViewer>("PART_ItemsScrollViewer");

        _itemsPanel = args.NameScope.Find<WrapPanel>("PART_ItemsPanel");
        UpdateMaximumHeight();

        _addItemButton?.Click -= OnAddItemButtonClick;
        _addItemButton = args.NameScope.Find<Button>("PART_AddItemButton");
        _addItemButton?.Click += OnAddItemButtonClick;

        if (Items is not null)
        {
            foreach (string item in Items) AddItemChip(item);
            _addItemButton?.IsEnabled = true;
        }
        else
        {
            _addItemButton?.IsEnabled = false;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        base.OnPropertyChanged(args);
        if (args.Property == ItemsProperty)
        {
            _addItemButton?.IsEnabled = Items is not null;
        }
        else if (args.Property == MaxLinesProperty)
        {
            UpdateMaximumHeight();
        }
    }

    private void UpdateMaximumHeight()
    {
        if (_itemsScrollViewer is null) return;

        if (MaxLines <= 0 || _itemsPanel is null)
        {
            _itemsScrollViewer.MaxHeight = double.PositiveInfinity;
            return;
        }

        if (!EnsureMaxHeight)
        {
            List<double> lineBottoms = [];
            foreach (Control item in _itemsPanel.Children)
            {
                if (!item.IsVisible) continue;

                double bottom = item.Bounds.Bottom;
                if (lineBottoms.Count == 0 || item.Bounds.Top > lineBottoms[^1])
                {
                    lineBottoms.Add(bottom);
                }
                else if (bottom > lineBottoms[^1])
                {
                    lineBottoms[^1] = bottom;
                }
            }

            _itemsScrollViewer.MaxHeight = lineBottoms.Count > MaxLines ? lineBottoms[MaxLines - 1] : double.PositiveInfinity;
        }
        else
        {
            // ReSharper disable once ArrangeRedundantParentheses
            double newHeight = (_itemsPanel.Children.FirstOrDefault(static child => child.IsVisible)?.Bounds.Bottom * MaxLines) ?? double.NaN;
            if (!double.IsNaN(newHeight)) newHeight += double.Max(_itemsPanel.LineSpacing * (MaxLines - 1), 0.0D);
            _itemsScrollViewer.Height = newHeight;
        }
    }

    private void AddNewItem()
    {
        if (Items is null) return;

        Items.Add(string.Empty);
        StartItemChipEdit(string.Empty);
        if (MaxLines > 0 && _itemsScrollViewer is not null)
        {
            Dispatcher.UIThread.Post(() => _itemsScrollViewer.ScrollToEnd());
        }
    }

    private void AddItemChip(string item)
    {
        if (_itemsPanel is null) return;

        TextBox itemChipTextBox = new()
        {
            Tag = item,
            IsEnabled = false,
            Text = item,
        };

        Button itemChipRemoveButton = new()
        {
            Tag = item,
            Classes =
            {
                "icon",
                "iconRemove",
            },
            Content = new MaterialIcon
            {
                Kind = MaterialIconKind.Minus,
                IconSize = 12.0D,
            },
            Width = 16.0D,
            Height = 16.0D,
            Cursor = new(StandardCursorType.Arrow), // Stops the hand cursor from appearing when hovering over the remove button.
        };
        itemChipRemoveButton.Click += OnItemChipRemoveButtonClick;

        Border itemChipBody = new()
        {
            Tag = item,
            Classes = {ClassItemChip},
            Cursor = new(StandardCursorType.Hand),
            Child = new StackPanel
            {
                Children =
                {
                    itemChipTextBox,
                    itemChipRemoveButton,
                },
            },
        };
        itemChipBody.PointerPressed += OnItemChipPointerPressed;

        // The new item chip is inserted before PART_AddItemButton in PART_ItemsPanel.
        _itemChips.Add(item, (itemChipBody, itemChipTextBox, itemChipRemoveButton));
        _itemsPanel.Children.Insert(_itemsPanel.Children.Count - 1, _itemChips[item].Body);
    }

    private void RemoveItemChip(string item)
    {
        if (_itemsPanel is null) return;

        ItemChip itemChip = _itemChips[item];
        itemChip.Body.PointerPressed -= OnItemChipPointerPressed;
        itemChip.TextBox.TextChanged -= OnItemChipTextBoxTextChanged;
        itemChip.TextBox.KeyDown -= OnItemChipTextBoxKeyDown;
        itemChip.TextBox.LostFocus -= OnItemChipTextBoxLostFocus;
        itemChip.RemoveButton.Click -= OnItemChipRemoveButtonClick;
        _itemsPanel.Children.Remove(itemChip.Body);
        _itemChips.Remove(item);
    }

    private void StartItemChipEdit(string item)
    {
        if (_itemsPanel is null) return;

        ItemChip itemChip = _itemChips[item];
        itemChip.TextBox.TextChanged += OnItemChipTextBoxTextChanged;
        itemChip.TextBox.KeyDown += OnItemChipTextBoxKeyDown;
        itemChip.TextBox.LostFocus += OnItemChipTextBoxLostFocus;
        itemChip.TextBox.IsEnabled = true;
        itemChip.TextBox.Focus();
        itemChip.TextBox.SelectAll();
    }

    private void EndItemChipEdit(string item)
    {
        if (_itemsPanel is null) return;

        ItemChip itemChip = _itemChips[item];
        itemChip.TextBox.TextChanged -= OnItemChipTextBoxTextChanged;
        itemChip.TextBox.KeyDown -= OnItemChipTextBoxKeyDown;
        itemChip.TextBox.LostFocus -= OnItemChipTextBoxLostFocus;
        itemChip.TextBox.IsEnabled = false;
        itemChip.TextBox.Text = item;
    }

    private void OnLayoutUpdated(object? sender, EventArgs args) => UpdateMaximumHeight();

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (_itemsPanel is null) return;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (string item in args.NewItems!) AddItemChip(item);
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (string item in args.OldItems!) RemoveItemChip(item);
                break;
            case NotifyCollectionChangedAction.Replace:
                for (int i = 0; i < args.NewItems!.Count; i++)
                {
                    string oldItem = (string)args.OldItems![i]!;
                    string newItem = (string)args.NewItems![i]!;

                    ItemChip itemChip = _itemChips[oldItem];
                    itemChip.Body.Tag = newItem;
                    itemChip.TextBox.Tag = newItem;
                    itemChip.TextBox.Text = newItem;
                    itemChip.RemoveButton.Tag = newItem;
                    _itemChips.Add(newItem, (itemChip.Body, itemChip.TextBox, itemChip.RemoveButton));
                    _itemChips.Remove(oldItem);
                }

                break;
            case NotifyCollectionChangedAction.Move:
                _itemsPanel.Children.Move(args.OldStartingIndex, args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                // Calling RemoveItemChip() on each item chip also cleans up events while preserving
                // PART_AddItemButton.
                foreach (KeyValuePair<string, ItemChip> pair in _itemChips) RemoveItemChip(pair.Key);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnItemChipPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (sender is Border {Tag: string item} && args.Properties.IsLeftButtonPressed)
        {
            args.Handled = true;
            StartItemChipEdit(item);
        }
    }

    private void OnItemChipTextBoxTextChanged(object? sender, TextChangedEventArgs args)
    {
        if (Items is null) return;

        if (sender is TextBox {Tag: string item})
        {
            args.Handled = true;

            ItemChip itemChip = _itemChips[item];

            string newItem = itemChip.TextBox.Text!;
            if (!newItem.IsWhiteSpace())
            {
                if (Items.Contains(newItem) && newItem != item)
                {
                    itemChip.TextBox.Classes.Add(ClassRestricted);
                }
                else
                {
                    itemChip.TextBox.Classes.Remove(ClassRestricted);
                }
            }
        }
    }

    private void OnItemChipTextBoxKeyDown(object? sender, KeyEventArgs args)
    {
        if (Items is null) return;

        if (sender is TextBox {Tag: string item})
        {
            bool TryCommitEdit()
            {
                ItemChip itemChip = _itemChips[item];
                string newItem = itemChip.TextBox.Text!;

                if (Items.Contains(newItem) && newItem != item)
                {
                    itemChip.TextBox.Classes.Add(ClassRestricted);
                    return false;
                }

                if (newItem != item) Items[Items.IndexOf(item)] = newItem;
                EndItemChipEdit(newItem);
                return true;
            }

            switch (args.Key)
            {
                case Key.Enter:
                    args.Handled = true;
                    if (!_itemChips[item].TextBox.Text!.IsWhiteSpace())
                    {
                        TryCommitEdit();
                    }
                    else
                    {
                        Items.Remove(item);
                    }
                    break;
                case Key.Escape:
                    args.Handled = true;
                    if (!item.IsWhiteSpace())
                    {
                        EndItemChipEdit(item);
                    }
                    else
                    {
                        Items.Remove(item);
                    }
                    break;
                case Key.OemComma when args.KeyModifiers == KeyModifiers.Alt:
                    args.Handled = true;
                    if (!_itemChips[item].TextBox.Text!.IsWhiteSpace() && TryCommitEdit()) AddNewItem();
                    break;
            }
        }
    }

    private void OnItemChipTextBoxLostFocus(object? sender, FocusChangedEventArgs args)
    {
        if (Items is null) return;

        if (sender is TextBox {Tag: string item})
        {
            args.Handled = true;
            if (!item.IsWhiteSpace())
            {
                EndItemChipEdit(item);
            }
            else
            {
                Items.Remove(item);
            }
        }
    }

    private void OnItemChipRemoveButtonClick(object? sender, RoutedEventArgs args)
    {
        if (Items is null) return;

        if (sender is Button {Tag: string item})
        {
            args.Handled = true;
            Items.Remove(item);
        }
    }

    private void OnAddItemButtonClick(object? sender, RoutedEventArgs args)
    {
        args.Handled = true;
        AddNewItem();
    }
}
