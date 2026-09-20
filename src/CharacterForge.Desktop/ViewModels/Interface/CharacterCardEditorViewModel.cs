using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CharacterForge.Desktop.Extensions;
using CharacterForge.Desktop.Models;
using CharacterForge.Desktop.Services;
using CharacterForge.Infrastructure.CharacterCards;
using CharacterForge.Infrastructure.Lorebooks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using Microsoft.ML.Tokenizers;

namespace CharacterForge.Desktop.ViewModels;

[UsedImplicitly]
public sealed partial class CharacterCardEditorViewModel : ViewModel
{
    private const string TokenCountTextStart = "est. tokens: ";
    private const string ExtensionPrefix = "characterforge_";
    private const string ExtensionIconBytes = $"{ExtensionPrefix}icon_bytes";

    [GeneratedRegex(@"{{user}}|<user>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroUserPattern { get; }

    // {{bot}} is not part of any specifications and appears to be exclusive to Risuai.
    [GeneratedRegex(@"{{char}}|{{bot}}|<char>|<bot>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroCharPattern { get; }

    // While CCv3 defines comment macros as {{comment: A}}, the value and separators
    // are ignored to match SillyTavern's parser behavior. The hidden_key macro is
    // treated like the comment macro for consistency here.
    [GeneratedRegex(@"{{//.*}}|{{hidden_key.*}}|{{comment.*}}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroCommentPattern { get; }

    [GeneratedRegex(@"{{reverse(?:\s+(?<Value>.+)|:{1,2}(?<Value>.+))}}", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MacroReversePattern { get; }

    [GeneratedRegex(@"```plaintext\n(?<Value>.+)\n```", RegexOptions.CultureInvariant | RegexOptions.Singleline)]
    private static partial Regex GeneratedMultiLineTextPattern { get; }

    private static readonly IReadOnlyList<FilePickerFileType> OpenCharacterCardFileTypes =
    [
        new("Character Card")
        {
            Patterns =
            [
                "*.png",
                "*.apng",
                "*.json",
                "*.charx",
            ],
        },
    ];

    private static readonly IReadOnlyList<FilePickerFileType> ExportCharacterCardFileTypes =
    [
        new("PNG") {Patterns = ["*.png"]},
        new("APNG") {Patterns = ["*.apng"]},
        new("JSON") {Patterns = ["*.json"]},
        new("CHARX") {Patterns = ["*.charx"]},
    ];

    private static readonly PngBitmapEncoderOptions IconBitmapEncoderOptions = new() {CompressionLevel = CompressionLevel.NoCompression};

    [ObservableProperty]
    public partial Bitmap? Icon { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameTokenCountText))]
    [NotifyPropertyChangedFor(nameof(CanGenerateDescription))]
    [NotifyPropertyChangedFor(nameof(CanGeneratePersonality))]
    [NotifyPropertyChangedFor(nameof(CanGenerateGreeting))]
    [NotifyPropertyChangedFor(nameof(CanGenerateTags))]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NicknameTokenCountText))]
    public partial string Nickname { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DescriptionTokenCountText))]
    [NotifyPropertyChangedFor(nameof(CanGeneratePersonality))]
    [NotifyPropertyChangedFor(nameof(CanGenerateTags))]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonalityTokenCountText))]
    public partial string Personality { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScenarioTokenCountText))]
    public partial string Scenario { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CurrentGreetingCategoryIndex { get; set; } = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GreetingTokenCountText))]
    public partial string CurrentGreeting { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExampleMessagesTokenCountText))]
    [NotifyPropertyChangedFor(nameof(CanGeneratePersonality))]
    public partial string ExampleMessages { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SystemPromptTokenCountText))]
    public partial string SystemPrompt { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PostHistoryInstructionsTokenCountText))]
    public partial string PostHistoryInstructions { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CreatorName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Version { get; set; } = string.Empty;

    [ObservableProperty]
    public partial LanguageItem CreatorNotesLanguage { get; set; }

    [ObservableProperty]
    public partial string CurrentCreatorNotes { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerateDescription))]
    private partial bool IsGeneratingDescription { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGeneratePersonality))]
    private partial bool IsGeneratingPersonality { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerateGreeting))]
    private partial bool IsGeneratingGreeting { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerateTags))]
    private partial bool IsGeneratingTags { get; set; }

    public AvaloniaList<string> Tags { get; } = [];

    public string NameTokenCountText { get => GetFieldTokenCountText(nameof(Name)); }
    public string NicknameTokenCountText { get => GetFieldTokenCountText(nameof(Nickname)); }
    public string DescriptionTokenCountText { get => GetFieldTokenCountText(nameof(Description)); }
    public string PersonalityTokenCountText { get => GetFieldTokenCountText(nameof(Personality)); }
    public string ScenarioTokenCountText { get => GetFieldTokenCountText(nameof(Scenario)); }

    public string GreetingTokenCountText
    {
        get
        {
            GreetingItem? greetingItem = _greetingCategories[CurrentGreetingCategoryIndex].CurrentItem;
            return greetingItem is not null ? $"{TokenCountTextStart}{greetingItem.Tokens}" : string.Empty;
        }
    }

    public string ExampleMessagesTokenCountText { get => GetFieldTokenCountText(nameof(ExampleMessages)); }
    public string SystemPromptTokenCountText { get => GetFieldTokenCountText(nameof(SystemPrompt)); }
    public string PostHistoryInstructionsTokenCountText { get => GetFieldTokenCountText(nameof(PostHistoryInstructions)); }

    public string GreetingIndexText
    {
        get
        {
            GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
            return $"{int.Min(greetingCategory.ItemIndex + 1, greetingCategory.Items.Count)}/{greetingCategory.Items.Count}";
        }
    }

    public ImmutableArray<LanguageItem> CreatorNotesLanguages { get => LanguageItem.Languages; }
    public bool IsEditingStandardGreetings { get => CurrentGreetingCategoryIndex == 0; }
    public bool IsEditingGroupOnlyGreetings { get => CurrentGreetingCategoryIndex == 1; }
    public bool ShowAddGroupOnlyGreetingPrompt { get => IsEditingGroupOnlyGreetings && _greetingCategories[1].Items.Count == 0; }

    public bool CanGoToPreviousGreeting
    {
        get
        {
            GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
            return greetingCategory.Items.Count != 0 && greetingCategory.ItemIndex > 0;
        }
    }

    public bool CanGoToNextGreeting
    {
        get
        {
            GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
            return greetingCategory.Items.Count != 0 && greetingCategory.ItemIndex < greetingCategory.Items.Count - 1;
        }
    }

    public bool CanRemoveGreeting
    {
        get
        {
            GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
            return greetingCategory.MinOneElement ? greetingCategory.Items.Count > 1 : greetingCategory.Items.Count != 0;
        }
    }

    public bool CanClearGreetings { get => IsEditingStandardGreetings || _greetingCategories[CurrentGreetingCategoryIndex].Items.Count != 0; }
    public bool CanGenerateDescription { get => !Name.IsWhiteSpace() && !IsGeneratingDescription; }
    public bool CanGeneratePersonality { get => !Name.IsWhiteSpace() && (!Description.IsWhiteSpace() || !ExampleMessages.IsWhiteSpace()) && !IsGeneratingPersonality; }
    public bool CanGenerateGreeting { get => !Name.IsWhiteSpace() && !IsGeneratingGreeting; }
    public bool CanGenerateTags { get => !Name.IsWhiteSpace() && !Description.IsWhiteSpace() && !IsGeneratingTags; }

    private readonly Tokenizer _tokenizer;
    private readonly FieldGeneratorService _fieldGeneratorService;
    private readonly DialogService _dialogService;

    private readonly ImmutableArray<GreetingCategory> _greetingCategories =
    [
        new(true, "Are you sure you want to clear all greetings? This will reset the primary greeting and remove all alternate greetings."),
        new(false, "Are you sure you want to clear all group-only greetings?"),
    ];

    private byte[]? _iconBytes;
    private LorebookV3.CoreProperties? _lorebook;
    private readonly Dictionary<string, byte[]> _charxFiles = [];
    private readonly Dictionary<string, string> _creatorNotes = [];
    private readonly Dictionary<string, JsonElement> _extensions = [];

    private readonly Dictionary<string, int> _fieldTokenCounts = new()
    {
        [nameof(Name)] = 0,
        [nameof(Nickname)] = 0,
        [nameof(Description)] = 0,
        [nameof(Personality)] = 0,
        [nameof(Scenario)] = 0,
        [nameof(ExampleMessages)] = 0,
        [nameof(SystemPrompt)] = 0,
        [nameof(PostHistoryInstructions)] = 0,
    };

    /// <remarks>
    /// <c>***</c> is the default example message separator in <i>SillyTavern</i>.
    /// </remarks>
    private static string ApplyMacroExampleMessageStart(string value) => value.Replace("<START>", "***", StringComparison.Ordinal);

    public CharacterCardEditorViewModel(Tokenizer tokenizer, FieldGeneratorService fieldGeneratorService, DialogService dialogService)
    {
        _tokenizer = tokenizer;
        _fieldGeneratorService = fieldGeneratorService;
        _dialogService = dialogService;

        CreatorNotesLanguage = CreatorNotesLanguages[0];
        foreach (GreetingCategory greetingCategory in _greetingCategories.Where(static greetingCategory => greetingCategory.MinOneElement)) greetingCategory.Items.Add(new());
        UpdateGreetingProperties();
    }

    [RelayCommand]
    private async Task OpenCharacterCard()
    {
        string[] paths = await _dialogService.ShowOpenFileDialogAsync(new()
        {
            Title = "Open a Character Card",
            AllowMultiple = false,
            FileTypeFilter = OpenCharacterCardFileTypes,
        });
        if (paths.Length == 0) return;
        string path = paths[0];

        if (CharacterCardFile.IsCharx(path))
        {
            try
            {
                await using (ZipArchive archive = await ZipFile.OpenReadAsync(path))
                {
                    if (archive.Entries.Any(static entry => entry.EncryptionMethod != ZipEncryptionMethod.None))
                    {
                        _dialogService.ShowNotification("CHARX file is encrypted.", NotificationType.Error, title: "Open Failed");
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                _dialogService.ShowNotification($"Cannot open CHARX file: {exception.Message}", NotificationType.Error, title: "Open Failed");
                return;
            }
        }

        try
        {
            CharacterCardInfo characterCardInfo = await CharacterCardFile.LoadAsync(path);

            if (characterCardInfo.SpecificationVersion is {Major: 3U, Minor: > 0U} or {Major: >= 4U}) _dialogService.ShowNotification($"spec_version is {characterCardInfo.SpecificationVersion}. It may have changes not supported by CharacterForge.", NotificationType.Warning);

            _extensions.Clear();
            foreach (KeyValuePair<string, JsonElement> pair in characterCardInfo.Model.Properties.Extensions) _extensions.Add(pair.Key, pair.Value);
            switch (characterCardInfo.Format)
            {
                case CharacterCardFormat.Png:
                    try
                    {
                        SetIconFromBytes(characterCardInfo.IconBytes!);
                    }
                    catch
                    {
                        Icon = null;
                        _dialogService.ShowNotification("Icon failed to load.", NotificationType.Warning);
                    }
                    break;
                case CharacterCardFormat.Json:
                    if (!_extensions.TryGetValue(ExtensionIconBytes, out JsonElement value)) break;

                    if (value.GetString() is not {} encodedIconString)
                    {
                        _dialogService.ShowNotification($"{ExtensionIconBytes} was not valid Base64.", NotificationType.Warning);
                        break;
                    }

                    try
                    {
                        SetIconFromBytes(Convert.FromBase64String(encodedIconString));
                    }
                    catch (ArgumentException)
                    {
                        Icon = null;
                        _dialogService.ShowNotification($"{ExtensionIconBytes} could not be decoded to an image.", NotificationType.Warning);
                    }
                    catch (FormatException)
                    {
                        Icon = null;
                        _dialogService.ShowNotification($"{ExtensionIconBytes} was not valid Base64.", NotificationType.Warning);
                    }
                    break;
                case CharacterCardFormat.Charx:
                    try
                    {
                        if (characterCardInfo.IconBytes is not null)
                        {
                            SetIconFromBytes([..characterCardInfo.IconBytes]);
                        }
                        else
                        {
                            Icon = null;
                        }
                    }
                    catch
                    {
                        Icon = null;
                        _dialogService.ShowNotification("Icon failed to load.", NotificationType.Warning);
                    }

                    if (characterCardInfo.CharxFiles is not null)
                    {
                        foreach (var pair in characterCardInfo.CharxFiles) _charxFiles.TryAdd(pair.Key, pair.Value);
                    }
                    break;
                default:
                    Icon = null;
                    break;
            }
            _extensions.Remove(ExtensionIconBytes);

            Name = characterCardInfo.Model.Properties.Name.ReplaceLineEndingsWithOneWhiteSpace();
            Nickname = characterCardInfo.Model.Properties.Nickname?.ReplaceLineEndingsWithOneWhiteSpace() ?? string.Empty;

            Description = characterCardInfo.Model.Properties.Description;
            Personality = characterCardInfo.Model.Properties.Personality;
            Scenario = characterCardInfo.Model.Properties.Scenario;

            void SetGreetingItemFields(GreetingItem greetingItem, string greeting)
            {
                greetingItem.Text = greeting;
                greetingItem.Tokens = CountTokens(greetingItem.Text, true);
            }

            void AddGreetingItem(int greetingCategoryIndex, string greeting)
            {
                GreetingItem greetingItem = new();
                SetGreetingItemFields(greetingItem, greeting);
                _greetingCategories[greetingCategoryIndex].Items.Add(greetingItem);
            }

            foreach (GreetingCategory greetingCategory in _greetingCategories)
            {
                greetingCategory.Items.Clear();
                if (greetingCategory.MinOneElement) greetingCategory.Items.Add(new());
                greetingCategory.ItemIndex = 0;
            }
            SetGreetingItemFields(_greetingCategories[0].Items[0], characterCardInfo.Model.Properties.FirstGreeting);
            foreach (string greeting in characterCardInfo.Model.Properties.AlternateGreetings) AddGreetingItem(0, greeting);
            foreach (string greeting in characterCardInfo.Model.Properties.GroupOnlyGreetings) AddGreetingItem(1, greeting);
            UpdateGreetingProperties();
            ExampleMessages = characterCardInfo.Model.Properties.ExampleMessages;

            SystemPrompt = characterCardInfo.Model.Properties.SystemPrompt;
            PostHistoryInstructions = characterCardInfo.Model.Properties.PostHistoryInstructions;

            _lorebook = characterCardInfo.Model.Properties.Lorebook;

            _charxFiles.Clear();
            if (characterCardInfo.CharxFiles is not null)
            {
                foreach (var pair in characterCardInfo.CharxFiles) _charxFiles.Add(pair.Key, pair.Value);
            }

            CreatorName = characterCardInfo.Model.Properties.CreatorName.ReplaceLineEndingsWithOneWhiteSpace();
            Version = characterCardInfo.Model.Properties.Version.ReplaceLineEndingsWithOneWhiteSpace();
            _creatorNotes.Clear();
            if (characterCardInfo.Model.Properties.CreatorNotes is not null)
            {
                foreach (var pair in characterCardInfo.Model.Properties.CreatorNotes) _creatorNotes.Add(pair.Key, pair.Value);
            }
            if (_creatorNotes.Count == 0 && characterCardInfo.Model.Properties.LegacyCreatorNotes.Length != 0) _creatorNotes.Add(CreatorNotesLanguages[0].Code, characterCardInfo.Model.Properties.LegacyCreatorNotes);
            LoadCreatorNotesForLanguage(CreatorNotesLanguage.Code);

            Tags.Clear();
            foreach (string tag in characterCardInfo.Model.Properties.Tags.Distinct(StringComparer.Ordinal)) Tags.Add(tag.ReplaceLineEndingsWithOneWhiteSpace());
        }
        catch (Exception exception)
        {
            _dialogService.ShowNotification($"Cannot open character card: {exception.Message}", NotificationType.Error, title: "Open Failed");
        }
    }

    [RelayCommand]
    private async Task ExportCharacterCard()
    {
        string? path = await _dialogService.ShowSaveFileDialogAsync(new()
        {
            Title = "Export Character Card",
            FileTypeChoices = ExportCharacterCardFileTypes,
            ShowOverwritePrompt = true,
        });

        if (path is null) return;

        try
        {
            string pathExtension = Path.GetExtension(path).ToLowerInvariant();
            long currentDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long creationDate = currentDate;

            if (File.Exists(path))
            {
                try
                {
                    CharacterCardV3 modifiedCharacterCard = (await CharacterCardFile.LoadAsync(path)).Model;
                    if (modifiedCharacterCard.Properties.CreationDate.HasValue) creationDate = modifiedCharacterCard.Properties.CreationDate.Value;
                }
                catch
                {
                    // Ignore modified character card load errors.
                }
            }

            if (pathExtension == ".json" && _iconBytes is not null) _extensions.Add(ExtensionIconBytes, JsonSerializer.SerializeToElement(Convert.ToBase64String(_iconBytes)));
            CharacterCardV3 characterCard = new()
            {
                Specification = CharacterCardV3.TypicalSpecification,
                SpecificationVersionString = CharacterCardV3.TypicalSpecificationVersionString,
                Properties = new()
                {
                    Name = Name,
                    Nickname = string.IsNullOrWhiteSpace(Nickname) ? null : Nickname,
                    CreatorName = CreatorName,
                    Version = Version,
                    Personality = Personality,
                    Description = Description,
                    Scenario = Scenario,
                    FirstGreeting = _greetingCategories[0].Items[0].Text,
                    AlternateGreetings = [.._greetingCategories[0].Items.Skip(1).Select(static greeting => greeting.Text)],
                    GroupOnlyGreetings = [.._greetingCategories[1].Items.Select(static greeting => greeting.Text)],
                    ExampleMessages = ExampleMessages,
                    SystemPrompt = SystemPrompt,
                    PostHistoryInstructions = PostHistoryInstructions,
                    Tags = [..Tags],
                    LegacyCreatorNotes = _creatorNotes.TryGetValue("en", out string? value) ? value : string.Empty,
                    CreatorNotes = _creatorNotes.Count != 0 ? new Dictionary<string, string>(_creatorNotes) : null,
                    Lorebook = _lorebook,
                    CreationDate = creationDate,
                    ModificationDate = currentDate,
                    Extensions = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(_extensions)) ?? [],
                },
            };
            _extensions.Remove(ExtensionIconBytes);

            switch (pathExtension)
            {
                case ".png" or ".apng":
                    await CharacterCardFile.SaveToPngAsync(path, characterCard, _iconBytes);
                    break;
                case ".json":
                    await CharacterCardFile.SaveToJsonAsync(path, characterCard);
                    break;
                case ".charx":
                    await CharacterCardFile.SaveToCharxAsync(path, characterCard, _iconBytes, _charxFiles);
                    break;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            _dialogService.ShowNotification($"Failed to export character card to '{path}'", NotificationType.Error);
        }
    }

    [RelayCommand]
    private void Inspect() => _dialogService.ShowModal<InspectModalViewModel>(viewModel =>
    {
        CharacterCardV3 characterCard = new()
        {
            Specification = CharacterCardV3.TypicalSpecification,
            SpecificationVersionString = CharacterCardV3.TypicalSpecificationVersionString,
            Properties = new()
            {
                Name = Name,
                Nickname = string.IsNullOrWhiteSpace(Nickname) ? null : Nickname,
                CreatorName = CreatorName,
                Version = Version,
                Personality = Personality,
                Description = Description,
                Scenario = Scenario,
                FirstGreeting = _greetingCategories[0].Items.Count != 0 ? _greetingCategories[0].Items[0].Text : string.Empty,
                AlternateGreetings = [.._greetingCategories[0].Items.Skip(1).Select(static greeting => greeting.Text)],
                GroupOnlyGreetings = [.._greetingCategories[1].Items.Select(static greeting => greeting.Text)],
                ExampleMessages = ExampleMessages,
                SystemPrompt = SystemPrompt,
                PostHistoryInstructions = PostHistoryInstructions,
                Tags = [..Tags],
                LegacyCreatorNotes = _creatorNotes.TryGetValue("en", out string? value) ? value : string.Empty,
                CreatorNotes = _creatorNotes.Count != 0 ? new Dictionary<string, string>(_creatorNotes) : null,
                Lorebook = _lorebook,
                Extensions = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(JsonSerializer.Serialize(_extensions)) ?? [],
            },
        };

        viewModel.PreviewText = CharacterCardJsonSerializer.Serialize(characterCard);
        viewModel.TotalTokenCount = _fieldTokenCounts.Values.Sum();
        viewModel.StandardGreetingCount = _greetingCategories[0].Items.Count;
        viewModel.GroupOnlyGreetingCount = _greetingCategories[1].Items.Count;
    });

    [RelayCommand]
    private void Reset() => _dialogService.ShowModal<ConfirmModalViewModel>(viewModel =>
    {
        viewModel.Title = "Please Confirm...";
        viewModel.Message = "Reset the character card?";
        viewModel.OnConfirmed = () =>
        {
            Icon = null;
            Name = string.Empty;
            Nickname = string.Empty;
            Description = string.Empty;
            Personality = string.Empty;
            Scenario = string.Empty;
            foreach (GreetingCategory greetingCategory in _greetingCategories)
            {
                greetingCategory.Items.Clear();
                if (greetingCategory.MinOneElement) greetingCategory.Items.Add(new());
                greetingCategory.ItemIndex = 0;
            }
            UpdateGreetingProperties();
            ExampleMessages = string.Empty;
            SystemPrompt = string.Empty;
            PostHistoryInstructions = string.Empty;
            _lorebook = null;
            _charxFiles.Clear();
            CreatorName = string.Empty;
            Version = string.Empty;
            _creatorNotes.Clear();
            CurrentCreatorNotes = string.Empty;
            Tags.Clear();
            _extensions.Clear();

            _dialogService.HideModal();
        };
        viewModel.OnCancelled = _dialogService.HideModal;
    });

    [RelayCommand]
    private void OpenGenerateDescriptionModal() => _dialogService.ShowModal<GenerateDescriptionModalViewModel>(viewModel => viewModel.StartGeneration = (prompt, includePersonality, includeScenario, includeExampleMessages) => GenerateTextForFieldAsync("DescriptionWriter",
                                                                                                                                                                                                                                                           prompt,
                                                                                                                                                                                                                                                           value => Description = value,
                                                                                                                                                                                                                                                           false,
                                                                                                                                                                                                                                                           includePersonality,
                                                                                                                                                                                                                                                           includeScenario,
                                                                                                                                                                                                                                                           includeExampleMessages,
                                                                                                                                                                                                                                                           value => IsGeneratingDescription = value));

    [RelayCommand]
    private void OpenGeneratePersonalityModal() => _dialogService.ShowModal<GeneratePersonalityModalViewModel>(viewModel => viewModel.StartGeneration = (prompt, includeDescription, includeScenario, includeExampleMessages) => GenerateTextForFieldAsync("PersonalityWriter",
                                                                                                                                                                                                                                                           prompt,
                                                                                                                                                                                                                                                           value => Personality = value,
                                                                                                                                                                                                                                                           includeDescription,
                                                                                                                                                                                                                                                           false,
                                                                                                                                                                                                                                                           includeScenario,
                                                                                                                                                                                                                                                           includeExampleMessages,
                                                                                                                                                                                                                                                           value => IsGeneratingPersonality = value));

    [RelayCommand]
    private void OpenGenerateGreetingModal() => _dialogService.ShowModal<GenerateGreetingModalViewModel>(viewModel => viewModel.StartGeneration = (prompt, includeDescription, includePersonality, includeScenario, includeExampleMessages) =>
    {
        GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
        int greetingItemIndex = greetingCategory.ItemIndex;
        return GenerateTextForFieldAsync("GreetingWriter",
                                         prompt,
                                         value =>
                                         {
                                             if (greetingItemIndex > greetingCategory.Items.Count - 1) greetingCategory.Items.Add(new());

                                             GreetingItem greetingItem = greetingCategory.Items[greetingItemIndex];
                                             greetingItem.Text = value;
                                             greetingItem.Tokens = CountTokens(greetingItem.Text, true);
                                             UpdateGreetingProperties();
                                         },
                                         includeDescription,
                                         includePersonality,
                                         includeScenario,
                                         includeExampleMessages,
                                         value => IsGeneratingGreeting = value);
    });

    [RelayCommand]
    private void GoToPreviousGreeting()
    {
        GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
        if (greetingCategory.ItemIndex > 0)
        {
            --greetingCategory.ItemIndex;
            UpdateGreetingProperties();
        }
    }

    [RelayCommand]
    private void GoToNextGreeting()
    {
        GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
        if (greetingCategory.ItemIndex < greetingCategory.Items.Count - 1)
        {
            ++greetingCategory.ItemIndex;
            UpdateGreetingProperties();
        }
    }

    [RelayCommand]
    private void AddGreeting()
    {
        GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
        greetingCategory.Items.Add(new());
        greetingCategory.ItemIndex = greetingCategory.Items.Count - 1;
        UpdateGreetingProperties();
    }

    [RelayCommand]
    private void RemoveGreeting()
    {
        GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
        if (greetingCategory.Items.Count != 0)
        {
            greetingCategory.Items.RemoveAt(greetingCategory.ItemIndex);
            UpdateGreetingProperties();
        }
    }

    [RelayCommand]
    private void ClearGreetings()
    {
        GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
        _dialogService.ShowModal<ConfirmModalViewModel>(viewModel =>
        {
            viewModel.Message = greetingCategory.ClearModalMessage;
            viewModel.OnConfirmed = () =>
            {
                greetingCategory.Items.Clear();
                if (greetingCategory.MinOneElement) greetingCategory.Items.Add(new());
                UpdateGreetingProperties();
                _dialogService.HideModal();
            };
            viewModel.OnCancelled = _dialogService.HideModal;
        });
    }

    [RelayCommand]
    private void ClearTags() => Tags.Clear();

    [RelayCommand]
    private async Task GenerateTagsAsync()
    {
        try
        {
            IsGeneratingTags = true;
            string output = await _fieldGeneratorService.GenerateAsync("Tagger", "TagArray", BuildGenerationInput(true, true, true, true, null));

            string[] newTags = JsonSerializer.Deserialize<string[]>(output) ?? throw new InvalidOperationException("Response did not contain a JSON string array.");
            if (newTags.Length == 0) throw new InvalidOperationException("Response contained an empty JSON string array.");

            Tags.Clear();
            foreach (string tag in newTags.Where(static tag => !tag.IsWhiteSpace())
                                          .Distinct(StringComparer.Ordinal))
            {
                Tags.Add(tag.Trim().ReplaceLineEndingsWithOneWhiteSpace());
            }
        }
        catch (Exception exception)
        {
            _dialogService.ShowNotification(exception.Message, NotificationType.Error, title: "Generation Failed");
        }
        finally
        {
            IsGeneratingTags = false;
        }
    }

    private string ApplyGeneralMacros(string text)
    {
        text = MacroUserPattern.Replace(text, "User");
        text = MacroCharPattern.Replace(text, Nickname.Length == 0 ? Name : Nickname);
        text = MacroCommentPattern.Replace(text, string.Empty);
        text = MacroReversePattern.Replace(text, static match => match.Groups["Value"].Value.ToReverse());
        return text;
    }

    private int CountTokens(string text, bool applyMacros) => _tokenizer.CountTokens(!applyMacros ? text : ApplyGeneralMacros(text));

    private void NotifyMacroCharChanged()
    {
        foreach (GreetingCategory greetingCategory in _greetingCategories)
        {
            foreach (GreetingItem greetingItem in greetingCategory.Items) greetingItem.Tokens = CountTokens(greetingItem.Text, true);
        }
        OnPropertyChanged(nameof(DescriptionTokenCountText));
        OnPropertyChanged(nameof(PersonalityTokenCountText));
        OnPropertyChanged(nameof(ScenarioTokenCountText));
        OnPropertyChanged(nameof(GreetingTokenCountText));
        OnPropertyChanged(nameof(ExampleMessagesTokenCountText));
        OnPropertyChanged(nameof(SystemPromptTokenCountText));
        OnPropertyChanged(nameof(PostHistoryInstructionsTokenCountText));
    }

    private void SetIconFromBytes(byte[] bytes)
    {
        using MemoryStream memoryStream = new(bytes);
        Icon = new(memoryStream);
    }

    private void UpdateGreetingProperties()
    {
        GreetingCategory greetingCategory = _greetingCategories[CurrentGreetingCategoryIndex];
        greetingCategory.ItemIndex = greetingCategory.Items.Count == 0 ? 0 : int.Clamp(greetingCategory.ItemIndex, 0, greetingCategory.Items.Count - 1);

        if (greetingCategory.Items.Count != 0)
        {
            CurrentGreeting = greetingCategory.CurrentItem is not null ? greetingCategory.CurrentItem.Text : string.Empty;
        }
        else
        {
            // CurrentGreeting should not modify anything in this state, so it is used as a
            // visual indicator.
            CurrentGreeting = "Add a greeting to begin editing...";
        }
        OnPropertyChanged(nameof(GreetingIndexText));
        OnPropertyChanged(nameof(IsEditingStandardGreetings));
        OnPropertyChanged(nameof(IsEditingGroupOnlyGreetings));
        OnPropertyChanged(nameof(ShowAddGroupOnlyGreetingPrompt));
        OnPropertyChanged(nameof(CanGoToPreviousGreeting));
        OnPropertyChanged(nameof(CanGoToNextGreeting));
        OnPropertyChanged(nameof(CanRemoveGreeting));
        OnPropertyChanged(nameof(CanClearGreetings));
    }

    private void LoadCreatorNotesForLanguage(string languageCode) => CurrentCreatorNotes = _creatorNotes.TryGetValue(languageCode, out string? text) ? text : string.Empty;

    private string GetFieldTokenCountText(string name) => $"{TokenCountTextStart}{_fieldTokenCounts[name]}";

    private string BuildGenerationInput(bool includeDescription, bool includePersonality, bool includeScenario, bool includeExampleMessages, string? prompt)
    {
        StringBuilder inputBuilder = new();

        void WriteTextBlock(string name, string value)
        {
            if (value.Length != 0) inputBuilder.AppendLine($"{name}:\n```plaintext\n{value}\n```\n");
        }

        inputBuilder.AppendLine("# Character Card\n");
        WriteTextBlock(nameof(Name), Name);
        if (includeDescription) WriteTextBlock(nameof(Description), ApplyGeneralMacros(Description));
        if (includePersonality) WriteTextBlock(nameof(Personality), ApplyGeneralMacros(Personality));
        if (includeScenario) WriteTextBlock(nameof(Scenario), ApplyGeneralMacros(Scenario));
        if (includeExampleMessages) WriteTextBlock("Example Messages", ApplyMacroExampleMessageStart(ApplyGeneralMacros(ExampleMessages)));
        if (!string.IsNullOrEmpty(prompt)) inputBuilder.AppendLine($"---\n\n{prompt}");
        return inputBuilder.ToString().TrimEnd();
    }

    private async Task GenerateTextForFieldAsync(string systemPromptName, string prompt, Action<string> applyOutput, bool includeDescription, bool includePersonality, bool includeScenario, bool includeExampleMessages, Action<bool> setIsGenerating)
    {
        setIsGenerating.Invoke(true);
        try
        {
            Group output = GeneratedMultiLineTextPattern.Match(await _fieldGeneratorService.GenerateAsync(systemPromptName, "MultiLineText", BuildGenerationInput(includeDescription, includePersonality, includeScenario, includeExampleMessages, prompt))).Groups["Value"];
            if (!output.Success || output.ValueSpan.IsWhiteSpace()) throw new InvalidOperationException("Response did not contain text.");

            applyOutput.Invoke(output.Value.Trim());
        }
        finally
        {
            setIsGenerating.Invoke(false);
        }
    }

    partial void OnIconChanged(Bitmap? value)
    {
        if (value is not null)
        {
            using MemoryStream memoryStream = new();
            value.Save(memoryStream, IconBitmapEncoderOptions);
            _iconBytes = memoryStream.ToArray();
        }
        else
        {
            _iconBytes = null;
        }
    }

    partial void OnNameChanged(string value)
    {
        _fieldTokenCounts[nameof(Name)] = CountTokens(value, false);
        if (Nickname.Length == 0) NotifyMacroCharChanged();
    }

    partial void OnNicknameChanged(string value)
    {
        _fieldTokenCounts[nameof(Nickname)] = CountTokens(value, false);
        NotifyMacroCharChanged();
    }

    partial void OnDescriptionChanged(string value) => _fieldTokenCounts[nameof(Description)] = CountTokens(value, true);

    partial void OnPersonalityChanged(string value) => _fieldTokenCounts[nameof(Personality)] = CountTokens(value, true);

    partial void OnScenarioChanged(string value) => _fieldTokenCounts[nameof(Scenario)] = CountTokens(value, true);

    // ReSharper disable once UnusedParameterInPartialMethod
    partial void OnCurrentGreetingCategoryIndexChanged(int value) => UpdateGreetingProperties();

    partial void OnCurrentGreetingChanged(string value)
    {
        GreetingItem? currentItem = _greetingCategories[CurrentGreetingCategoryIndex].CurrentItem;
        if (currentItem is not null)
        {
            currentItem.Text = value;
            currentItem.Tokens = CountTokens(value, true);
        }
    }

    partial void OnExampleMessagesChanged(string value) => _fieldTokenCounts[nameof(ExampleMessages)] = CountTokens(ApplyMacroExampleMessageStart(value), true);

    partial void OnSystemPromptChanged(string value) => _fieldTokenCounts[nameof(SystemPrompt)] = CountTokens(value, true);

    partial void OnPostHistoryInstructionsChanged(string value) => _fieldTokenCounts[nameof(PostHistoryInstructions)] = CountTokens(value, true);

    partial void OnCreatorNotesLanguageChanged(LanguageItem oldValue, LanguageItem newValue)
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (oldValue is not null && CurrentCreatorNotes.Length != 0) _creatorNotes[oldValue.Code] = CurrentCreatorNotes;
        if (newValue is not null) LoadCreatorNotesForLanguage(newValue.Code);
        // ReSharper restore ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    }

    partial void OnCurrentCreatorNotesChanged(string value)
    {
        if (value.Length != 0)
        {
            _creatorNotes[CreatorNotesLanguage.Code] = value;
        }
        else
        {
            _creatorNotes.Remove(CreatorNotesLanguage.Code);
        }
    }
}
