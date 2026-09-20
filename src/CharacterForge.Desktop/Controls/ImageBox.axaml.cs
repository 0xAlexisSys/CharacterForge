using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace CharacterForge.Desktop.Controls;

public sealed class ImageBox : TemplatedControl
{
    public event EventHandler<object?, Exception>? ImageSetFailed;

    public static readonly StyledProperty<Bitmap?> SourceProperty = AvaloniaProperty.Register<ImageBox, Bitmap?>(nameof(Source), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);
    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<ImageBox, Stretch>(nameof(Stretch), defaultValue: Stretch.Uniform);
    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<ImageBox, double>(nameof(Size), defaultValue: 150.0D);

    private static readonly IReadOnlyList<FilePickerFileType> ImageFileTypes = [FilePickerFileTypes.ImageAll];
    private static readonly Cursor SourceBorderCursor = new(StandardCursorType.Hand);

    public Bitmap? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public Stretch Stretch { get => GetValue(StretchProperty); set => SetValue(StretchProperty, value); }
    public double Size { get => GetValue(SizeProperty); set => SetValue(SizeProperty, value); }

    private Border? _sourceBorder;
    private MenuItem? _chooseContextMenuItem;
    private MenuItem? _clearContextMenuItem;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs args)
    {
        _sourceBorder?.PointerPressed -= OnSourceBorderPointerPressed;
        _sourceBorder = args.NameScope.Find<Border>("PART_SourceBorder");
        _sourceBorder?.Cursor = SourceBorderCursor;
        _sourceBorder?.PointerPressed += OnSourceBorderPointerPressed;

        _chooseContextMenuItem?.Click -= OnChooseContextMenuItemClick;
        _chooseContextMenuItem = args.NameScope.Find<MenuItem>("PART_ChooseContextMenuItem");
        _chooseContextMenuItem?.Click += OnChooseContextMenuItemClick;

        _clearContextMenuItem?.Click -= OnClearContextMenuItemClick;
        _clearContextMenuItem = args.NameScope.Find<MenuItem>("PART_ClearContextMenuItem");
        _clearContextMenuItem?.Click += OnClearContextMenuItemClick;
    }

    private async Task PickImageAsync()
    {
        IReadOnlyList<IStorageFile> files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Open an Image",
            AllowMultiple = false,
            FileTypeFilter = ImageFileTypes,
        });
        if (files.Count == 0) return;
        string path = files[0].Path.LocalPath;

        try
        {
            await using FileStream fileStream = File.OpenRead(path);
            Source = new(fileStream);
        }
        catch (Exception exception)
        {
            ImageSetFailed?.Invoke(this, exception);
        }
    }

    private async void OnSourceBorderPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.Properties.IsLeftButtonPressed) await PickImageAsync();
    }

    private async void OnChooseContextMenuItemClick(object? sender, RoutedEventArgs args) => await PickImageAsync();

    private void OnClearContextMenuItemClick(object? sender, RoutedEventArgs args) => Source = null;
}
