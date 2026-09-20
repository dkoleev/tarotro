# Tarotro - Unity Game Project

## Project Overview
Tarotro is a 2D battle game built with Unity 6000.6.0f1 that combines tarot card mechanics (Balatro-like) with combat systems. The project follows a clean architecture pattern with dependency injection and reactive programming.

## Build & Run
- Open in Unity 6000.6.0f1
- Scenes: `Assets/Scenes/game.unity` (main), `Assets/Scenes/level_0.unity`
- Platform: Windows (Steam via Facepunch Steamworks SDK)
- No CLI build or test commands — build and play from Unity Editor

## Technology Stack

### Unity Version
- **Unity 6000.6.0f1**
- Universal Render Pipeline (URP)
- Unity Input System

### Key Dependencies
- **VContainer** - Dependency injection framework
- **UniTask** - High-performance async/await integration
- **MessagePipe** - High-performance messaging library
- **DOTween** - Type-safe animation engine for Unity
- **Addressables** - Asset management system
- **Unity Localization** - Multi-language support
- **SuperTiled2Unity** - Tiled map importer

### 2D Features
- 2D Animation & Sprite tools
- Aseprite integration
- Pixel Perfect Camera from URP package
- PSD Importer
- Tilemap & SpriteShape

## Project Structure (Target)

```
Tarotro/
├── Assets/
│   ├── Scripts/                    # C# source code
│   │   ├── Configs/            # Game configuration classes
│   │   ├── DiScopes/           # VContainer lifetime scopes
│   │   ├── Logic/              # Game logic layer
│   │   │   └── Battle/         # Battle system
│   │   │       ├── Enemies/    # Enemy management
│   │   │       ├── Slots/      # Slot machine logic
│   │   │       └── Effects/    # Effect system
│   │   ├── Presenters/         # MVP presenters
│   │   ├── Views/              # View layer
│   │   └── UI/                 # UI components
│   ├── Art/                    # Art assets
│   ├── Configs/                # Scriptable object configs
│   ├── Scenes/                 # Unity scenes
│   ├── Resources/              # Unity resources
│   ├── Localization/           # Localization tables
│   ├── Bundles/                # Addressable prefabs
│   ├── AddressableAssetsData/  # Addressables configuration
│   └── Settings/               # Project settings
├── Packages/                    # Package dependencies
└── ProjectSettings/            # Unity project settings
```

## Development Guidelines

### Reference Games
- Balatro, Slots & Daggers

### Code Style
- Follow C# naming conventions
- Use VContainer's `[Inject]` attribute for dependency injection
- Prefer async/await with UniTask over coroutines
- Use `var` instead of explicit types on variable declarations
- Use DOTween for animations
- Use Avocado.Toolbox.GameLogger instead of Debug. Always use second parameter logCategory in GameLogger. Example: `GameLogger.Info($"Item with guid {guid} unequipped.", "Category");`
- **Data classes should only contain data** - no business logic methods. Classes in `Configs/` folder (like `FightData`, `EnemyData`, `LevelData`) should only have properties and simple getters. All calculation/logic methods belong in `Logic/` layer (managers, models, etc.)
- **`[SerializeField]` naming** - Use camelCase without underscore prefix: `[SerializeField] private float roomWidth;` not `[SerializeField] private float _roomWidth;`
- **Always use braces for single-line expressions** - `if`, `for`, `foreach`, `while` bodies must be wrapped in `{}` even when they contain a single statement

### File Organization
- Keep related functionality in dedicated folders
- Separate concerns: Logic, Presentation, View

## Platform Targets
- Steam (Windows)

## Notes for AI Assistants
- **ALWAYS `git fetch` and `git pull` the latest changes from the remote before reading files or making any modifications.** Never assume the local state is up to date.
- Always preserve VContainer DI patterns when modifying code
- Use UniTask for async operations, not Unity coroutines
- Maintain MVP separation of concerns
- **NEVER create .meta files manually** - Unity auto-generates them with correct GUIDs when the project is opened. After creating new files, remind the user to open Unity before committing so .meta files are generated correctly.
