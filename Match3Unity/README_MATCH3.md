# Match3 Unity Module (Royal Match-inspired)

## Folder structure

- Match3Unity/
  - Scripts/
    - Animation/AnimationController.cs
    - Core/GameManager.cs
    - Core/SpecialTileResolver.cs
    - Data/Enums.cs
    - Data/LevelConfig.cs
    - Grid/GridManager.cs
    - Grid/Tile.cs
    - Input/InputManager.cs
    - Level/LevelManager.cs
    - Matching/MatchData.cs
    - Matching/MatchFinder.cs
    - Pooling/TilePool.cs
    - UI/UIManager.cs
  - Resources/Levels/levels.json

## Unity setup (quick)

1. Create Unity project (2023 LTS or newer).
2. Import TextMeshPro essentials.
3. Create Canvas (Screen Space Camera) and EventSystem.
4. Add scene objects:
   - `GameManager`, `GridManager`, `InputManager`, `MatchFinder`, `AnimationController`, `LevelManager`, `TilePool`, `UIManager`, `SpecialTileResolver`.
5. Make a Tile prefab:
   - Root `RectTransform` + `Image` icon + optional highlight child.
   - Attach `Tile.cs` and map icon/highlight refs.
6. Add 6 tile sprites and special sprites (rocket H/V, bomb, color bomb).
7. Assign all serialized references in inspector.
8. Put `levels.json` into `Resources/Levels` and assign to `LevelManager.levelJson`.
9. Hook UI labels/buttons to `UIManager` and `GameManager.Restart()`.
10. Play.

## Design notes

- Swap validation blocks non-matching moves.
- Cascades continue until no matches remain.
- Object pool prevents frequent instantiate/destroy spikes.
- Input supports tap-select and swipe.
- Hint pulse appears after idle delay.

## Art direction (short guide)

- Bright, saturated palette with high value contrast.
- Rounded tile silhouettes; soft highlights and subtle inner shadows.
- Glossy candy finish (top-left specular, soft gradient).
- FX: elastic eases, tiny overshoot on land, punchy pop for matches.
- Special tiles should have unique, readable icons at small sizes.
