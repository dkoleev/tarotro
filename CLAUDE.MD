# Tarotro - Unity Game Project

## Project Overview
Abyss is a 2D battle game built with Unity 6000.6.0f1 that combines tarot card mechanics (Balatro like) with combat systems. The project follows a clean architecture pattern with dependency injection and reactive programming.

## Technology Stack

### Unity Version
- **Unity 6000.6.0f1**
- Universal Render Pipeline (URP)
- Unity Input System

### Key Dependencies
- **VContainer** - Dependency injection framework
- **UniTask** - High-performance async/await integration
- **MessagePipe** - High-performance messaging library
- **DoTween** -  is a fast, efficient, fully type-safe object-oriented animation engine for Unity
- **Addressables** - Asset management system
- **Unity Localization** - Multi-language support

### 2D Features
- 2D Animation & Sprite tools
- Aseprite integration
- Pixel Perfect Camera From URP package
- PSD Importer
- Tilemap & Spriteshape

## Project Structure

```
Abyss/
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
│   ├── Configs/                # Scriptable objects configs
│   ├── Scenes/                 # Unity scenes
│   ├── Resources/              # Unity resources
│   ├── Localization/           # Localization tables
│   ├── AddressableAssetsData/  # Addressables configuration
│   └── Settings/               # Project settings
│   └── Bundles/                # Project prefabs
├── Packages/                    # Package dependencies
└── ProjectSettings/            # Unity project settings
```

## Development Guidelines
### Reference projects
- Balatro, Slots & Daggers

### Code Style
- Follow C# naming conventions
- Use VContainer's `[Inject]` attribute for dependency injection
- Prefer async/await with UniTask over coroutines
- Use var instead of int, float etc on variable declaration
- Use DoTween for animations
- Use Avocado.Toolbox.GameLogger instead of Debug. Always use second parameter logCategory in GameLogger. Example: GameLogger.Info($"Item with guid {guid} unequipped.", "Category");
- **Data classes should only contain data** - no business logic methods. Classes in `Configs/` folder (like `FightData`, `EnemyData`, `LevelData`) should only have properties and simple getters. All calculation/logic methods belong in `Logic/` layer (managers, models, etc.)

### File Organization
- Keep related functionality in dedicated folders
- Separate concerns: Logic, Presentation, View

## Platform Targets
- Steam

## Notes for AI Assistants
- Always preserve VContainer DI patterns when modifying code
- Use UniTask for async operations, not Unity coroutines
- Maintain MVP separation of concerns
- **NEVER create .meta files manually** - Unity auto-generates them with correct GUIDs when the project is opened. After creating new files, remind the user to open Unity before committing so .meta files are generated correctly.
