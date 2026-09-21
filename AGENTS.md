# CharacterForge

CharacterForge is a tool for creating Character Cards for use with language model frontends such as SillyTavern. It is
developed with .NET 11.0, C# 15.0, and Avalonia 12.1.2.

## Repository Structure

- `global.json`
- `Directory.Build.props`
- `Directory.Packages.props`
- `src/` - Projects
  - `CharacterForge.Desktop` - Main application
  - `CharacterForge.Infrastructure` - Character Card utilities

## Theme Guidelines

The interface primarily uses a vibrant dark blue palette; icon buttons can use other colors as long as they fit the
vibrant vibes. See `Resources/ThemeResources.axaml` in `CharacterForge.Desktop` for predefined colors/brushes.

## Code Guidelines

- Adhering to the code style is important.
- Utilize DRY/KISS principles.
- New `TemplatedControl` code should be self-contained (fully code-behind).
- Prefer `AvaloniaList<T>` over `ObservableCollection<T>`.
- Use `is null`/`is not null` for null checking.
- Avoid the shorthand `record` for record classes, use `record class`.
